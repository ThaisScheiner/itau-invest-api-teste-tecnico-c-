using System.Text.Json;
using Confluent.Kafka;
using ItauInvest.Application.Services;

namespace ItauInvest.API.Application.Workers;

public class OperacaoMensagem
{
    public long UsuarioId { get; set; }
    public long AtivoId { get; set; }
}

public class PosicaoWorker : BackgroundService
{
    private readonly ILogger<PosicaoWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConsumerConfig _consumerConfig;
    private readonly string _topic;

    public PosicaoWorker(ILogger<PosicaoWorker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = configuration["KafkaConsumers:PosicaoWorker:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
        _topic = configuration["KafkaConsumers:PosicaoWorker:Topic"];
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // --- CORREÇÃO APLICADA AQUI ---
        // Envolvemos o loop de consumo em uma Task.Run para não bloquear a inicialização da aplicação.
        return Task.Run(() =>
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
            consumer.Subscribe(_topic);
            _logger.LogInformation(">> PosicaoWorker em execução, aguardando mensagens no tópico '{Topic}'.", _topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    var mensagem = result.Message.Value;

                    _logger.LogInformation(">> [PosicaoWorker] Mensagem recebida: {Message}", mensagem);
                    var operacaoInfo = JsonSerializer.Deserialize<OperacaoMensagem>(mensagem, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (operacaoInfo != null && operacaoInfo.UsuarioId > 0 && operacaoInfo.AtivoId > 0)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var posicaoService = scope.ServiceProvider.GetRequiredService<PosicaoService>();
                        posicaoService.RecalcularESalvarPosicaoAsync(operacaoInfo.UsuarioId, operacaoInfo.AtivoId).GetAwaiter().GetResult();
                        _logger.LogInformation(">> [PosicaoWorker] Posição recalculada para Usuário {UsuarioId} e Ativo {AtivoId}", operacaoInfo.UsuarioId, operacaoInfo.AtivoId);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ">> [PosicaoWorker] Erro inesperado.");
                    Task.Delay(5000, stoppingToken).GetAwaiter().GetResult();
                }
            }
        }, stoppingToken);
    }
}
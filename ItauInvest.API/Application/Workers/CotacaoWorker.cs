using System.Text.Json;
using Confluent.Kafka;
using ItauInvest.Application.Services;

namespace ItauInvest.API.Application.Workers;

public class CotacaoMensagem
{
    public string CodigoAtivo { get; set; } = string.Empty;
    public decimal Preco { get; set; }
}

public class CotacaoWorker : BackgroundService
{
    private readonly ILogger<CotacaoWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConsumerConfig _consumerConfig;
    private readonly string _topic;

    public CotacaoWorker(ILogger<CotacaoWorker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = configuration["KafkaConsumers:CotacaoWorker:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
        _topic = configuration["KafkaConsumers:CotacaoWorker:Topic"];
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // --- CORREÇÃO APLICADA AQUI ---
        // Envolvemos o loop de consumo em uma Task.Run para não bloquear a inicialização da aplicação.
        return Task.Run(() =>
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
            consumer.Subscribe(_topic);
            _logger.LogInformation(">> CotacaoWorker em execução, aguardando mensagens no tópico '{Topic}'.", _topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    var mensagem = result.Message.Value;

                    _logger.LogInformation(">> [CotacaoWorker] Mensagem recebida: {Message}", mensagem);
                    var cotacaoInfo = JsonSerializer.Deserialize<CotacaoMensagem>(mensagem, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (cotacaoInfo != null && !string.IsNullOrEmpty(cotacaoInfo.CodigoAtivo))
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var cotacaoService = scope.ServiceProvider.GetRequiredService<CotacaoService>();
                        cotacaoService.SalvarCotacaoAsync(cotacaoInfo.CodigoAtivo, cotacaoInfo.Preco).GetAwaiter().GetResult();
                        _logger.LogInformation(">> [CotacaoWorker] Cotação para {CodigoAtivo} salva com sucesso.", cotacaoInfo.CodigoAtivo);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ">> [CotacaoWorker] Erro inesperado.");
                    Task.Delay(5000, stoppingToken).GetAwaiter().GetResult();
                }
            }
        }, stoppingToken);
    }
}

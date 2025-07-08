using ItauInvest.API.Application.Workers;
using ItauInvest.API.Infrastructure.Data;
using ItauInvest.API.Middleware;
using ItauInvest.Application.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os servi�os ao container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura o DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<InvestDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// --- REGISTRO DOS SERVI�OS ---
// Registra os servi�os da camada de aplica��o
builder.Services.AddScoped<PosicaoService>();
builder.Services.AddScoped<CotacaoService>(); // Adiciona o novo servi�o de cota��o

// --- REGISTRO DOS WORKERS ---
// Registra ambos os workers para rodarem em segundo plano
builder.Services.AddHostedService<PosicaoWorker>();
builder.Services.AddHostedService<CotacaoWorker>();

var app = builder.Build();

// Configura o pipeline de requisi��es HTTP.
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Popula o banco com dados iniciais
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InvestDbContext>();
    DbInitializer.Seed(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Itau Invest API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
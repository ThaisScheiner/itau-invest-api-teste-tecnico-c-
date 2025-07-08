using ItauInvest.API.Application.Workers;
using ItauInvest.API.Infrastructure.Data;
using ItauInvest.API.Middleware;
using ItauInvest.Application.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- CÓDIGO A SER ADICIONADO (INÍCIO) ---
// Define uma política de CORS específica para permitir que o front-end Angular
// que rodará na porta 4200 possa acessar a API.
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200") // A porta padrão do Angular
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});
// --- CÓDIGO A SER ADICIONADO (FIM) ---


// Adiciona os serviços ao container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura o DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<InvestDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// Registra os serviços e workers
builder.Services.AddScoped<PosicaoService>();
builder.Services.AddScoped<CotacaoService>();
builder.Services.AddHostedService<PosicaoWorker>();
builder.Services.AddHostedService<CotacaoWorker>();

var app = builder.Build();

// Configura o pipeline de requisições HTTP.
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

// --- CÓDIGO A SER ADICIONADO ---
// Habilita a política de CORS que definimos acima.
app.UseCors(MyAllowSpecificOrigins);
// --- FIM DO CÓDIGO A SER ADICIONADO ---

app.UseAuthorization();
app.MapControllers();
app.Run();

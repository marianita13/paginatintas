using System;
using System.Reflection;
using API.Extension;
using API.Helpers;
using API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Serilog;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Logging.AddSerilog(logger);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(Assembly.GetEntryAssembly());
builder.Services.ConfigureCors();
builder.Services.AddAplicationServices();
builder.Services.AddJwt(builder.Configuration);
builder.Services.AddScoped<IMezclaService, MezclaService>();

builder.Services.AddDbContext<paginatintasContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("ConexMysql");

    var mysqlBuilder = new MySqlConnectionStringBuilder(connectionString);

Console.WriteLine("========== MYSQL DEBUG ==========");
Console.WriteLine($"MYSQL HOST: {mysqlBuilder.Server}");
Console.WriteLine($"MYSQL PORT: {mysqlBuilder.Port}");
Console.WriteLine($"MYSQL DATABASE: {mysqlBuilder.Database}");
Console.WriteLine($"MYSQL USER: {mysqlBuilder.UserID}");
Console.WriteLine("=================================");

    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0))
    );
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMiddleware<ExceptionMiddleware>();
app.UseStatusCodePagesWithReExecute("/errors/{0}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<paginatintasContext>();
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var _logger = loggerFactory.CreateLogger<Program>();
        _logger.LogError(ex, "Ocurrio un error durante la migracion");
    }
}

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapControllers();
app.Run();

public partial class Program
{
}

// dotnet ef migrations add InitialCreate --project Persistence --startup-project API|x|
// dotnet ef database update --project Persistence --startup-project API

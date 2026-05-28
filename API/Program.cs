using System;
using System.Reflection;
using System.Threading.Tasks;
using API.Extension;
using API.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

//builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(Assembly.GetEntryAssembly());
builder.Services.ConfigureCors();
builder.Services.AddAplicationServices();
builder.Services.AddJwt(builder.Configuration);

builder.Services.AddDbContext<paginatintasContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("ConexMysql");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseStatusCodePagesWithReExecute("/errors/{0}");
// Configure the HTTP request pipeline.
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
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
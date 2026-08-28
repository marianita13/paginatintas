using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Data;

namespace API.Services
{
    /// <summary>
    /// Servicio en segundo plano que revisa el stock cada hora
    /// y envía un correo por cada tinta que esté por debajo del mínimo.
    /// </summary>
    public class StockAlertaService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StockAlertaService> _logger;

        // Cuánto tiempo espera entre cada revisión (1 hora)
        private readonly TimeSpan _intervalo = TimeSpan.FromHours(24);

        // Evitar enviar el mismo correo repetidamente en la misma sesión
        private readonly System.Collections.Generic.HashSet<int> _yaNotificados = new();

        public StockAlertaService(IServiceScopeFactory scopeFactory, ILogger<StockAlertaService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StockAlertaService iniciado.");

            // Primera revisión al arrancar el servidor
            await RevisarStockAsync();

            using var timer = new PeriodicTimer(_intervalo);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RevisarStockAsync();
            }
        }

        private async Task RevisarStockAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context      = scope.ServiceProvider.GetRequiredService<paginatintasContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var tintasBajas = context.TintaBase
                    .Where(t => t.StockActual <= t.StockMinimo_alerta)
                    .ToList();

                foreach (var tinta in tintasBajas)
                {
                    // Solo notificar una vez por sesión del servidor
                    if (_yaNotificados.Contains(tinta.Id)) continue;

                    _logger.LogWarning(
                        "Stock bajo detectado: {Tinta} — {Stock}g (mínimo: {Minimo}g)",
                        tinta.NombreTinta, tinta.StockActual, tinta.StockMinimo_alerta);

                    await emailService.EnviarCorreoStockBajoAsync(
                        tinta.NombreTinta,
                        tinta.StockActual,
                        tinta.StockMinimo_alerta
                    );

                    _yaNotificados.Add(tinta.Id);
                    _logger.LogInformation("Correo enviado para {Tinta}.", tinta.NombreTinta);
                }

                // Limpiar la lista de notificados cada 24 horas
                // reseteando el HashSet para volver a notificar al día siguiente
                if (DateTime.Now.Hour == 0) _yaNotificados.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revisar stock o enviar correos.");
            }
        }
    }
}
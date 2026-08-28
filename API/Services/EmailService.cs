using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCorreoStockBajoAsync(string nombreTinta, decimal stockActual, decimal stockMinimo)
        {
            var settings = _config.GetSection("Email");

            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("Carbolsas - Sistema", settings["From"]));
            // Destinatario leído desde appsettings.json
            mensaje.To.Add(new MailboxAddress("Administrador", settings["To"]));
            mensaje.Subject = $"⚠ Stock bajo: {nombreTinta}";

            mensaje.Body = new TextPart("html")
            {
                Text = $@"
                <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto'>
                  <div style='background:#1B2B6B;padding:24px;border-radius:8px 8px 0 0;text-align:center'>
                    <h1 style='color:#fff;margin:0;font-size:1.4rem'>CARBOLSAS Ltda</h1>
                    <p style='color:rgba(255,255,255,.7);margin:4px 0 0;font-size:.85rem'>Sistema de Gestión de Color</p>
                  </div>
                  <div style='background:#fff;padding:32px;border:1px solid #e0e0e0;border-top:none'>
                    <div style='background:#fff3f3;border-left:4px solid #C8202E;padding:16px;border-radius:4px;margin-bottom:24px'>
                      <h2 style='color:#C8202E;margin:0 0 8px;font-size:1.1rem'>⚠ Alerta de Stock Bajo</h2>
                      <p style='margin:0;color:#555'>Una de las tintas base ha alcanzado el nivel mínimo de alerta.</p>
                    </div>
                    <table style='width:100%;border-collapse:collapse;margin-bottom:24px'>
                      <tr style='background:#f5f5f5'>
                        <td style='padding:12px 16px;font-weight:700;border:1px solid #e0e0e0'>Tinta</td>
                        <td style='padding:12px 16px;border:1px solid #e0e0e0'>{nombreTinta}</td>
                      </tr>
                      <tr>
                        <td style='padding:12px 16px;font-weight:700;border:1px solid #e0e0e0'>Stock actual</td>
                        <td style='padding:12px 16px;border:1px solid #e0e0e0;color:#C8202E;font-weight:700'>{stockActual:F0}g ({stockActual/1000:F2} kg)</td>
                      </tr>
                      <tr style='background:#f5f5f5'>
                        <td style='padding:12px 16px;font-weight:700;border:1px solid #e0e0e0'>Stock mínimo</td>
                        <td style='padding:12px 16px;border:1px solid #e0e0e0'>{stockMinimo:F0}g ({stockMinimo/1000:F2} kg)</td>
                      </tr>
                    </table>
                    <p style='color:#555;font-size:.9rem'>
                      Es momento de realizar una nueva compra de <strong>{nombreTinta}</strong>
                      para evitar interrupciones en la producción.
                    </p>
                  </div>
                  <div style='background:#f5f5f5;padding:16px;border-radius:0 0 8px 8px;text-align:center'>
                    <p style='margin:0;color:#888;font-size:.78rem'>Carbolsas Ltda — Sistema automático de alertas de inventario</p>
                  </div>
                </div>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                settings["Host"],
                int.Parse(settings["Port"] ?? "587"),
                SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(settings["From"], settings["Password"]);
            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);
        }
    }
}
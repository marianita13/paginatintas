using System.Threading.Tasks;

namespace API.Services
{
    public interface IEmailService
    {
        Task EnviarCorreoStockBajoAsync(string nombreTinta, decimal stockActual, decimal stockMinimo);
    }
}
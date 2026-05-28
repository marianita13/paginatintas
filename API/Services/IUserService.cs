using API.Dtos;
using System.Threading.Tasks;

namespace API.Services
{
    public interface IUserService
    {
        Task<string> RegisterAsync(DataUserDto model);
        Task<DataUserDto> GetTokenAsync(LoginDto model);
        Task<DataUserDto> RefreshTokenAsync(string refreshToken);
    }
}
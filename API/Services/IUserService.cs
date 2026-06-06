using API.Dtos;
using System.Threading.Tasks;

namespace API.Services
{
    public interface IUserService
    {
        Task<string> RegisterAsync(RegistroDto model);
        Task<DataUserDto> GetTokenAsync(LoginDto model);
        Task<string> AddRolAsync(AddRolDto model);
        Task<DataUserDto> RefreshTokenAsync(string refreshToken);
    }
}
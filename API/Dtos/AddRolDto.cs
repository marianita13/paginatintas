namespace API.Dtos
{
    public class AddRolDto
    {
        public required string Nombre { get; set; }
        public required string Password { get; set; }
        public required string Rol { get; set; }
    }
}

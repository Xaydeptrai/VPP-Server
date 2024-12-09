namespace vpp_server.Models.Dtos.RequestDtos
{
    public class AdminChangePasswordDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}

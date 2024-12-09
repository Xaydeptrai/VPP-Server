namespace vpp_server.Models.Dtos.RequestDtos
{
    public class ChangePasswordDto
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}

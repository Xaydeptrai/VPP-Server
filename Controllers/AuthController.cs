using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using vpp_server.Models;
using vpp_server.Services;
using vpp_server.Models.Dtos.RequestDtos;
using vpp_server.Models.Dtos.ResponseDtos;
using Microsoft.AspNetCore.Authorization;

namespace vpp_server.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtTokenGenerator _jwtTokenGenerator;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, JwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpPost("register/customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterDto registerDto)
        {
            var user = new ApplicationUser
            {
                Name = registerDto.Name,
                UserName = registerDto.Email,
                Email = registerDto.Email,
                PhoneNumber = registerDto.Phone
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Registration failed", Result = result.Errors });
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            return Ok(new ResponseDto { IsSuccess = true, Message = "Customer registered successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDto registerDto)
        {
            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Name = registerDto.Name,
                Email = registerDto.Email,
                PhoneNumber = registerDto.Phone
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Registration failed", Result = result.Errors });
            }

            await _userManager.AddToRoleAsync(user, "Admin");

            return Ok(new ResponseDto { IsSuccess = true, Message = "Admin registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized(new ResponseDto { IsSuccess = false, Message = "Invalid email or password" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
            {
                return Unauthorized(new ResponseDto { IsSuccess = false, Message = "Invalid email or password" });
            }

            var token = await _jwtTokenGenerator.GenerateJwtToken(user);

            return Ok(new ResponseDto { IsSuccess = true, Message = "Login successful", Result = new { token } });
        }
    }
}

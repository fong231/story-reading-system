using Microsoft.AspNetCore.Mvc;
using BE.Services;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // POST: api/Users/Login
        [HttpPost("Login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginDto dto)
        {
            var user = await _userService.LoginAsync(dto.UsernameOrEmail, dto.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Thông tin đăng nhập không chính xác hoặc tài khoản bị khóa" });
            }

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                email = user.Email,
                isActive = user.IsActive,
                message = "Đăng nhập thành công"
            });
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _userService.GetActiveUsersAsync();
            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUser(int id)
        {
            var userDetails = await _userService.GetUserDetailsAsync(id);
            if (userDetails == null) return NotFound();

            return Ok(userDetails);
        }

        // POST: api/Users/CreateTestUsers
        [HttpPost("CreateTestUsers")]
        public async Task<ActionResult> CreateTestUsers()
        {
            var success = await _userService.CreateTestUsersAsync();
            if (!success)
            {
                return BadRequest(new { message = "Test users đã tồn tại" });
            }

            return Ok(new { message = "Test users created successfully" });
        }
    }

    public class LoginDto
    {
        public string UsernameOrEmail { get; set; }
        public string Password { get; set; }
    }
}

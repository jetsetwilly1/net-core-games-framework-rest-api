using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.Services.Models;

namespace Midwolf.GamesFramework.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "User Creation")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<ApiUser> _signInManager;
        private readonly UserManager<ApiUser> _userManager;

        public UsersController(IUserService userService, UserManager<ApiUser> userManager,
            SignInManager<ApiUser> signInManager)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(NewUser model)
        {
            var apiUser = await _userService.CreateUserAsync(model);

            if (apiUser == null)
                return BadRequest(new { message = "Unable to create tokens." });

            return Ok(apiUser);
        }

        [HttpPost("tokenrefresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshUserTokens(RefreshUserTokens model)
        {
            var tokens = await _userService.RefreshUserTokens(model);

            if (tokens == null)
                return BadRequest(new { message = "Unable to refresh tokens." });

            return Ok(tokens);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAll();
            return Ok(users);
        }

    }
}

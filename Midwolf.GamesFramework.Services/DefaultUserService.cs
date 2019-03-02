using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Midwolf.GamesFramework.Services.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Midwolf.GamesFramework.Services.Models.Db;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Midwolf.GamesFramework.Services.Storage;
using Midwolf.GamesFramework.Services.Interfaces;

namespace Midwolf.GamesFramework.Services
{
    public sealed class DefaultUserService : IUserService
    {
        private readonly AppSettings _appSettings;
        private readonly ApiDbContext _context;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApiUser> _userManager;
        private readonly SignInManager<ApiUser> _signInManager;

        // ROLES
        // SuperUser - me I can do everything and authenticate people
        // Admin - user can do everything apart from authenticate people
        // Public - This user can only 'read' games, entries, players etc would be used on client facing apps.
        // May change these roles and fine grain them to specific controllers.

        public DefaultUserService(IOptions<AppSettings> appSettings, 
            ApiDbContext context, IMapper mapper, ILoggerFactory loggerFactory, 
            UserManager<ApiUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApiUser> signInManager)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<DefaultUserService>();
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<UserTokens> RefreshUserTokens(RefreshUserTokens refreshDto)
        {
            var user = await _userManager.FindByEmailAsync(refreshDto.Email);
            var passwordOk = await _userManager.CheckPasswordAsync(user, refreshDto.Password);

            if (user == null || (!passwordOk))
                return null;

            var tokens = new UserTokens();
            
            var adminRole = user.Id == "1" ? "superuser" : "admin";

            var privateToken = await GenerateTokenAsync(user, adminRole);
            var tokenHandler = new JwtSecurityTokenHandler();
            tokens.Token = tokenHandler.WriteToken(privateToken);

            // generate a public token
            var publicToken = await GenerateTokenAsync(user, "public");
            tokens.PublicToken = tokenHandler.WriteToken(publicToken);

            return tokens;
        }

        public async Task<NewUser> CreateUserAsync(NewUser userDto)
        {
            var apiUser = new ApiUser
            {
                Email = userDto.Email,
                UserName = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName
            };

            var result = await _userManager.CreateAsync(apiUser, userDto.Password);

            if (result.Succeeded)
            {
                var privateToken = await GenerateTokenAsync(apiUser, "admin");
                var tokenHandler = new JwtSecurityTokenHandler();
                userDto.Token = tokenHandler.WriteToken(privateToken);

                // generate a public token
                var publicToken = await GenerateTokenAsync(apiUser, "public");
                userDto.PublicToken = tokenHandler.WriteToken(publicToken);

                // save the tokens.
                await _userManager.UpdateAsync(apiUser);
            }
            
            // remove password before returning
            userDto.Password = null;
            
            return userDto;
        }

        private async Task<SecurityToken> GenerateTokenAsync(ApiUser user, string role)
        {
            var claims = new ClaimsIdentity();

            var roleIdentity = await _roleManager.FindByNameAsync(role);

            var roles = await _roleManager.GetClaimsAsync(roleIdentity);

            claims.AddClaims(roles);

            claims.AddClaim(new Claim(ClaimTypes.Role, role));
            
            claims.AddClaim(new Claim(ClaimTypes.Name, user.Id.ToString()));
            claims.AddClaim(new Claim(ClaimTypes.Email, user.Email.ToString()));

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                //Expires = DateTime.UtcNow.AddYears(5), //it will never expire.
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return token;
        }

        public async Task<IEnumerable<ApiUser>> GetAll()
        {
            throw new NotImplementedException();
            //var d = await _context.Users.ToListAsync();

            //// return users without passwords
            //var query = d.Select(x => { x.Password = null; return x; });

            //// to do map user enttity to user
            //var usersDto = _mapper.Map<IEnumerable<User>>(query);

            //return usersDto;
        }
    }
}

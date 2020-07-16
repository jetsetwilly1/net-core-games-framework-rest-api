using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Hangfire.MySql.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Midwolf.Api.Infrastructure;
using Midwolf.Competitions.Api.Infrastructure;
using Midwolf.GamesFramework.Api.Infrastructure;
using Midwolf.GamesFramework.CompetitionServices;
using Midwolf.GamesFramework.Services;
using Midwolf.GamesFramework.Services.Interfaces;
using Midwolf.GamesFramework.Services.Models;
using Midwolf.GamesFramework.Services.Storage;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Midwolf.GamesFramework.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // this code writes out the errors of model state.
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = new ApiError(actionContext.ModelState);

                    return new BadRequestObjectResult(errors);
                };
            });

            // this add a logger service that can be used on controllers
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });


            var connectionStringsSection = Configuration.GetSection("ConnectionStrings");
            var connString = connectionStringsSection.Get<ConnectionsStrings>();

            // db in mysql
            services.AddDbContext<ApiDbContext>(options => {
                options.UseMySql(connString.Mysql);
                options.UseLazyLoadingProxies();
            });

            // ===== Add Identity ========
            services.AddIdentity<ApiUser, IdentityRole>()
                .AddEntityFrameworkStores<ApiDbContext>()
                .AddDefaultTokenProviders();

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddHttpContextAccessor();

            services.AddMvc(options => {

                //options.ModelBinderProviders.Insert(0, new EventBinderProvider());
                //options.InputFormatters.Clear();
                options.FormatterMappings.SetMediaTypeMappingForFormat("json", "application/json");

            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(opt =>
                {
                    // These should be the defaults, but we can be explicit:
                    opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    opt.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    opt.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
                });


            services.AddHangfire(x => 
            x.UseStorage(
                new MySqlStorage(connString.Mysql, new MySqlStorageOptions() { TablePrefix = "hangfire" })));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Midwolf Solutions Gaming API",
                    Description = "Core gaming API allowing consumers to track games and players entries.",
                    TermsOfService = "None",
                    Contact = new Contact() { Name = "Midwolf Solutions", Email = "support@midwolf.co.uk", Url = "www.solutions.midwolf.co.uk" }
                });

                c.ExampleFilters();

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme { In = "header", Description = "Please enter JWT with Bearer into field", Name = "Authorization", Type = "apiKey" });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> {
                    { "Bearer", Enumerable.Empty<string>() },
                });

                c.DescribeAllEnumsAsStrings();

                c.DocumentFilter<TagDescriptionsDocumentFilter>();

                c.SchemaFilter<SwaggerIgnoreFilter>();

                // for adding sub classes
                c.DocumentFilter<PolymorphismDocumentFilter<IEventRules>>();
                c.SchemaFilter<PolymorphismSchemaFilter<IEventRules>>();

                // for removing unwanted models
                c.DocumentFilter<SwaggerRemoveModelsDocumentFilter>();

                c.EnableAnnotations();
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower()}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                var servicesXmlFile = "midwolf.gamesframework.services.xml";
                var xmlservicesPath = Path.Combine(AppContext.BaseDirectory, servicesXmlFile);
                c.IncludeXmlComments(xmlservicesPath);
            });

            // add the automatic examples for responses here...
            services.AddSwaggerExamplesFromAssemblyOf<GameResponseExample>();
            services.AddSwaggerExamplesFromAssemblyOf<EventResponseExample>();

            services.AddAutoMapper(opt =>
            {
                opt.AllowNullCollections = true; // this will leave null collections alone. like dictionaries
            });

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false, // dont care about expiration dates..
                    RequireExpirationTime = false
                };

                // any exceptions thrown duting authentication are handled by jsonmiddleware..
                x.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // Override the response status code.
                        context.Response.StatusCode = 401;

                        // Emit the WWW-Authenticate header.
                        context.Response.Headers.Append(
                            HeaderNames.WWWAuthenticate,
                            context.Options.Challenge);

                        //if (!string.IsNullOrEmpty(context.Error))
                        //{
                        //    await context.Response.WriteAsync(context.Error);
                        //}

                        //if (!string.IsNullOrEmpty(context.ErrorDescription))
                        //{
                        //    await context.Response.WriteAsync(context.ErrorDescription);
                        //}

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new ApiError("Authentication Failed.")));

                        context.HandleResponse();
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("User Creation", policy =>
                policy.RequireRole("superuser"));

                options.AddPolicy("User Only", policy =>
                {
                    policy.Requirements.Add(new ApiMinimumRequirments(new List<IApiRequirement>
                    {
                        new UserMinimumRequirement(UserRolePermission.Administrator)
                    }));
                });
                options.AddPolicy("Minimal Restriction", policy =>
                {
                    policy.Requirements.Add(new ApiMinimumRequirments(new List<IApiRequirement>
                    {
                        new UserMinimumRequirement(UserRolePermission.Public),
                        new PlayerMinimumRequirement(PlayerRolePermission.ApiBasic)
                    }));
                });
                options.AddPolicy("Administrators", policy =>
                {
                    policy.Requirements.Add(new ApiMinimumRequirments(new List<IApiRequirement>
                    {
                        new UserMinimumRequirement(UserRolePermission.Administrator),
                        new PlayerMinimumRequirement(PlayerRolePermission.Administer)
                    }));
                });

                options.AddPolicy("Moderate", policy =>
                {
                    policy.Requirements.Add(new ApiMinimumRequirments(new List<IApiRequirement>
                    {
                        new UserMinimumRequirement(UserRolePermission.Administrator),
                        new PlayerMinimumRequirement(PlayerRolePermission.Moderate)
                    }));
                });
            });

            // configure DI for application services
            services.AddScoped<IUserService, DefaultUserService>();
            services.AddScoped<IGameService, DefaultGameService>();
            services.AddScoped<IEventService, DefaultEventService>();
            services.AddScoped<IChainService, DefaultChainService>();
            services.AddScoped<IPlayerService, DefaultPlayerService>();
            services.AddScoped<IEntryService, DefaultEntryService>();
            services.AddScoped<IModerateService, DefaultModerateService>();
            services.AddScoped<IRandomDrawEventService, DefaultRandomDrawEventService>();
            services.AddSingleton<IAuthorizationHandler, ApiPermissionsHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, UserManager<ApiUser> userManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                //var dbContext = app.ApplicationServices.GetRequiredService<ApiDbContext>();
                //AddTestData(dbContext);
            }

            app.UseHttpsRedirection();

            var jsonExceptionMiddleware = new JsonExceptionMiddlware(
                app.ApplicationServices.GetRequiredService<IHostingEnvironment>(), app.ApplicationServices.GetRequiredService<ILoggerFactory>());
            app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = jsonExceptionMiddleware.Invoke });

            app.UseMvc();
            app.UseAuthentication();

            var hangFireAuth = new DashboardOptions
            {
                Authorization = new[]
                {
                    new HangFireAuthorization(app.ApplicationServices.GetService<IAuthorizationService>(),
                    app.ApplicationServices.GetService<IHttpContextAccessor>())
                }
            };

            app.UseHangfireServer(
                new BackgroundJobServerOptions
                {
                    WorkerCount = 1 // limit to 1 connection only.
                });

            app.UseHangfireDashboard("/hangfire", options: hangFireAuth);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Midwolf Solutions Gaming API V1");
            });

            //IDentityInitializer.SeedUsers(userManager);
            var initialiseIndentDb = new IdentityDBInitialise();
            initialiseIndentDb.SeedData(app.ApplicationServices.GetService<UserManager<ApiUser>>(),
                app.ApplicationServices.GetService<RoleManager<IdentityRole>>()).Wait();

            //var mediaType = new MediaTypeHeaderValue("application/json");

            //var formatter = new JsonMediaTypeFormatter();
            //formatter.SupportedMediaTypes.Clear();
            //formatter.SupportedMediaTypes.Add(mediaType);

            //var config = new HttpConfiguration();
            //config.Formatters.Clear();
            //config.Formatters.Add(formatter);
        }
    }

    public class TagDescriptionsDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// Ive added this to make sure the controllers are in the correct order.
        /// </summary>
        /// <param name="swaggerDoc"></param>
        /// <param name="context"></param>
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            var eventsDescription = "<p>Once you have created your game container you will need to create your events.</p>" + 
                "<p>Events can be of type submission|moderate|randomdraw and each event must have a start and end date in unix" + 
                " timestamp that doesn't clash. I.E. The events must follow in order.</p>" +
                "<p>Once you have added your events to a game then you can put together your chain by using the chain endpoint.</p>";

            swaggerDoc.Tags = new[] {
            new Tag { Name = "Games", Description = "Create your game container which will store information and maintain state of the game as its running." },
            new Tag { Name = "Events", Description = eventsDescription },
            new Tag { Name = "Chain", Description = "With your game and events in place you need to create the Chain." },
            new Tag { Name = "Moderation", Description = "This resource allows you to moderate any moderation events within your game." },
            new Tag { Name = "Players", Description = "A player must be created before adding an entry to your game." },
            new Tag { Name = "Entries", Description = "Create entries to your game using your players and keep monitor there state." }
        };
        }
    }

    public class CustomClaimTypes
    {
        public const string Permission = "competitionsapi/permission";
    }

    public class IdentityDBInitialise
    {
        private const string _superuserRoleName = "superuser";
        private string _superuserEmail = "stuart.elcocks@gmail.com";
        private string _superuserPassword = "Password01!";

        private string[] _defaultRoles = new string[] { _superuserRoleName, "administrator", "public", "apibasic", "registered", "administrate", "moderate", "judge" };

        private RoleManager<IdentityRole> _roleManager;
        private UserManager<ApiUser> _userManager;

        public async Task SeedData(UserManager<ApiUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;

            await Initialize();
        }

        public async Task Initialize()
        {
            await EnsureRoles();
            await EnsureDefaultUser();
        }

        protected async Task EnsureRoles()
        {
            foreach (var role in _defaultRoles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var succeed = await _roleManager.CreateAsync(new IdentityRole(role));

                    if (succeed.Succeeded)
                    {
                        var roleIdentity = await _roleManager.FindByNameAsync(role);
                        if (role == "superuser")
                        {
                            // now add role permissions
                            //await _roleManager.AddClaimAsync(roleIdentity, new Claim(CustomClaimTypes.Permission, "all.view"));
                            //await _roleManager.AddClaimAsync(roleIdentity, new Claim(CustomClaimTypes.Permission, "all.create"));
                            //await _roleManager.AddClaimAsync(roleIdentity, new Claim(CustomClaimTypes.Permission, "all.update"));
                            //await _roleManager.AddClaimAsync(roleIdentity, new Claim(CustomClaimTypes.Permission, "all.delete"));
                        }

                        if (role == "administrator")
                        {
                            // now add role permissions
                            //await _roleManager.AddClaimAsync(roleIdentity, new Claim(CustomClaimTypes.Permission, "games.view"));
                            //await _roleManager.AddClaimAsync(roleIdentity, new Claim(CustomClaimTypes.Permission, "games.create"));
                            //await _roleManager.AddClaimAsync(roleIdentity, new Claim(CustomClaimTypes.Permission, "games.update"));
                            //await _roleManager.AddClaimAsync(roleIdentity, new Claim(CustomClaimTypes.Permission, "games.delete"));
                        }

                        if (role == "public")
                        {
                            // now add role permissions
                            //await _roleManager.AddClaimAsync(roleIdentity, new Claim(CustomClaimTypes.Permission, "games.view"));
                        }

                        // PLAYER Roles

                    }
                }
            }
        }

        protected async Task EnsureDefaultUser()
        {
            var superUser = await _userManager.FindByEmailAsync(_superuserEmail);

            if (superUser != null)
            {
                await _userManager.AddPasswordAsync(superUser, _superuserPassword);

                await _userManager.AddToRoleAsync(superUser, _superuserRoleName);
            }
            else
            {
                // create it
                await _userManager.CreateAsync(new ApiUser
                {
                    Id = "1",
                    Email = _superuserEmail,
                    EmailConfirmed = true,
                    UserName = _superuserEmail,
                    FirstName = "Stuart",
                    LastName = "Elcocks"
                });

                var user = await _userManager.FindByEmailAsync(_superuserEmail);

                await _userManager.AddPasswordAsync(user, _superuserPassword);

                await _userManager.AddToRoleAsync(user, _superuserRoleName);
            }
        }
    }
}

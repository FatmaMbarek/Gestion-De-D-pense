//using System.Security.Claims;
//using System.Text;
//using GestionDesDépenses.Models;
//using GestionDesDépenses.Services;
//using GestionDesDépenses.Services.Interfaces;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using Serilog;

//var builder = WebApplication.CreateBuilder(args);

//// Configure Serilog
//Log.Logger = new LoggerConfiguration()
//    .ReadFrom.Configuration(builder.Configuration)
//    .Enrich.FromLogContext()
//    .WriteTo.Console()
//    .WriteTo.File("logs/userlog-.txt", rollingInterval: RollingInterval.Day)
//    .CreateLogger();

//builder.Host.UseSerilog();

//// Add DbContext
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// Add Identity
//builder.Services.AddIdentity<User, IdentityRole>(options =>
//{
//    options.User.RequireUniqueEmail = true;
//})
//.AddEntityFrameworkStores<ApplicationDbContext>()
//.AddDefaultTokenProviders();
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigin", policy =>
//    {
//        policy.WithOrigins("https://localhost:5086", "https://accounts.google.com")
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials();
//    });
//});
//// Add SignInManager for UserService (needed for LogoutAsync and LogoutAllUsersAsync)
//builder.Services.AddScoped<SignInManager<User>>();

//// Add Authentication with OIDC and JWT
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//})
//.AddCookie()
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//    };
//})
//.AddOpenIdConnect("Google", options =>
//{
//    options.Authority = "https://accounts.google.com";
//    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//    options.ResponseType = "code";
//    options.SaveTokens = true;
//    options.CallbackPath = "/signin-google";
//    options.GetClaimsFromUserInfoEndpoint = true;
//    options.Scope.Add("profile");
//    options.Scope.Add("email");
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidIssuer = "https://accounts.google.com",
//        ValidateAudience = true,
//        ValidAudience = builder.Configuration["Authentication:Google:ClientId"],
//        ValidateLifetime = true
//    };
//    options.Events = new OpenIdConnectEvents
//    {
//        OnRedirectToIdentityProvider = context =>
//        {
//            context.ProtocolMessage.RedirectUri = "https://localhost:5086/signin-google";
//            return Task.CompletedTask;
//        },
//        OnTokenValidated = async context =>
//        {
//            var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
//            var claims = context.Principal.Claims;
//            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
//            var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
//            var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;

//            if (!string.IsNullOrEmpty(email))
//            {
//                await userService.EnsureUserExistsAsync(email, firstName, lastName);
//            }
//        }
//    };
//});

//// Add Services
//builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<IEmailService, EmailService>(); // Ensure this is present

//// Add Controllers
//builder.Services.AddControllers();

//// Add Swagger
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

//    // Add OAuth2 configuration for Swagger
//    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
//    {
//        Type = SecuritySchemeType.OAuth2,
//        Flows = new OpenApiOAuthFlows
//        {
//            AuthorizationCode = new OpenApiOAuthFlow
//            {
//                AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/auth"),
//                TokenUrl = new Uri("https://oauth2.googleapis.com/token"),
//                Scopes = new Dictionary<string, string>
//                {
//                    { "openid", "OpenID scope" },
//                    { "profile", "Profile scope" },
//                    { "email", "Email scope" }
//                }
//            }
//        }
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "oauth2"
//                }
//            },
//            new[] { "openid", "profile", "email" }
//        }
//    });
//});

//var app = builder.Build();

//// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
//        c.OAuthClientId(builder.Configuration["Authentication:Google:ClientId"]);
//        c.OAuthUsePkce();
//        c.OAuthScopeSeparator(" ");
//    });
//}

//app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseCors("AllowSpecificOrigin");
//app.UseAuthorization();
//app.MapControllers();
//app.Run();
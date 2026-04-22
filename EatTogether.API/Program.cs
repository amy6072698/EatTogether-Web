
using EatTogether.API.Models.EfModels;
using EatTogether.API.Models.Infra;
using EatTogether.Models.Repositories;
using EatTogether.Models.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EatTogether.API
{
    public class Program  
	{
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

			// 設定 CORS
			builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    var origins = builder.Configuration["AllowedOrigins"]!.Split(",");
                    policy.WithOrigins(origins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // 設定 JWT 認證
            var jwtKey = builder.Configuration["Jwt:SecretKey"]!;
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.Zero  // Token ����ɶ����e�\�~�t
					};

                    // 從 HttpOnly Cookie 取得 Token
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["access_token"];
                            return Task.CompletedTask;
						}
                    };
				});

            builder.Services.AddAuthorization();

            // 設定 Rate Limiting
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("auth", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.PermitLimit = 5;
                    opt.QueueLimit = 0;
                    opt.AutoReplenishment = true;
                });
                options.AddFixedWindowLimiter("general", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.PermitLimit = 60;
                    opt.QueueLimit = 0;
                });
                options.RejectionStatusCode = 429; // Too Many Requests
			});

            // 註冊 DbContext
            builder.Services.AddDbContext<EatTogetherDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 設定 Services
            builder.Services.AddHttpContextAccessor();
			//builder.Services.AddScoped<ITokenService, TokenService>();
			//builder.Services.AddScoped<IAuthService, AuthService>();
			//builder.Services.AddScoped<IMemberService, MemberService>();

			// 設定 6. Repositories
			//builder.Services.AddScoped<IAuthRepository, AuthRepository>();
			//builder.Services.AddScoped<IMemberRepository, MemberRepository>();
			//builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
			// 註冊Repository
			builder.Services.AddScoped<IDishRepository, DishRepository>();
			builder.Services.AddScoped<ISetMealRepository, SetMealRepository>();

			// 註冊Service
			builder.Services.AddScoped<DishService>();
			builder.Services.AddScoped<SetMealService>();


            // 前台寄信服務
            builder.Services.AddScoped<IEmailService, EmailService>();


			// Add services to the container.

			// 排除 class library（EatTogether.dll）的 MVC Controllers，避免與 API Controllers 路由衝突
			builder.Services.AddControllers()
				.ConfigureApplicationPartManager(apm =>
				{
					var libParts = apm.ApplicationParts
						.Where(p => p.Name == "EatTogether")
						.ToList();
					foreach (var part in libParts)
						apm.ApplicationParts.Remove(part);
				});
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // Swashbuckle 預設不支援 DateOnly / TimeOnly，需手動映射
                options.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
                    { Type = "string", Format = "date" });
                options.MapType<DateOnly?>(() => new Microsoft.OpenApi.Models.OpenApiSchema
                    { Type = "string", Format = "date", Nullable = true });
                options.MapType<TimeOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
                    { Type = "string", Format = "time" });
                options.MapType<TimeOnly?>(() => new Microsoft.OpenApi.Models.OpenApiSchema
                    { Type = "string", Format = "time", Nullable = true });

                // class library 內有多個 action 共用同一 method/path，取第一筆避免 Swagger 500
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // 靜態檔案：優先用設定的外部 wwwroot（MVC 專案），找不到再用預設
            var staticRoot = builder.Configuration["StaticFilesRoot"];
            if (!string.IsNullOrEmpty(staticRoot) && Directory.Exists(staticRoot))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(staticRoot)
                });
            }
            else
            {
                app.UseStaticFiles();
            }
            app.UseCors("FrontendPolicy");   // 順序：CORS → RateLimit → Auth
			app.UseRateLimiter();
            app.UseAuthentication();
			app.UseAuthorization();


            app.MapControllers();

            app.Run(); 
        }
    }
}

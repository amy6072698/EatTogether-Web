using EatTogether.Models.EfModels;
using EatTogether.Models.Infra;
using EatTogether.Models.Repositories;
using EatTogether.Models.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EatTogether.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

			// ── CORS ─────────── 
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

            // ── JWT 驗證 ───────────
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
                        ClockSkew = TimeSpan.Zero  // Token 到期時間不容許誤差
					};

                    // 從 HttpOnly Cookie 讀取 Token
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

            // ── Rate Limiting ───────────
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

            // ── DbContext ───────────
            builder.Services.AddDbContext<EatTogetherDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ── Services ───────────
            builder.Services.AddHttpContextAccessor();
            //builder.Services.AddScoped<ITokenService, TokenService>();
            //builder.Services.AddScoped<IAuthService, AuthService>();
            //builder.Services.AddScoped<IMemberService, MemberService>();

            // ── 6. Repositories ────────────────────────────────────
            //builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            //builder.Services.AddScoped<IMemberRepository, MemberRepository>();
            //builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // 註冊Repository
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IDishRepository, DishRepository>();
            builder.Services.AddScoped<ISetMealRepository, SetMealRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ITableRepository, TableRepository>();
            builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
            builder.Services.AddScoped<ICouponRepository, CouponRepository>();
            builder.Services.AddScoped<IMemberCouponRepository, MemberCouponRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IFunctionRepository, FunctionRepository>();
            builder.Services.AddScoped<IRoleFunctionRepository, RoleFunctionRepository>();
            builder.Services.AddScoped<IMemberRepository, MemberRepository>();
            builder.Services.AddScoped<IPreOrderRepository, PreOrderRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IReportRepository, ReportRepository>();
            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<IArticleCategoryRepository, ArticleCategoryRepository>();
            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();

            // 註冊Service
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<DishService>();
            builder.Services.AddScoped<SetMealService>();
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<TableService>();
            builder.Services.AddScoped<ReservationService>();
            builder.Services.AddScoped<CouponService>();
            builder.Services.AddScoped<ReservationEmailService>();
            builder.Services.AddScoped<BirthdayCouponService>();
            builder.Services.AddHostedService<BirthdayCouponBackgroundService>();
            builder.Services.AddHostedService<CouponNotifyBackgroundService>();
            builder.Services.AddSingleton<DishSchedulerService>();
            builder.Services.AddHostedService(sp => sp.GetRequiredService<DishSchedulerService>());
            builder.Services.AddScoped<IAuthService, AuthService>();
            // 💡 註冊使用者編號產生器，解決 UserService 無法啟動的問題
            builder.Services.AddScoped<UserNumberGenerator>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IMemberService, MemberService>();
            builder.Services.AddScoped<IPasswordResetEmailService, PasswordResetEmailService>();
            // 結帳相關
            builder.Services.AddMemoryCache();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<EcPayService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<EventService>();
            builder.Services.AddScoped<ArticleCategoryService>();
            builder.Services.AddScoped<ArticleService>();


            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // --- 加入以下這段 ---
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("http://localhost:5173") // 💡 填寫你前端的網址
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
            // ------------------

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 💡 必須放在 UseRouting 之後，MapControllers 之前
            app.UseCors("AllowAll");

            app.UseHttpsRedirection();
            app.UseCors("FrontendPolicy");   // 順序：CORS → RateLimit → Auth
            app.UseRateLimiter();
            app.UseAuthentication();
			app.UseAuthorization();

            // ── 靜態圖片（從 MVC 專案 wwwroot 目錄 serve） ──────────────
            var mvcWwwroot = builder.Configuration["MvcWwwroot"];
            if (!string.IsNullOrEmpty(mvcWwwroot) && Directory.Exists(mvcWwwroot))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(mvcWwwroot),
                    RequestPath  = ""
                });
            }


            app.MapControllers();

            app.Run();
        }
    }
}

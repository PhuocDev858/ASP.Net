using Microsoft.EntityFrameworkCore;
using TranHuuPhuoc_2123110236.Data;
using TranHuuPhuoc_2123110236.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TranHuuPhuoc_2123110236.Services.OrderServices;

var builder = WebApplication.CreateBuilder(args);

// Thêm Environment Variables vào configuration
builder.Configuration.AddEnvironmentVariables();

// Lấy connection string từ environment hoặc appsettings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Nếu không có từ appsettings, lấy từ environment variable (cho Render)
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("ConnectionStrings:DefaultConnection")
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
}

// Nếu vẫn không có, dùng default (cho development)
connectionString = connectionString ?? "Server=LAPTOP-MP7VACPG\\HUUPHUOC;Database=PCShop_Net8;User Id=SA;Password=Phuocga147;Trusted_Connection=True;TrustServerCertificate=True;";

// Đăng ký SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IEmployeeAuthService, EmployeeAuthService>();
builder.Services.AddScoped<ICustomerAuthService, CustomerAuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? Environment.GetEnvironmentVariable("JwtSettings:Secret")
    ?? "your-super-secret-key-minimum-32-characters-long-here-for-HS256";

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? Environment.GetEnvironmentVariable("JwtSettings:Issuer")
    ?? "TranHuuPhuoc_App";

var jwtAudience = builder.Configuration["JwtSettings:Audience"]
    ?? Environment.GetEnvironmentVariable("JwtSettings:Audience")
    ?? "TranHuuPhuoc_Users";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Thêm CORS nếu cần
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Chỉ show Swagger ở Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Chỉ redirect HTTPS ở Production nếu cần (Render không cần)
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Sử dụng CORS
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Thêm dòng này - để Render biết port để listen
if (app.Environment.IsProduction())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Urls.Add($"http://0.0.0.0:{port}");
}

app.Run();
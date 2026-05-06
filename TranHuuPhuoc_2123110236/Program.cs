using Microsoft.EntityFrameworkCore;
using TranHuuPhuoc_2123110236.Data;
using TranHuuPhuoc_2123110236.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TranHuuPhuoc_2123110236.Services.OrderServices;
using Amazon.S3;
using Amazon;

var builder = WebApplication.CreateBuilder(args);

// Thêm Environment Variables vào configuration (tự động map __ → :)
builder.Configuration.AddEnvironmentVariables();

// Đăng ký SQL Server - tự đọc từ ConnectionStrings__DefaultConnection
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? "Server=LAPTOP-MP7VACPG\\HUUPHUOC;Database=PCShop_Net8;User Id=SA;Password=Phuocga147;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IEmployeeAuthService, EmployeeAuthService>();
builder.Services.AddScoped<ICustomerAuthService, CustomerAuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IVNPayService, VNPayService>();
builder.Services.AddScoped<IVietQRService, VietQRService>();
builder.Services.AddScoped<IPaymentManagementService, PaymentManagementService>();

// JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? "your-super-secret-key-minimum-32-characters-long-here-for-HS256";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "TranHuuPhuoc_App";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "TranHuuPhuoc_Users";

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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Đăng ký AWS S3 Client
var useS3 = bool.Parse(builder.Configuration["FileUpload:UseS3"] ?? "false");
if (useS3)
{
    var awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")
        ?? builder.Configuration["AWS:S3:AccessKey"];
    var awsSecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
        ?? builder.Configuration["AWS:S3:SecretKey"];
    var awsRegion = builder.Configuration["AWS:S3:Region"] ?? "ap-southeast-1";

    var credentials = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
    var config = new AmazonS3Config { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(awsRegion) };
    
    builder.Services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(credentials, config));
}

var app = builder.Build();

// Tạo thư mục uploads nếu chưa tồn tại
var uploadsFolderPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads", "images");
if (!Directory.Exists(uploadsFolderPath))
{
    Directory.CreateDirectory(uploadsFolderPath);
}

// Phục vụ static files (CSS, JS, ảnh, v.v.)
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsProduction())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Urls.Add($"http://0.0.0.0:{port}");
}

app.Run();
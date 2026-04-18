using D.A.sneaker.Data;
using D.A.sneaker.Middleware;
using D.A.sneaker.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ADD CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("allow", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ADD DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// ADD AUTH JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
    new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"]!))
    };

});

builder.Services.AddHttpClient<OllamaService>();
// ADD CONTROLLERS
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();


// SWAGGER + AUTHORIZE BUTTON
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Nhập: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference =
                    new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

// ═══ AUTO FIX IDENTITY SEED ═══════════════════════════════
// Tự động reseed IDENTITY cho tất cả bảng khi app khởi động
// để tránh lỗi "PRIMARY KEY constraint violation"
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    var tables = new[] { "Users", "Products", "ProductImages", "ProductVariants",
        "Orders", "OrderItems", "Payments", "Reviews", "Promotions",
        "Customers", "CartItems", "Wishlists", "Colors", "Sizes", "Category",
        "ChatHistories", "UserChatStates" };
    
    foreach (var table in tables)
    {
        try
        {
            var maxId = db.Database.ExecuteSqlRaw(
                $"IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{table}') " +
                $"BEGIN DECLARE @max INT = (SELECT ISNULL(MAX(Id), 0) FROM [{table}]); " +
                $"DBCC CHECKIDENT('{table}', RESEED, @max); END");
        }
        catch { /* table might not exist yet */ }
    }
    Console.WriteLine("✅ IDENTITY reseed completed for all tables.");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ IDENTITY reseed skipped: {ex.Message}");
}

// MIDDLEWARE (thứ tự quan trọng)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("allow");
app.UseHttpsRedirection();

app.UseAuthentication(); //phải trước Authorization
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMiddleware<ErrorMiddleware>();
app.MapControllers();

app.Run();


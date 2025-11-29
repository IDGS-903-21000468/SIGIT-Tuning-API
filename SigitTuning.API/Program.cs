using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SigitTuning.API.Data;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. CONFIGURAR BASE DE DATOS =====
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSql")));

// ===== 2. CONFIGURAR AUTENTICACIÓN JWT =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// ===== 3. CONFIGURAR CORS (para Android y React) =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===== 4. AGREGAR CONTROLADORES =====
builder.Services.AddControllers();

// ===== 5. CONFIGURAR SWAGGER (Documentación del API) =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SIGIT-Tuning API",
        Version = "v1",
        Description = "API RESTful para la aplicación de tuning automotriz"
    });

    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ===== 6. CONFIGURAR PIPELINE DE PETICIONES =====

// Usar Swagger en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIGIT-Tuning API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Middleware CORS PRIMERO
app.UseCors("AllowAll");

// ===== HABILITAR ARCHIVOS ESTÁTICOS =====
// 1. Raíz wwwroot por defecto
app.UseStaticFiles();

// 2. Carpeta uploads general
var uploadsPath = Path.Combine(builder.Environment.WebRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // Permitir CORS en archivos estáticos
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
    }
});

// 3. Carpeta products específica
var productsPath = Path.Combine(builder.Environment.WebRootPath, "uploads", "products");
if (!Directory.Exists(productsPath))
{
    Directory.CreateDirectory(productsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(productsPath),
    RequestPath = "/uploads/products",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
    }
});

// 4. Carpeta categories específica
var categoriesPath = Path.Combine(builder.Environment.WebRootPath, "uploads", "categories");
if (!Directory.Exists(categoriesPath))
{
    Directory.CreateDirectory(categoriesPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(categoriesPath),
    RequestPath = "/uploads/categories",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
    }
});

// 5. Carpeta avatars específica
var avatarsPath = Path.Combine(builder.Environment.WebRootPath, "uploads", "avatars");
if (!Directory.Exists(avatarsPath))
{
    Directory.CreateDirectory(avatarsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(avatarsPath),
    RequestPath = "/uploads/avatars",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ===== MENSAJE DE INICIO =====
Console.WriteLine("╔════════════════════════════════════════════╗");
Console.WriteLine("║  SIGIT-TUNING API INICIADO                 ║");
Console.WriteLine("║  Puerto: http://localhost:5000             ║");
Console.WriteLine("║  Swagger: http://localhost:5000            ║");
Console.WriteLine("║  Usuarios: http://localhost:5000/api/users ║");
Console.WriteLine("║  Uploads: http://localhost:5000/uploads    ║");
Console.WriteLine("║  Products: http://localhost:5000/uploads/products ║");
Console.WriteLine("╚════════════════════════════════════════════╝");

app.Run();
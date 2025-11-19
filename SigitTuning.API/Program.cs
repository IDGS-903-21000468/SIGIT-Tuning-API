using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
// NUEVO: Imports para servir archivos estáticos
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SigitTuning.API.Data;
using System.IO;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. CONFIGURAR BASE DE DATOS =====
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("CadenaSql")));

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
        c.RoutePrefix = string.Empty; // Swagger en la raíz: http://localhost:5000
    });
}

// Middleware
app.UseCors("AllowAll");

//
// ---> NUEVO: HABILITAR ARCHIVOS ESTÁTICOS <---
//
// 1. Habilita el servicio de archivos estáticos para la carpeta wwwroot (general)
app.UseStaticFiles();

// 2. Habilita específicamente tu carpeta 'uploads' para que sea accesible vía URL
// Mapea la ruta URL "/uploads" a la carpeta física "/wwwroot/uploads"
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "uploads")),
    RequestPath = "/uploads"
});
// ---> FIN DE LÍNEAS NUEVAS <---


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ===== 7. MENSAJE DE INICIO =====
Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║  SIGIT-TUNING API INICIADO               ║");
Console.WriteLine("║  Puerto: http://localhost:5000           ║");
Console.WriteLine("║  Swagger: http://localhost:5000          ║");
Console.WriteLine("╚══════════════════════════════════════════╝");

app.Run();
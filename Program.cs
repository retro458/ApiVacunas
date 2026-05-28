using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ApiVacunas.Data;
using Microsoft.AspNetCore.HttpOverrides;
using QuestPDF.Infrastructure;
using ApiVacunas.Services;
var builder = WebApplication.CreateBuilder(args);

// QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

// CONFIGURACIÓN DE SERVICIOS
builder.Services.AddScoped<ICertificadoPdfService, CertificadoPdfService>();
// 1. BASE DE DATOS (Oracle)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("Oracle")));

// 2. CONFIGURACIÓN DE CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebAppPolicy", policy =>
    {
        policy.WithOrigins("https://admin.nodesv.com", "http://localhost:5173") 
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Indispensable para Cookies
    });
});

// 3. AUTENTICACIÓN JWT CONFIGURADA PARA COOKIES
var jwtKey = builder.Configuration["Jwt:Key"]!;
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Nombre de la cookie, se usa para el controller
            context.Token = context.Request.Cookies["X-Access-Token"];
            return Task.CompletedTask;
        }
    };
});

// 3.5 CONFIGURACIÓN DE SWAGGER ADAPTADA PARA COOKIES
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("CookieAuth", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-Access-Token", 
        In = Microsoft.OpenApi.Models.ParameterLocation.Cookie,
        Description = "Pega directamente tu token JWT aquí (sin la palabra Bearer)"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "CookieAuth"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. CONFIGURACIÓN PARA PROXY (Cloudflare Tunnel)
// Esto arregla los errores del modo "Full" que da claudflared
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 5. PIPELINE (El orden es vital)
app.UseCors("WebAppPolicy");

// Si no esta en desarrollo, Cloudflare ya maneja el HTTPS, 
// pero esto ayuda a .NET a entender el contexto.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

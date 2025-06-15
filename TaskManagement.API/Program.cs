using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using Serilog; 
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Application.Validators;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Infrastructure.Services;

// Configurar Serilog 
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/taskmanagement-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Iniciando Task Management API");

    // Cargar archivo .env
    Env.Load();

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog como el logger principal 
    builder.Host.UseSerilog();

    // Configurar para leer variables de entorno
    builder.Configuration.AddEnvironmentVariables();

    // añadir servicios al contenedor
    builder.Services.AddControllers();

    // Entity Framework Core y SQL Server
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
        ?? builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddDbContext<TaskManagementDbContext>(options =>
        options.UseSqlServer(connectionString));

    // RepositorIOS
    builder.Services.AddScoped<ITaskRepository, TaskRepository>();
    builder.Services.AddScoped<IStateRepository, StateRepository>();

    // Servicios
    builder.Services.AddScoped<ITaskService, TaskService>();    
    builder.Services.AddScoped<IStateService, StateService>();   

    // Autenticación y autorización Servicios
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IJwtService, JwtService>();

    // AutoMapper
    builder.Services.AddAutoMapper(typeof(TaskManagement.Application.Mappings.MappingProfile));

    // FluentValidation 
    builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();

    // JWT 
    var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
        ?? builder.Configuration["Jwt:Key"];
    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
        ?? builder.Configuration["Jwt:Issuer"];
    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
        ?? builder.Configuration["Jwt:Audience"];

    // JWT Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    // Swagger with JWT
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Task Management API", Version = "v1" });
        
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });

    var app = builder.Build();

    // Exception Middleware 
    app.UseMiddleware<TaskManagement.API.Middleware.ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Task Management API iniciada correctamente");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}
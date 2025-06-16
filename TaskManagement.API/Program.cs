
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog; 
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Application.Validators;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Infrastructure.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/taskmanagement-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Iniciando Task Management API");

    Env.Load();

    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.UseUrls("http://localhost:5230");
    builder.Host.UseSerilog();

    // variables de entorno
    builder.Configuration.AddEnvironmentVariables();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy
                .AllowAnyOrigin()       
                .AllowAnyMethod()       
                .AllowAnyHeader();      
        });
    });

    // Controllers
    builder.Services.AddControllers();

    // Entity Framework Core y SQL Server
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
        ?? builder.Configuration.GetConnectionString("DefaultConnection");

    Log.Information("Configurando base de datos...");
    builder.Services.AddDbContext<TaskManagementDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Repositorios
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
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();

    // JWT Configuration
    Log.Information("Configurando JWT...");

    var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

    // Validaciones JWT
    if (string.IsNullOrEmpty(jwtKey))
    {
        Log.Fatal("JWT_KEY no está configurada");
        throw new InvalidOperationException("JWT_KEY es requerida");
    }

    if (jwtKey.Length < 32)
    {
        Log.Fatal("JWT_KEY debe tener al menos 32 caracteres");
        throw new InvalidOperationException("JWT_KEY debe tener al menos 32 caracteres");
    }

    Log.Information("JWT configurado correctamente - Issuer: {Issuer}, Audience: {Audience}", 
        jwtIssuer, jwtAudience);

    // JWT Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "Task Management API", 
            Version = "v1",
            Description = "API para gestión de tareas con autenticación JWT"
        });
        
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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

    //  Middleware 
    app.UseMiddleware<TaskManagement.API.Middleware.ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseCors("AllowAll");

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.MapGet("/health", () => Results.Ok(new { 
        status = "healthy", 
        timestamp = DateTime.UtcNow,
        version = "1.0.0"
    }));

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
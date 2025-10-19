using Microsoft.EntityFrameworkCore;
using ConnectWorkout.Core.Interfaces;
using ConnectWorkout.Infrastructure.Data;
using ConnectWorkout.Infrastructure.Repositories;
using ConnectWorkout.Infrastructure.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ConnectWorkout.API.Middlewares;
using System.Collections.Generic; 

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 🔧 Server Binding Configuration
// ========================================
// Bind to all network interfaces (0.0.0.0) to accept connections from any source
// This is critical for mobile/frontend connections from different hosts
builder.WebHost.UseUrls("http://0.0.0.0:7009");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// ========================================
// 🌐 CORS Configuration for React Native/Expo
// ========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactNative", policy =>
    {
        policy
            .AllowAnyOrigin()      // Allows requests from any origin (mobile devices, emulators, etc.)
            .AllowAnyMethod()      // Allows all HTTP methods (GET, POST, PUT, DELETE, etc.)
            .AllowAnyHeader();     // Allows all headers
    });
});
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
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "ConnectWorkout API", Version = "v1" });
    
    // Adiciona configuração para autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header
            },
            new List<string>()
        }
    });
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStudentInstructorRepository, StudentInstructorRepository>();
builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
builder.Services.AddScoped<IWorkoutDayRepository, WorkoutDayRepository>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
builder.Services.AddScoped<IExerciseStatusRepository, ExerciseStatusRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IInstructorService, InstructorService>();
//builder.Services.AddScoped<IStudentService, StudentService>();
//builder.Services.AddScoped<IWorkoutService, WorkoutService>();
//builder.Services.AddScoped<IProgressService, ProgressService>();

// Configuração do HttpClient para ExerciseDB
builder.Services.AddHttpClient<IExerciseDbService, ExerciseDbService>();

// Necessário para deixar as URLs com letra minúscula
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ========================================
// 📝 Request Logging Middleware (for debugging)
// ========================================
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();

    Console.WriteLine("========================================");
    Console.WriteLine($"🔥 INCOMING REQUEST");
    Console.WriteLine($"   Method: {context.Request.Method}");
    Console.WriteLine($"   Path: {context.Request.Path}");
    Console.WriteLine($"   Origin: {origin}");
    Console.WriteLine($"   Content-Type: {context.Request.ContentType}");
    Console.WriteLine($"   Host: {context.Request.Host}");
    Console.WriteLine("========================================");

    await next();

    Console.WriteLine($"📤 Response Status: {context.Response.StatusCode}");
    Console.WriteLine($"   CORS Header: {context.Response.Headers["Access-Control-Allow-Origin"]}");
    Console.WriteLine("========================================\n");
});

// IMPORTANT: CORS must come BEFORE other middleware
app.UseCors("AllowReactNative");

// Comment out HTTPS redirection for mobile development
// app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🚀 Server is ready to accept requests from React Native/Expo");

app.Run();
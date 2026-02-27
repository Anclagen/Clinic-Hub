
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Backend.Data.Seeding;
using Microsoft.OpenApi;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<AuthService>();

// Fluent validation, and convert properties on errors to camel case so it matches frontend
builder.Services.AddValidatorsFromAssemblyContaining<CreateAppointmentValidator>();
ValidatorOptions.Global.PropertyNameResolver = (_, member, _) =>
    member != null ? JsonNamingPolicy.CamelCase.ConvertName(member.Name) : null;

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            static string NormalizeKey(string key)
            {
                key = key.Replace("$.", "");
                if (key == "dto") return "body";
                return JsonNamingPolicy.CamelCase.ConvertName(key);
            }

            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => NormalizeKey(kvp.Key),
                    kvp => kvp.Value!.Errors.Select(x => x.ErrorMessage).ToArray()
                );

            var errorPayload = new ApiErrorDTO
            {
                StatusCode = 400,
                Message = "Data binding failed.",
                Errors = errors
            };

            return new BadRequestObjectResult(errorPayload);
        };
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ClinicHub API",
        Version = "v1",
    });
    options.DescribeAllParametersInCamelCase();
    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,   // <-- use Http for bearer
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    };
    options.AddSecurityDefinition("Bearer", bearerScheme);
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

//Configure JWT settings
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

//Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8
            .GetBytes(jwtSettings.SecretKey)
        ),
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            // stops the default empty 401 response
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var payload = new ApiErrorDTO
            {
                StatusCode = 401,
                Message = "Unauthorized."
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var payload = new ApiErrorDTO
            {
                StatusCode = 403,
                Message = "Forbidden. You don't have permission to access this resource."
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    };
});

builder.Services.AddAuthorization();

//Add CORS service
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
   {
       builder.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
   });
});

builder.Services.AddScoped<SeedRunner>();
builder.Services.AddScoped<ISeeder, CategorySeeder>();
builder.Services.AddScoped<ISeeder, ClinicSeeder>();
builder.Services.AddScoped<ISeeder, SpecialitySeeder>();
builder.Services.AddScoped<ISeeder, DoctorSeeder>();
builder.Services.AddScoped<ISeeder, AdminSeeder>();

var app = builder.Build();

if (args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();

    await db.Database.MigrateAsync();
    var runner = scope.ServiceProvider.GetRequiredService<SeedRunner>();
    await runner.RunAsync();

    Console.WriteLine("Seeding done.");
    return;
}

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ClinicHub API V1");
        options.RoutePrefix = "doc";
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
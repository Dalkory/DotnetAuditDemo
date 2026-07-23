using System.Text;
using DotnetAuditDemo.Api.BackgroundJobs;
using DotnetAuditDemo.Api.Clients;
using DotnetAuditDemo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Dotnet Audit Demo API",
        Version = "v1",
        Description = "A deliberately flawed API used to demonstrate a professional .NET audit."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
});

var connectionString = builder.Configuration.GetConnectionString("Orders")
    ?? throw new InvalidOperationException("Connection string 'Orders' is missing.");

// Deliberate defect: DbContext is not thread-safe and must not be a singleton.
builder.Services.AddDbContext<DemoDbContext>(
    options => options.UseNpgsql(connectionString),
    contextLifetime: ServiceLifetime.Singleton,
    optionsLifetime: ServiceLifetime.Singleton);

// Deliberate defect: outbound HTTP calls can wait forever.
builder.Services.AddHttpClient<ExchangeRateClient>(client =>
{
    client.Timeout = Timeout.InfiniteTimeSpan;
});

builder.Services.AddHostedService<InvoiceReminderWorker>();

var signingKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("JWT signing key is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "DotnetAuditDemo",
            ValidateAudience = true,
            ValidAudience = "DotnetAuditDemo.Client",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (!app.Configuration.GetValue<bool>("SkipDatabaseInitialization"))
{
    var dbContext = app.Services.GetRequiredService<DemoDbContext>();
    await DemoDataSeeder.SeedAsync(dbContext);
}

app.Run();

public partial class Program;

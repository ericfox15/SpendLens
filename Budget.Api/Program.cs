using Budget.Api.Budget.Attribute;
using Budget.Application.Contracts.Options;
using Budget.Shared.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// ── Serilog ────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

// ── Config validation (fail fast if required settings are missing) ──────────
var connectionString = builder.Configuration.GetConnectionString("BudgetDatabase")
    ?? throw new InvalidOperationException("ConnectionStrings:BudgetDatabase is required in appsettings");

services.Configure<RepositoryOptions>(o => o.BudgetDatabaseConnectionString = connectionString);
services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

services.AddOptions<JwtOptions>().Bind(builder.Configuration.GetSection("Jwt"))
    .ValidateDataAnnotations().ValidateOnStart();

// ── Auth ───────────────────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection["Secret"] ?? "dev-secret-change-in-production-min-32-chars!!";

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"] ?? "budget-helper",
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"] ?? "budget-helper-app",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
services.AddAuthorization();

// ── MVC + Newtonsoft (PascalCase on the wire, matching eMAR) ──────────────
services.AddMvc(options =>
    {
        options.Filters.Add<AuditRequestGlobalAttribute>();
        options.Filters.Add<PermissionGlobalAttribute>();
    })
    .AddNewtonsoftJson(o => o.SerializerSettings.ContractResolver = new DefaultContractResolver());

// ── Swagger ────────────────────────────────────────────────────────────────
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SpendLens API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new()
    {
        [new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>()
    });
});

// ── CORS (Angular dev server) ──────────────────────────────────────────────
services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

// ── DI: project registrations ──────────────────────────────────────────────
services.AddSharedRepository();
services.AddSharedService();

// Filters that use DI must be registered as services
services.AddScoped<AuditRequestGlobalAttribute>();
services.AddScoped<PermissionGlobalAttribute>();

services.AddMemoryCache();
services.AddHealthChecks();

// ── Build ──────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SpendLens v1"));

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

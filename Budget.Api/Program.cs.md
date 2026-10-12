# Program.cs

**Project:** Budget.Api
**Layer:** App Startup / Composition Root

## What it does

Configures and launches the ASP.NET Core 8 web application. This is the **composition root** — the single place where all dependencies are wired together.

## Configuration sections

### Serilog
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/budget-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```
Structured logging to console + rolling daily files. Replaces the default `ILogger<T>` with Serilog.

### Fail-fast validation
```csharp
builder.Services.AddOptions<RepositoryOptions>()
    .BindConfiguration("ConnectionStrings")
    .ValidateDataAnnotations()
    .ValidateOnStart();  // crash immediately if connection string missing
```
If any required config is missing, the app refuses to start with a clear message. Better than crashing on the first DB call at runtime.

### JWT Authentication
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new() {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
        };
    });
```

### Newtonsoft.Json — PascalCase on the wire
```csharp
builder.Services.AddControllers()
    .AddNewtonsoftJson(o => o.SerializerSettings.ContractResolver = new DefaultContractResolver());
```
`DefaultContractResolver` keeps property names as-is (PascalCase). Angular models match this exactly. Without this, `System.Text.Json` would camelCase all properties by default, breaking Angular model bindings.

### CORS
```csharp
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader()));
```
Allows the Angular dev server to call the API. In production, replace with the deployed frontend origin.

### DI registrations
```csharp
builder.Services.AddSharedRepository();
builder.Services.AddSharedService();
builder.Services.AddPassRepository();
builder.Services.AddPassService();
```

### Background jobs
```csharp
JobManager.Initialize(new JobScheduler(app.Services));  // FluentScheduler

if (config["BackgroundJobs:PaymentCleanup:Enabled"] == "true")
    builder.Services.AddHostedService<PaymentDraftCleanupJob>();
```

### Static file serving (Angular)
```csharp
app.UseStaticFiles();   // serves Budget.Frontend/dist → wwwroot
app.MapFallbackToFile("index.html");  // SPA fallback routing
```
The Angular app is built into `wwwroot/` — the API serves it as static files. One process, one port in production.

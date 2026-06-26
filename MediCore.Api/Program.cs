using System.Text;
using MediCore.Api.Data;
using MediCore.Api.Domain.Entities;
using MediCore.Api.Infrastructure;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Formatting.Json;

// Logger di bootstrap: cattura anche gli errori in fase di avvio.
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        new JsonFormatter(),
        "logs/medicore-.json",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));

    // Necessaria ai token provider di Identity (AddIdentityCore non la registra da sola).
    builder.Services.AddDataProtection();

    builder.Services.AddIdentityCore<AppUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    var jwtSection = builder.Configuration.GetSection("Jwt");
    var jwtKey = jwtSection["Key"]
        ?? throw new InvalidOperationException("Chiave JWT non configurata (Jwt:Key).");

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
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<ITokenService, JwtTokenService>();
    builder.Services.AddScoped<IServizioService, ServizioService>();
    builder.Services.AddScoped<IPrestazioneService, PrestazioneService>();
    builder.Services.AddScoped<ITariffaService, TariffaService>();
    builder.Services.AddScoped<ITurnoService, TurnoService>();
    builder.Services.AddScoped<IMedicoService, MedicoService>();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Incollare il token JWT (senza prefisso 'Bearer ')."
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
    });

    var app = builder.Build();

    // Applica le migration pendenti e popola ruoli/amministratore all'avvio.
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        await services.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        await IdentityDataSeeder.SeedAsync(services, app.Configuration);
    }

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Avvio dell'applicazione terminato in modo inatteso");
}
finally
{
    Log.CloseAndFlush();
}

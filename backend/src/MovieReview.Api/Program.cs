using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MovieReview.Api.Auth;
using MovieReview.Api.Data;
using MovieReview.Api.External.Tmdb;
using MovieReview.Api.Repositories.Implementations;
using MovieReview.Api.Repositories.Interfaces;
using MovieReview.Api.Services.Implementations;
using MovieReview.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // MediaType serializes as "Movie" / "TvShow" instead of 0 / 1.
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ---------- Swagger (with JWT bearer support) ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Movie Review Platform API",
        Version = "v1",
        Description = "RESTful Web API for browsing movies & TV shows, writing reviews and posting ratings. " +
                      "Service Oriented Architecture final project — Arb Xhelili, SEEU."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT returned by /api/auth/login — no 'Bearer ' prefix needed."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// ---------- Database ----------
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Falls back to a local placeholder so design-time tooling (dotnet ef) can build the model
    // without a real connection string.
    var connectionString = builder.Configuration.GetConnectionString("Default");
    options.UseNpgsql(string.IsNullOrWhiteSpace(connectionString)
        ? "Host=localhost;Database=moviereview_design"
        : connectionString);
});

// ---------- Authentication & authorization ----------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Missing Jwt configuration section.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// ---------- CORS ----------
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(corsOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// ---------- Repositories — one instance per HTTP request, services depend on the interfaces only ----------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITitleRepository, TitleRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();

// ---------- TMDB external service ----------
builder.Services.Configure<TmdbSettings>(builder.Configuration.GetSection(TmdbSettings.SectionName));
builder.Services.AddHttpClient<ITmdbClient, TmdbClient>((serviceProvider, client) =>
{
    var tmdbSettings = builder.Configuration.GetSection(TmdbSettings.SectionName).Get<TmdbSettings>() ?? new TmdbSettings();
    client.BaseAddress = new Uri(tmdbSettings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

// ---------- Services — business rules live here, controllers stay logic-free ----------
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITitleService, TitleService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITmdbImportService, TmdbImportService>();

var app = builder.Build();

// ---------- Apply migrations & seed the admin account ----------
if (!string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("Default")))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await context.Database.MigrateAsync();
    await DbSeeder.SeedAdminAsync(context, app.Configuration, logger);
}
else
{
    app.Logger.LogWarning("No connection string configured — skipping migration and admin seeding.");
}

// Swagger stays enabled in every environment: the rubric asks for an API testable via Swagger UI.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Movie Review Platform API v1");
});

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

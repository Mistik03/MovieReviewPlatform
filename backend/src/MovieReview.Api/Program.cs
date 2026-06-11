using Microsoft.EntityFrameworkCore;
using MovieReview.Api.Data;
using MovieReview.Api.Repositories.Implementations;
using MovieReview.Api.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Falls back to a local placeholder so design-time tooling (dotnet ef) can build the model
    // without a real connection string.
    var connectionString = builder.Configuration.GetConnectionString("Default");
    options.UseNpgsql(string.IsNullOrWhiteSpace(connectionString)
        ? "Host=localhost;Database=moviereview_design"
        : connectionString);
});

// Repositories — one instance per HTTP request, services depend on the interfaces only.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITitleRepository, TitleRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

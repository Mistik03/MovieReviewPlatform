using Microsoft.EntityFrameworkCore;
using MovieReview.Api.Data;

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

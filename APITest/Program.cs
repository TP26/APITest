using APITest.Models;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string postgresqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

builder.Services.AddDbContext<CategoryContext>(options => options.UseNpgsql(postgresqlConnectionString), contextLifetime: ServiceLifetime.Scoped);
builder.Services.AddDbContext<ItemContext>(options => options.UseNpgsql(postgresqlConnectionString), contextLifetime: ServiceLifetime.Scoped);
builder.Services.AddDbContext<ConfigurationItemListsContext>(options => options.UseNpgsql(postgresqlConnectionString),contextLifetime: ServiceLifetime.Scoped);
builder.Services.AddDbContext<ConfigurationContext>(options => options.UseNpgsql(postgresqlConnectionString), contextLifetime: ServiceLifetime.Scoped);
builder.Services.AddDbContext<CoOrdinatesContext>(options => options.UseNpgsql(postgresqlConnectionString), contextLifetime: ServiceLifetime.Scoped);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
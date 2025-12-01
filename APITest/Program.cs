using APITest;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

//To add - Swagger documentation

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ItemDb>(opt => opt.UseInMemoryDatabase("ItemList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Item API",
        Description = "Simple testing Item API",
    });
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.MapGet("/items", async (ItemDb db) =>
    await db.Items.ToListAsync());

app.MapGet("/items/{id}", async (int id, ItemDb db) =>
    await db.Items.FindAsync(id)
        is Item item
            ? Results.Ok(item)
            : Results.NotFound());

app.MapGet("/items/find/{position}", async (int position, ItemDb db) =>
await db.Items.Where(item  => item.Position == position).ToListAsync());

app.MapPost("/items", async (Item item, ItemDb db) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();

    return Results.Created($"/items/{item.Id}", item);
});

app.MapPut("/items/{id}", async (int id, Item inputItem, ItemDb db) =>
{
    var item = await db.Items.FindAsync(id);

    if (item is null) return Results.NotFound();

    item.Name = inputItem.Name;
    item.Position = inputItem.Position;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/items/{id}", async (int id, ItemDb db) =>
{
    if (await db.Items.FindAsync(id) is Item item)
    {
        db.Items.Remove(item);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PersonDbContext>(opt => opt.UseInMemoryDatabase("MinimalApi"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( x =>
x.SwaggerDoc("v1", new OpenApiInfo 
{
    Title = "Minimal Api Demo",
    Version = "v1",
    Contact = new OpenApiContact{ Name = "Matheus Souza", Email = "matheussouzaslv2@gmail.com" }

}));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/person", async (PersonDbContext db) =>
 await db.Persons.ToListAsync())
    .Produces(StatusCodes.Status200OK);

app.MapGet("/person/{id}", async (PersonDbContext db, Guid id) =>
{
    var personExist = await db.Persons.FindAsync(id);
    if (personExist is null) return Results.NotFound();

    return Results.Ok(personExist);
})
   .Produces(StatusCodes.Status200OK)
   .Produces(StatusCodes.Status404NotFound);

app.MapPost("/person", async (PersonDbContext db, Person person) =>
{
    db.Persons.Add(person);
    await db.SaveChangesAsync();

    return Results.Created($"/person/{person.Id}", person);
})
   .Produces(StatusCodes.Status201Created)
   .Produces(StatusCodes.Status400BadRequest);

app.MapPut("/person/{id}", async (PersonDbContext db, Guid id, Person person) =>
{
    var personExist = await db.Persons.FindAsync(id);
    if (personExist is null) return Results.NotFound();

    personExist.FirstName = person.FirstName;
    personExist.LastName = person.LastName;
    personExist.IsActive = person.IsActive;

    await db.SaveChangesAsync();

    return Results.NoContent();
})
   .Produces(StatusCodes.Status204NoContent)
   .Produces(StatusCodes.Status400BadRequest);

app.MapDelete("/person/{id}", async (PersonDbContext db, Guid id) =>
{
    var personExist = await db.Persons.FindAsync(id);
    if (personExist is null) return Results.NotFound();

    db.Persons.Remove(personExist);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
   .Produces(StatusCodes.Status204NoContent)
   .Produces(StatusCodes.Status400BadRequest);


app.Run();

class Person
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
}

class PersonDbContext : DbContext
{
    public PersonDbContext(DbContextOptions<PersonDbContext> options) : base(options) { }

    public DbSet<Person> Persons { get; set; }
}

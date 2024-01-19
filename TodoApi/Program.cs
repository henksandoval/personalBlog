var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var toDoItems = new List<ToDoItem>
{
	new() { Id = 1, Title = "Do laundry", Completed = false },
	new() { Id = 2, Title = "Take out trash", Completed = false }
};

app.MapGet("/api/todo", () => toDoItems)
	.WithName("GetAll")
	.Produces(StatusCodes.Status200OK);

app.MapPost("/api/todo", (ToDoItem toDoItem, HttpContext http) =>
	{
		toDoItem.Id = toDoItems?.Max(item => item.Id) + 1 ?? 1;
		toDoItems?.Add(toDoItem);

		var location = $"{http.Request.Path}/{toDoItem.Id}";
		return Results.Created(location, toDoItem);
	})
	.WithName("CreateToDoItem")
	.Produces<ToDoItem>(StatusCodes.Status201Created);

app.Run();

public class ToDoItem
{
	public int Id { get; set; }
	public string Title { get; set; }
	public bool Completed { get; set; }
}

public abstract partial class Program { }
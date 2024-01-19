using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

namespace TodoApi.Integration.Tests;

public class ToDoControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly HttpClient client = factory.CreateClient();

	[Fact]
	public async Task GetAll_ReturnsToDoItems()
	{
		// Act
		var response = await client.GetAsync("/api/todo");

		// Assert
		response.EnsureSuccessStatusCode();
		var toDoItems = JsonConvert.DeserializeObject<List<ToDoItem>>(await response.Content.ReadAsStringAsync());
		Assert.NotNull(toDoItems);
		Assert.True(toDoItems.Count > 0);
	}

	[Fact]
	public async Task Create_AddsToDoItem()
	{
		// Arrange
		var newItem = new ToDoItem { Title = "Test task", Completed = false };
		var httpContent = new StringContent(JsonConvert.SerializeObject(newItem), Encoding.UTF8, "application/json");

		// Act
		var response = await client.PostAsync("/api/todo", httpContent);

		// Assert
		Assert.Equal(HttpStatusCode.Created, response.StatusCode);
		var createdItem = JsonConvert.DeserializeObject<ToDoItem>(await response.Content.ReadAsStringAsync());
		Assert.NotNull(createdItem);
		Assert.Equal("Test task", createdItem.Title);
		Assert.False(createdItem.Completed);
		Assert.True(createdItem.Id > 0);
	}
}
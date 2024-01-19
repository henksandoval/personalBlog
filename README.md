Bienvenidos al primer artículo de una serie dedicada a explorar las pruebas de integración en .NET utilizando WebApplicationFactory y TDD. Sumérgete en este viaje y vamos a desentrañar las mejores prácticas y herramientas para mantener la calidad del software en un mundo que no cesa de cambiar.

## Introducción: La evolución segura en el mundo del software

Las empresas son entidades que viven en constante cambio, buscando adaptarse y mejorar para mantener su competitividad en un mercado siempre en evolución. En la vanguardia de esta transformación se encuentra el software, que como herramienta vital de la modernidad, debe evolucionar de la misma manera: de forma segura y confiable. Fallas en el software pueden desencadenar consecuencias graves, desde una imagen deteriorada hasta la pérdida significativa de clientes.

En este entorno de cambio continuo, las metodologías de pruebas de software adquieren un rol protagónico. Entre ellas, las pruebas de integración se destacan por validar la cohesión entre los distintos componentes de nuestros sistemas. Sin embargo, su ejecución puede ser un reto, requiriendo tiempo y recursos significativos, además de la complejidad inherente a su diseño y ejecución.

Imagina un cambio de código menor que se despliega a producción sin una segunda revisión. Todo parece estar bien hasta que las llamadas de los clientes comienzan: "¡El sistema está caído!" Este escenario, aunque dramático, ilustra la necesidad crítica de pruebas de integración eficientes. WebApplicationFactory en .NET proporciona una poderosa herramienta para evitar tales catástrofes, permitiendo que los desarrolladores testeen sus aplicaciones en un entorno que simula el servidor real.

## WebApplicationFactory: Un puente hacia la confianza en el código

Para abordar estos desafíos, .NET presenta WebApplicationFactory, una herramienta clave del paquete Microsoft.AspNetCore.Mvc.Testing. Esta clase ingeniosa permite la ejecución de un cliente de pruebas que se comunica con una instancia en memoria de un entorno de servidor real de nuestra aplicación .NET. Esto nos facilita realizar solicitudes HTTP y recibir respuestas sin la necesidad de un despliegue completo, permitiendo pruebas de integración más rápidas y menos costosas.

### Beneficios de WebApplicationFactory

1. **Entorno de prueba realista**: `WebApplicationFactory` configura un entorno de servidor real que se ejecuta en memoria. Esto significa que las pruebas pueden interactuar con la aplicación de una manera que es muy cercana a cómo lo haría un cliente real, como un navegador web o un cliente HTTP.
    
2. **Configuración de pruebas simplificada**: Automatiza la configuración del host y el servidor web basándose en la configuración de producción, pero con la flexibilidad de sobrescribir dicha configuración para las pruebas.
    
3. **Aislamiento**: Permite configurar de manera independiente cada instancia de `WebApplicationFactory`, asegurando un aislamiento efectivo entre pruebas.
    
4. **Inyección de dependencias**: Permite la inyección de dependencias de prueba dentro de la aplicación, lo que facilita el reemplazo de servicios reales por versiones simuladas (mocks) o falsas (fakes) brindando un control preciso del entorno de pruebas.
    
5. **Pruebas de sub-sistemas**: Habilita la prueba de componentes individuales como bases de datos, sistemas de archivos, APIs de terceros, etc., en un entorno que simula cómo interactuarían con la aplicación completa.
    
6. **Soporte para cookies y autenticación**: Permite probar flujos de trabajo que involucran autenticación y manejo de sesiones, ya que el servidor de prueba maneja cookies y cabeceras HTTP de forma similar a como lo haría en producción.
    
7. **Facilidad para realizar solicitudes HTTP**: Al proporcionar un cliente HTTP preconfigurado, facilita la realización de solicitudes HTTP a la aplicación sin necesidad de configurar manualmente cabeceras, tokens de autenticación, etc.

Con `WebApplicationFactory`, el equipo de .NET nos ofrece una solución robusta para abordar las complejidades de las pruebas de integración, permitiéndonos centrarnos más en la calidad del código y la entrega de valor a nuestros usuarios.

## Ejemplo Práctico: Implementación de WebApplicationFactory

### Agregar WebApplicationFactory en nuestra solución.

1. Agregar una referencia al paquete [Microsoft.AspNetCore.Mvc.Testing](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Testing).

	Puedes hacerlo de varias maneras:

   * Utiliza el administrador de paquetes NuGet de tu IDE favorito.

   * A través de la interfaz de línea de comandos de .NET, ejecuta el siguiente comando: `dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.0.1`

   * Modifica directamente el archivo de proyecto (.csproj) de tu aplicación para incluir la referencia: `<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.1" />`

2. Habilitar el acceso a la clase Program.

> Para que `WebApplicationFactory` pueda funcionar correctamente, necesita acceder a la clase Program de tu aplicación:

   * Concede acceso a los tipos internal de tu aplicación al proyecto de pruebas agregando lo siguiente a tu archivo .csproj:

```xml
	<ItemGroup>
		<InternalsVisibleTo Include="NombreProyectoDePruebas" />
	</ItemGroup>
```

   * O bien, haciendo que la clase `Program` sea pública con una declaración parcial en el archivo de inicio de tu aplicación:
```csharp
	public partial class Program { }
```

### Configurando WebApplicationFactory en nuestra API

Para ilustrar cómo se usa `WebApplicationFactory`, consideremos un controlador de ejemplo con una Minimal API. Este controlador gestiona dos llamadas una GET y otra POST.

```csharp
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
```

Ahora, veamos cómo podríamos probar este controlador utilizando `WebApplicationFactory`. Para ello, vamos a crear una clase de prueba que implemente la interfaz `IClassFixture` para inicializar mediante `WebApplicationFactory` nuestro entorno de pruebas.

```csharp
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
```

En este ejemplo de prueba:

* Utilizamos `IClassFixture<WebApplicationFactory<Program>>` para indicar que nuestra clase de pruebas usará una instancia de `WebApplicationFactory` con la configuración definida en la clase `Program` de nuestra API.
* Creamos un cliente HTTP con `CreateClient()`, que no requiere que el servidor Kestrel esté escuchando en un puerto TCP real.
* Realizamos peticiones HTTP asincrónicas al EndPoint de nuestra API y verificamos que la respuesta sea exitosa y que el contenido sea del tipo esperado.
* Deserializamos el contenido de la respuesta y validamos que contenga los datos esperados.

Si deseas ver el código en un sistema, te invito a visitarlo en mi repositorio de GitHub. [Repositorio de GitHub](https://github.com/henksandoval/personalBlog/tree/WebApplicationFactory)

## Conclusión y próximos pasos

`WebApplicationFactory` es más que una herramienta; es un aliado en el desafío de mantener la calidad y la confianza en el software que evoluciona rápidamente.

Te animo a poner en práctica estos conocimientos con `WebApplicationFactory` en tu entorno de desarrollo. Y mientras avanzas, recuerda que este es solo el comienzo. En artículos futuros, exploraremos más a fondo las pruebas de integración y cómo las prácticas de Desarrollo Guiado por Pruebas (TDD por sus siglas en inglés, Test Driven Development) pueden transformar tu proceso de desarrollo en .NET.

Tus preguntas, comentarios y experiencias son bienvenidos y serán el combustible para futuras discusiones y mejoras. Tu participación es crucial: comparte tus preguntas y experiencias, y únete a la conversación para elevar nuestras prácticas de desarrollo.

Recuerda, cada prueba que escribimos no sólo verifica una funcionalidad, sino que también refleja nuestro compromiso con la excelencia en el código que entregamos. ¡Adelante, y hagamos que cada línea de código cuente!

# Fuentes

* Integration tests in ASP.NET Core [learn.microsoft.com/](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0).

Desarrollar aplicaciones por primera vez no es una tarea sencilla. Es un desafío, y a veces, agotador. Sin embargo, mantener y actualizar aplicaciones existentes puede ser aún más complicado por varias razones:

* Falta de conocimientos adecuados.
    
* Estilos de codificación avanzados y personales de otros desarrolladores.
    
* Escasez o ausencia de documentación, lo cual puede ser frustrante, especialmente si no se alinea con el código.

La comodidad de conformarse con código que "simplemente funciona" a menudo prevalece, ignorando su sostenibilidad o legibilidad a largo plazo. ¿Cuántas veces hemos enfrentado código legado que parece un jeroglífico moderno? Personalmente, más veces de las que me gustaría admitir y, en ocasiones, he sido yo quien ha contribuido a ese caos.

Generar código complejo y enredado, sin pruebas unitarias, o con nombres de variables crípticos, es una garantía de pérdida de tiempo para quienes, en el futuro, intenten descifrarlo.

## ¿Por qué Importa el Código Limpio?

El código que escribes hoy es el legado que dejas para el futuro. No se trata solo de cumplir con una tarea; es esencial hacerlo de tal forma que sea comprensible y mantenible a largo plazo, ya sea por ti o por quien herede tu trabajo.

Imagina volver a tu código seis meses después o entregarlo a un colega. ¿Entenderán la lógica? ¿Pueden agregar fácilmente nuevas características sin romper la funcionalidad existente? Ahí es donde el código limpio entra en juego.

Como anedocta personal, hace poco tiempo, tras cumplir apresuradamente con un requerimiento de negocio, cerré la tarea y pasé al siguiente proyecto. Dos meses después, al enfrentarme a un nuevo requerimiento en el mismo dominio, me di cuenta de que no entendía mi propio código. Era un laberinto de herencias, genéricos y funciones anidadas. Por suerte, una amplia cobertura de pruebas me permitió refactorizar sin miedo a dañar la funcionalidad existente. Para garantizar la claridad esta vez, solicité la revisión y el feedback de mis compañeros. Esa experiencia frustrante me motivó a retomar este viejo post y compartir estas reflexiones con ustedes.

En las siguientes secciones, no solo hablaré sobre Código Limpio; intentaré mostrarles cómo un buen código puede mejorarse aún más con la aplicación de pequeñas mejoras.

## **Los Principios del Clean Code** <sup>[1]</sup>

En su influyente obra "**Clean Code**", **Robert C. Martin**, también conocido como Uncle Bob, comparte una serie de principios fundamentales para la escritura de código de alta calidad. A continuación, detallamos algunas de estas ideas esenciales, que pueden transformar el código de aceptable a excepcional.

### **Nombres Significativos** <sup>[2]</sup>

Elige nombres que comuniquen claramente la intención detrás de una variable, función o clase. Por ejemplo, `diasDesdeCreacion` es más descriptivo que `dias`. Un buen nombre es autoexplicativo y puede reducir la dependencia de comentarios adicionales.

### **Evitar los "Números Mágicos"** <sup>[3]</sup>

Los "números mágicos" son valores numéricos con significados no obvios. Reemplazarlos con constantes nombradas mejora la comprensión del código. Así, `const HORAS_EN_UN_DIA = 24;` es más claro que usar el número 24 directamente en el código.

### **Las Funciones Deben Hacer Una Sola Cosa** <sup>[4]</sup>

Cada función debe estar diseñada para realizar una sola acción bien definida. Esto facilita las pruebas, la depuración y la lectura del código. Si una función está verificando el acceso del usuario y luego actualizando un registro, considera dividirla en dos funciones distintas.

### **Keep it simple, Stupid (KISS)** <sup>[5]</sup>

Evita complicar el código innecesariamente. La solución más simple y directa es a menudo la mejor opción. Esto es crucial para mantener el código manejable y reducir el riesgo de errores.

### **Don't Repeat Yourself (DRY)** <sup>[6]</sup>

La duplicación de código es una fuente común de errores. Utiliza abstracciones para evitar repetir la misma lógica. Sin embargo, ten cuidado de no caer en la trampa de la abstracción excesiva en nombre de DRY, lo cual puede llevar a una complejidad innecesaria.

### **Escribe Pruebas Unitarias** <sup>[7]</sup>

El desarrollo de pruebas unitarias debe ir de la mano con la escritura de código. Estas pruebas son esenciales para validar que cada componente del sistema funciona como se espera y para mitigar el miedo a los cambios que puedan introducir errores.

### **Refactoriza Regularmente** <sup>[8]</sup>

La refactorización no debe verse como una tarea que se realiza una sola vez. Es un aspecto continuo del desarrollo de software. Dedica tiempo a revisar y mejorar el código, buscando oportunidades para aplicar los principios de Clean Code.

**Estos principios son más que simples reglas; son la base para escribir código que no solo es funcional sino también sostenible y agradable de trabajar. En la próxima sección, analizaremos cómo aplicar estos principios de manera efectiva, asegurando que nuestro código no solo funcione, sino que también sea elegante y fácil de mantener.**

## Ejemplo de Refactorización .NET

Al revisar el código fuente de una aplicación, me encontré con el siguiente método que, aunque escrito con cuidado, presentaba oportunidades para alinearse más estrechamente con las prácticas recomendadas de Clean Code.

Inicialmente, el método se veía así:

```csharp
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace BlogExamples.CleanCode;

public class Repository<TEntity> where TEntity: class, IEntity
{
	private readonly DbSet<TEntity> dbSet;

	public Repository(DbContext dbContext)
	{
		dbSet = dbContext.Set<TEntity>();
	}

	public async Task<PaginatedEntityModel<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filterExpression, Expression<Func<TEntity, string>> orderExpression, PaginationModel paginationModel)
	{
		IQueryable<TEntity> query = dbSet.Where(filterExpression);
		double iTotal = await query.CountAsync();
		int skip = paginationModel.Page - 1 * paginationModel.RecordsPerPage;

		IList<TEntity> entities = await query
			.OrderBy(orderExpression)
			.Skip(skip)
			.Take(paginationModel.RecordsPerPage)
			.ToListAsync();

		PaginatedEntityModel<TEntity> paginatedEntities = new PaginatedEntityModel<TEntity>
		{
			Entities = entities,
			TotalRecordCount = iTotal,
			CurrentPage = paginationModel.Page,
			RecordsPerPage = paginationModel.RecordsPerPage,
			TotalPages = Math.Ceiling(iTotal / paginationModel.RecordsPerPage)
		};

		return paginatedEntities;
	}
}

public class PaginatedEntityModel<TEntity> where TEntity : class, IEntity
{
	public IList<TEntity> Entities { get; set; }
	public int CurrentPage { get; set; }
	public int RecordsPerPage { get; set; }
	public double TotalRecordCount { get; set; }
	public double TotalPages { get; set; }
}

public interface IEntity {}

public record PaginationModel(int Page, int RecordsPerPage);
```

En principio, el código me parece bastante acertado y diría que escrito con cariño. Sin embargo, propondría algunas mejoras para ser consistentes con las mejores practicas del CleanCode.

1. **Claridad y legibilidad**: Corregir los nombres de las variables para que sean consistentes y descriptivos. Por ejemplo, cambiar iTotal por totalRecordCount, lo que es más descriptivo de lo que la variable está almacenando.
    
2. **Responsabilidad única**: Trasladar la lógica del cálculo de cantidad de registros a omitir y del total de páginas a métodos propios.
    
    Esto no solo hace el código más legible, sino que también encapsula el comportamiento reutilizarlo si es necesario.
    
    Para el calculo del total de páginas sería optimo mover el método a la clase PaginatedEntityModel, lo cual es más lógico ya que este cálculo se relaciona directamente con la paginación.
    
3. **Uso de comentarios con sentido**: Incorporar un comentario para justificar el número mágico 1 (que con mi conocimiento es imposible explicar sin un comentario).
    
4. **Evitar números mágicos**: Corregir la precedencia de los operadores en el cálculo de skip para evitar malentendidos y errores.
    
5. **Robustez**: Agregar validaciones para paginationModel y entities para evitar la creación de instancias en un estado inválido.
    
6. **Encapsulamiento y validación**: Agregar un constructor a PaginatedEntityModel para asegurar que todos los valores proporcionados sean válidos, lo que refuerza la integridad del modelo de paginación.
    
7. **Mantenibilidad**: Trasladar la lógica para calcular el total de páginas a un método privado, facilitando el mantenimiento, mejorando la cohesión y evitando la duplicación de código.
    

Con las mejoras propuestas, el código refactorizado quedaría de la siguiente manera, también puedes explorar los cambios mediante GitDiff en [este enlace de mi repositorio de GitHub](https://github.com/henksandoval/personalBlog/commit/0f20366dd0c9d0c685d29c65e4ab99a17b7860eb)

```csharp
﻿using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace BlogExamples.CleanCode;

public class Repository<TEntity> where TEntity: class, IEntity
{
	private readonly DbSet<TEntity> dbSet;

	public Repository(DbContext dbContext)
	{
		dbSet = dbContext.Set<TEntity>();
	}

	public async Task<PaginatedEntityModel<TEntity>> GetAllAsync(
		Expression<Func<TEntity, bool>> filterExpression, 
		Expression<Func<TEntity, string>> orderExpression, 
		PaginationModel paginationModel)
	{
		if (paginationModel is null)
		{
			throw new ArgumentNullException(nameof(paginationModel));
		}

		IQueryable<TEntity> query = dbSet.Where(filterExpression);
		double totalRecordCount = await query.CountAsync();
		int skip = CalculateSkip(paginationModel.Page, paginationModel.RecordsPerPage);

		IList<TEntity> entities = await query
			.OrderBy(orderExpression)
			.Skip(skip)
			.Take(paginationModel.RecordsPerPage)
			.ToListAsync();

		return new PaginatedEntityModel<TEntity>(entities, totalRecordCount, paginationModel);
	}

	private static int CalculateSkip(int page, int recordsPerPage)
	{
		// Calculate the number of records to skip by converting the 1-based page number to 
		// a 0-based index and multiplying by the number of records per page. 
		// This aligns Page 1 with a skip of 0 records, ensuring the first page starts with the first record.
		return (page - 1) * recordsPerPage;
	}
}

public class PaginatedEntityModel<TEntity> where TEntity : class, IEntity
{
	public IList<TEntity> Entities { get; }
	public int CurrentPage { get; }
	public int RecordsPerPage { get; }
	public double TotalRecordCount { get; }
	public double TotalPages => CalculateTotalPages();

	public PaginatedEntityModel(IList<TEntity> entities, double totalRecordCount, PaginationModel paginationModel)
	{
		Entities = entities ?? throw new ArgumentNullException(nameof(entities));
		TotalRecordCount = totalRecordCount >= 0 ? totalRecordCount 
			: throw new ArgumentOutOfRangeException(nameof(totalRecordCount));
		CurrentPage = paginationModel.Page;
		RecordsPerPage = paginationModel.RecordsPerPage;
	}

	private double CalculateTotalPages()
	{
		return Math.Ceiling(TotalRecordCount / RecordsPerPage);
	}
}

public interface IEntity {}

public record PaginationModel(int Page, int RecordsPerPage);
```

# Conclusión

A lo largo de esta discusión, hemos explorado el valor innegable de adherirse a los principios de código limpio y hemos iniciado el proceso de transformar un bloque de código en algo más mantenible y comprensible. Sin embargo, nuestro viaje hacia la mejora del código no termina aquí.

En nuestra próxima entrega, nos zambulliremos aún más profundamente en el arte de la refactorización. Desmenuzaremos técnicas específicas y las aplicaremos a una variedad de "olores" de código, esos indicadores sutiles pero reveladores de problemas subyacentes. Más que simples instrucciones, compartiremos las herramientas y el razonamiento para identificar cuándo y cómo actuar ante estos desafíos comunes.

Recuerda que la búsqueda del código limpio no es un ejercicio de perfecciónismo ni un riguroso cumplimiento de reglas inmutables. Más bien, es un compromiso continuo para escribir código que no solo funcione, sino que también sea accesible, manejable y escalable para tu equipo y para ti mismo en el futuro.

Así que mantengan su mente inquisitiva y su teclado listo; hay un mundo de código esperando ser refinado. ¡Estén atentos para la próxima entrega y prepárense para llevar su habilidad de refactorización al siguiente nivel!

# Fuentes

* <sup>1</sup> Martin, Robert C. (2008). *Clean Code: A Handbook of Agile Software Craftsmanship*. Pearson. Disponible en [Amazon](https://amzn.eu/d/eAITKn3).
* <sup>2</sup> Martin, Robert C. *Clean Code: Fundamentals, Episode 2 - Names++*. [CleanCoders.com](https://cleancoders.com/episode/clean-code-episode-2).
* <sup>3</sup> *Replace Magic Number with Symbolic Constant* en Refactoring Guru. Disponible en [Refactoring.guru](https://refactoring.guru/replace-magic-number-with-symbolic-constant).
* <sup>4</sup> Martin, Robert C. (2014, May 8). *The Single Responsibility Principle*. [Clean Coder Blog](https://blog.cleancoder.com/uncle-bob/2014/05/08/SingleReponsibilityPrinciple.html).
* <sup>5</sup> *KISS (Keep it Simple, Stupid) - A Design Principle*. Interaction Design Foundation. Disponible en [Interaction-Design.org](https://www.interaction-design.org/literature/article/kiss-keep-it-simple-stupid-a-design-principle).
* <sup>6</sup> Hunt, Andrew; Thomas, David (1999). *The Pragmatic Programmer: From Journeyman to Master*. Addison-Wesley.
* <sup>7</sup> Exeal (2023, October). *Práctica deliberada: una forma alternativa de aprender TDD*. Disponible en [Exeal.com](https://www.exeal.com/blog/2023/10/practica-deliberada-y-tdd/).
* <sup>8</sup> Exeal (2021, April). *Hábitos de productividad como programadores*. Disponible en [Exeal.com](https://www.exeal.com/blog/2021/04/habitos-de-productividad-como-programadores/).
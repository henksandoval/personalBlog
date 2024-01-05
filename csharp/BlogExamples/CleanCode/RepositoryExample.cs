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
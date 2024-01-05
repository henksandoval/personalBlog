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
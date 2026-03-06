using ExpenseControl.Data;
using ExpenseControl.Models.Interfaces;
using ExpenseControl.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Services.Base
{
    public abstract class BaseService<T> : IBaseService<T> where T : class, IEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        public BaseService(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }
        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        public virtual async Task UpdateAsync(T entity)
        {
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty == null) throw new Exception("Entity must have an Id property");
            var entityId = (int)idProperty.GetValue(entity);
            var existingEntity = await _dbSet.FindAsync(entityId);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception($"Entity with Id {entityId} not found");
            }

        }
        public virtual async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
        }

    }
}

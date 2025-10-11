using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface genérica para repositórios, definindo operações CRUD básicas
    /// </summary>
    /// <typeparam name="T">Tipo da entidade</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Obtém uma entidade pelo seu ID
        /// </summary>
        Task<T> GetByIdAsync(int id);
        
        /// <summary>
        /// Obtém todas as entidades
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();
        
        /// <summary>
        /// Encontra entidades com base em um predicado
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        /// <summary>
        /// Adiciona uma nova entidade
        /// </summary>
        Task AddAsync(T entity);
        
        /// <summary>
        /// Atualiza uma entidade existente
        /// </summary>
        Task UpdateAsync(T entity);
        
        /// <summary>
        /// Remove uma entidade
        /// </summary>
        Task DeleteAsync(T entity);
        
        /// <summary>
        /// Salva as alterações no banco de dados
        /// </summary>
        Task SaveChangesAsync();
    }
}
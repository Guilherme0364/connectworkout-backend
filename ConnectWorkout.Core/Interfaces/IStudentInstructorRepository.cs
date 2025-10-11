using System.Threading.Tasks;
using ConnectWorkout.Core.Models;

namespace ConnectWorkout.Core.Interfaces
{
    /// <summary>
    /// Interface para operações de repositório específicas de relações aluno-instrutor
    /// </summary>
    public interface IStudentInstructorRepository : IRepository<StudentInstructor>
    {
        /// <summary>
        /// Verifica se já existe uma conexão entre um aluno e um instrutor
        /// </summary>
        Task<bool> ConnectionExistsAsync(int studentId, int instructorId);
        
        /// <summary>
        /// Obtém uma conexão específica entre aluno e instrutor
        /// </summary>
        Task<StudentInstructor> GetConnectionAsync(int studentId, int instructorId);
        
        /// <summary>
        /// Remove uma conexão entre aluno e instrutor
        /// </summary>
        Task RemoveConnectionAsync(int studentId, int instructorId);
    }
}
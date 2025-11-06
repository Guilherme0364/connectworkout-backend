using System.Collections.Generic;
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
        [System.Obsolete("Use AcceptedConnectionExistsAsync, PendingInvitationExistsAsync, or AnyRelationshipExistsAsync instead")]
        Task<bool> ConnectionExistsAsync(int studentId, int instructorId);

        /// <summary>
        /// Verifica se existe uma conexão aceita entre um aluno e um instrutor
        /// </summary>
        Task<bool> AcceptedConnectionExistsAsync(int studentId, int instructorId);

        /// <summary>
        /// Verifica se existe um convite pendente entre um aluno e um instrutor
        /// </summary>
        Task<bool> PendingInvitationExistsAsync(int studentId, int instructorId);

        /// <summary>
        /// Verifica se existe qualquer relacionamento (qualquer status) entre um aluno e um instrutor
        /// </summary>
        Task<bool> AnyRelationshipExistsAsync(int studentId, int instructorId);

        /// <summary>
        /// Obtém uma conexão específica entre aluno e instrutor
        /// </summary>
        Task<StudentInstructor> GetConnectionAsync(int studentId, int instructorId);

        /// <summary>
        /// Remove uma conexão entre aluno e instrutor
        /// </summary>
        Task RemoveConnectionAsync(int studentId, int instructorId);

        /// <summary>
        /// Obtém todos os convites pendentes para um aluno específico
        /// </summary>
        Task<IEnumerable<StudentInstructor>> GetPendingInvitationsForStudentAsync(int studentId);

        /// <summary>
        /// Obtém um convite específico por ID
        /// </summary>
        Task<StudentInstructor> GetInvitationByIdAsync(int invitationId);

        /// <summary>
        /// Aceita um convite de instrutor
        /// </summary>
        Task<bool> AcceptInvitationAsync(int invitationId, int studentId);

        /// <summary>
        /// Rejeita um convite de instrutor
        /// </summary>
        Task<bool> RejectInvitationAsync(int invitationId, int studentId);
    }
}
using System.Collections.Generic;

namespace ConnectWorkout.Core.DTOs
{
    /// <summary>
    /// DTO para exibir informações resumidas de um instrutor
    /// </summary>
    public class InstructorSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
    }
}
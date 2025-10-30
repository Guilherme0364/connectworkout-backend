using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConnectWorkout.Core.DTOs
{
    /// <summary>
    /// Representa um exercício retornado pela API ExerciseDB
    /// </summary>
    public class ExerciseDbModel
    {
        private string _id;
        private string _gifUrl;

        // Resolution constants for different contexts
        public const int INSTRUCTOR_RESOLUTION = 360;  // Smaller for lists
        public const int STUDENT_RESOLUTION = 720;     // Larger for detailed view
        public const int THUMBNAIL_RESOLUTION = 180;   // Extra small for thumbnails
        public const int HD_RESOLUTION = 1080;         // High quality for zoom

        /// <summary>
        /// Identificador único na API ExerciseDB
        /// </summary>
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                // Auto-generate GifUrl when Id is set (default to student resolution)
                if (!string.IsNullOrEmpty(_id) && string.IsNullOrEmpty(_gifUrl))
                {
                    _gifUrl = GetImageUrl(_id, STUDENT_RESOLUTION);
                }
            }
        }

        /// <summary>
        /// Nome do exercício
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Parte do corpo trabalhada (ex: "back", "chest", "legs")
        /// </summary>
        public string BodyPart { get; set; }

        /// <summary>
        /// Equipamento necessário (ex: "barbell", "dumbbell", "body weight")
        /// </summary>
        public string Equipment { get; set; }

        /// <summary>
        /// URL da imagem GIF demonstrando o exercício
        /// Auto-generated from Id if not provided by API
        /// Uses RapidAPI image endpoint with resolution parameter
        /// </summary>
        public string GifUrl
        {
            get => _gifUrl ?? (!string.IsNullOrEmpty(_id) ? GetImageUrl(_id, STUDENT_RESOLUTION) : null);
            set => _gifUrl = value;
        }

        /// <summary>
        /// Músculo alvo principal
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Músculos secundários trabalhados
        /// </summary>
        public List<string> SecondaryMuscles { get; set; }

        /// <summary>
        /// Instruções de execução passo a passo
        /// </summary>
        public List<string> Instructions { get; set; }

        /// <summary>
        /// Generate image URL with specific resolution
        /// </summary>
        /// <param name="exerciseId">Exercise ID</param>
        /// <param name="resolution">Resolution (180, 360, 720, or 1080)</param>
        /// <returns>RapidAPI image URL</returns>
        public static string GetImageUrl(string exerciseId, int resolution)
        {
            // Validate resolution
            if (resolution != 180 && resolution != 360 && resolution != 720 && resolution != 1080)
            {
                resolution = STUDENT_RESOLUTION; // Default to 720
            }

            return $"https://exercisedb.p.rapidapi.com/image?exerciseId={exerciseId}&resolution={resolution}";
        }

        /// <summary>
        /// Get instructor-optimized image URL (smaller for lists)
        /// </summary>
        public string GetInstructorImageUrl()
        {
            return GetImageUrl(_id, INSTRUCTOR_RESOLUTION);
        }

        /// <summary>
        /// Get student-optimized image URL (larger for detailed view)
        /// </summary>
        public string GetStudentImageUrl()
        {
            return GetImageUrl(_id, STUDENT_RESOLUTION);
        }

        /// <summary>
        /// Get thumbnail image URL (extra small)
        /// </summary>
        public string GetThumbnailImageUrl()
        {
            return GetImageUrl(_id, THUMBNAIL_RESOLUTION);
        }

        /// <summary>
        /// Get HD image URL (highest quality)
        /// </summary>
        public string GetHDImageUrl()
        {
            return GetImageUrl(_id, HD_RESOLUTION);
        }
    }
}
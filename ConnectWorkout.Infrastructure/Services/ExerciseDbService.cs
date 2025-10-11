using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ConnectWorkout.Core.DTOs;
using ConnectWorkout.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConnectWorkout.Infrastructure.Services
{
    /// <summary>
    /// Serviço que integra com a API ExerciseDB externa
    /// </summary>
    public class ExerciseDbService : IExerciseDbService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ExerciseDbService> _logger;
        private readonly TimeSpan _cacheExpiryTime;
        private readonly JsonSerializerOptions _jsonOptions;

        public ExerciseDbService(
            HttpClient httpClient,
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<ExerciseDbService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            
            // Configuração do cliente HTTP
            _httpClient.BaseAddress = new Uri("https://exercisedb.p.rapidapi.com/");
            
            string apiKey = configuration["ExerciseDb:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("API key for ExerciseDB is missing in configuration");
            }
            
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", "exercisedb.p.rapidapi.com");
            
            // Configuração do cache
            int cacheMinutes = configuration.GetValue<int>("ExerciseDb:CacheExpiryMinutes", 1440); // 24 horas padrão
            _cacheExpiryTime = TimeSpan.FromMinutes(cacheMinutes);
            
            // Configuração do deserializador JSON
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<ExerciseDbModel>> GetAllExercisesAsync()
        {
            return await GetWithCacheAsync<List<ExerciseDbModel>>("exercises", _cacheExpiryTime);
        }

        public async Task<ExerciseDbModel> GetExerciseByIdAsync(string id)
        {
            return await GetWithCacheAsync<ExerciseDbModel>($"exercises/exercise/{id}", _cacheExpiryTime);
        }

        public async Task<List<ExerciseDbModel>> SearchExercisesByNameAsync(string name)
        {
            // A API não suporta busca por nome diretamente, então buscamos todos e filtramos
            var allExercises = await GetAllExercisesAsync();
            
            return allExercises.FindAll(e => 
                e.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<ExerciseDbModel>> GetExercisesByBodyPartAsync(string bodyPart)
        {
            return await GetWithCacheAsync<List<ExerciseDbModel>>(
                $"exercises/bodyPart/{bodyPart}", _cacheExpiryTime);
        }

        public async Task<List<ExerciseDbModel>> GetExercisesByTargetAsync(string target)
        {
            return await GetWithCacheAsync<List<ExerciseDbModel>>(
                $"exercises/target/{target}", _cacheExpiryTime);
        }

        public async Task<List<ExerciseDbModel>> GetExercisesByEquipmentAsync(string equipment)
        {
            return await GetWithCacheAsync<List<ExerciseDbModel>>(
                $"exercises/equipment/{equipment}", _cacheExpiryTime);
        }

        public async Task<List<string>> GetBodyPartsListAsync()
        {
            return await GetWithCacheAsync<List<string>>("exercises/bodyPartList", _cacheExpiryTime);
        }

        public async Task<List<string>> GetTargetsListAsync()
        {
            return await GetWithCacheAsync<List<string>>("exercises/targetList", _cacheExpiryTime);
        }

        public async Task<List<string>> GetEquipmentsListAsync()
        {
            return await GetWithCacheAsync<List<string>>("exercises/equipmentList", _cacheExpiryTime);
        }

        // Método genérico para buscar dados com cache
        private async Task<T> GetWithCacheAsync<T>(string endpoint, TimeSpan cacheTime)
        {
            string cacheKey = $"ExerciseDb_{endpoint}";
            
            // Tenta obter do cache primeiro
            if (_cache.TryGetValue(cacheKey, out T cachedResult))
            {
                _logger.LogInformation("Cache hit for {Endpoint}", endpoint);
                return cachedResult;
            }
            
            _logger.LogInformation("Cache miss for {Endpoint}, fetching from API", endpoint);
            
            try
            {
                // Se não estiver em cache, busca da API
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                
                // Salva no cache
                _cache.Set(cacheKey, result, cacheTime);
                
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching data from ExerciseDB API: {Message}", ex.Message);
                throw new Exception($"Failed to fetch data from ExerciseDB API: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing response from ExerciseDB API: {Message}", ex.Message);
                throw new Exception($"Failed to parse response from ExerciseDB API: {ex.Message}", ex);
            }
        }
    }
}
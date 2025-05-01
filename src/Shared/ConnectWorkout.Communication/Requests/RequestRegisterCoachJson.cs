namespace ConnectWorkout.Communication.Requests
{ // DTO que se encaixa no JSON de cadastro do Coach
    public class RequestRegisterCoachJson
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

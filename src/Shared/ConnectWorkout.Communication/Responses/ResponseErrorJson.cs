namespace ConnectWorkout.Communication.Responses
{    
    public class ResponseErrorJson
    { 
        public IList<string> Errors { get; set; }

        // Construtor que aceita uma lista de erros (quando há múltiplos)
        public ResponseErrorJson(IList<string> errors)
        {
            Errors = errors;
        }

        // Construtor que aceita apenas um erro (evita criar lista manualmente)
        public ResponseErrorJson(string error)
        {
            // Equivale a: new List<string> { error }
            Errors = [error];
        }
    }
}

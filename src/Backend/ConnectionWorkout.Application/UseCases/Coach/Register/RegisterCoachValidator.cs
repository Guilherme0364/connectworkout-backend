using ConnectWorkout.Communication.Requests;
using FluentValidation;

namespace ConnectWorkout.Application.UseCases.Coach.Register
{ // Classe de validação para cadastro do Coach
    public class RegisterCoachValidator : AbstractValidator<RequestRegisterCoachJson>
    {
        public RegisterCoachValidator()
        {
            RuleFor(coach => coach.Name).NotEmpty().WithMessage("O nome não deve ser vazio.");

            RuleFor(coach => coach.Email).NotEmpty().WithMessage("O e-mail não deve ser vazio.");

            RuleFor(coach => coach.Password.Length).GreaterThanOrEqualTo(6).WithMessage("A senha deve conter 6 caracteres pelo menos.");

            // Se o e-mail não for nulo (negação de IsNullOrEmpty), então
            When(coach => string.IsNullOrEmpty(coach.Email) == false, () =>
            {
                RuleFor(coach => coach.Email).EmailAddress().WithMessage("Digite um e-mail válido.");
            });
        }            
    }
}

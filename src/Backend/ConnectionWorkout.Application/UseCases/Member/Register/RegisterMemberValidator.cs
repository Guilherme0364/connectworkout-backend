using ConnectWorkout.Communication.Requests;
using FluentValidation;

namespace ConnectWorkout.Application.UseCases.Member.Register
{
    public class RegisterMemberValidator : AbstractValidator<RequestRegisterMemberJson>
    {
        public RegisterMemberValidator() 
        {
            RuleFor(member => member.Name).NotEmpty().WithMessage("O nome não deve ser vazio.");

            RuleFor(member => member.Email).NotEmpty().WithMessage("O e-mail não deve ser vazio.");

            RuleFor(member => member.Password.Length).GreaterThanOrEqualTo(6).WithMessage("A senha deve conter 6 caracteres pelo menos.");

            // Se o e-mail não for nulo (negação de IsNullOrEmpty), então
            When(member => string.IsNullOrEmpty(member.Email) == false, () =>
            {
                RuleFor(member => member.Email).EmailAddress().WithMessage("Digite um e-mail válido.");
            });
        }
    }
}

using FluentValidation;

namespace CollectManagement.Application.Features.Utilisateurs.Queries.LoginCheck;

public class LoginQueryValidator
    : AbstractValidator<LoginCheckQuery>
{
    public LoginQueryValidator()
    {
        
    }
}
using FluentValidation;

namespace CollectManagement.Application.Features.Utilisateurs.Commands.CreateUtilisateur;

public class CreateUtilisateurCommandValidator
    : AbstractValidator<CreateUtilisateurCommand>
{
    public CreateUtilisateurCommandValidator()
    {
        RuleFor(r => r.NomUtilisateur)
            .NotEmpty()
            .NotNull()
            .WithMessage("Username is required.")
            // Accepte les lettres (y compris accentuées), chiffres et certains caractères spéciaux
            .Matches(@"^[\p{L}\p{N}_\.\-]*$") 
            .WithMessage("Username can contain letters, numbers, underscores, dots, and dashes.");


        RuleFor(r => r.Nom)
            .NotEmpty()
            .NotNull()
            .WithMessage("Nom is required.");

        RuleFor(r => r.Prenom)
            .NotEmpty()
            .NotNull()
            .WithMessage("Prenom is required.");

        RuleFor(r => r.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid Email.");

        RuleFor(r => r.Password)
            .NotEmpty()
            .NotNull()
            .WithMessage("Passwoed is required.");
    }
}
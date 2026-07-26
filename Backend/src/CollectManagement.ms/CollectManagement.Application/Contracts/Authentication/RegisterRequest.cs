namespace CollectManagement.Application.Contracts.Authentication;

public record RegisterModeratorRequest(
    string NomUtilisateur,
    string Nom,
    string Prenom,
    string Email);
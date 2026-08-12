using CollectManagement.Domain.Common;
using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagementDomain.Societes;
using CollectManagementDomain.Societes.ValueObjects;

namespace CollectManagement.Domain.Utilisateurs;

public sealed class Utilisateur : AuditableEntity
{
    public UtilisateurId UtilisateurId { get; private set; }

    public string NomUtilisateur { get; private set; }

    public string Nom { get; private set; }

    public string Prenom { get; private set; }

    public string Email { get; private set; }

    public string Password { get; private set; }

    public RoleUtilisateurId? RoleUtilisateurId { get; private set; }

    public RoleUtilisateur? RoleUtilisateur { get; private set; }

    public bool IsActive { get; private set; }

    public SocieteId SocieteId { get; private set; }

    public Societe? Societe { get; private set; }

    private Utilisateur(
        UtilisateurId utilisateurId,
        string nomUtilisateur,
        string nom,
        string prenom,
        string email,
        string password,
        RoleUtilisateurId? roleUtilisateurId,
        bool isActive,
        SocieteId societeId)
    {
        UtilisateurId = utilisateurId;
        NomUtilisateur = nomUtilisateur;
        Nom = nom;
        Prenom = prenom;
        Email = email;
        Password = password;
        RoleUtilisateurId = roleUtilisateurId;
        IsActive = isActive;
        SocieteId = societeId;
    }

    public static Utilisateur Create(
        UtilisateurId utilisateurId,
        string nomUtilisateur,
        string nom,
        string prenom,
        string email,
        string password,
        RoleUtilisateurId? roleUtilisateurId,
        bool isActive,
        SocieteId societeId)
    {
        return new Utilisateur(
            utilisateurId,
            nomUtilisateur,
            nom,
            prenom,
            email,
            password,
            roleUtilisateurId,
            isActive,
            societeId);
    }

    public static Utilisateur QueryCreate(
        UtilisateurId utilisateurId,
        string nomUtilisateur,
        string nom,
        string prenom,
        string email,
        string password,
        RoleUtilisateurId? roleUtilisateurId,
        bool isActive,
        SocieteId societeId)
    {
        return new Utilisateur(
            utilisateurId,
            nomUtilisateur,
            nom,
            prenom,
            email,
            password,
            roleUtilisateurId,
            isActive,
            societeId);
    }

    public void Update(
        string nomUtilisateur,
        string nom,
        string prenom,
        string email,
        string password,
        RoleUtilisateurId? roleUtilisateurId,
        bool isActive,
        SocieteId societeId)
    {
        NomUtilisateur = nomUtilisateur;
        Nom = nom;
        Prenom = prenom;
        Email = email;
        Password = password;
        RoleUtilisateurId = roleUtilisateurId;
        IsActive = isActive;
        SocieteId = societeId;
    }

    public void Update(RoleUtilisateur roleUtilisateur)
    {
        RoleUtilisateur = roleUtilisateur;
    }

#pragma warning disable CS8618
    private Utilisateur()
    {
    }
#pragma warning restore CS8618
}
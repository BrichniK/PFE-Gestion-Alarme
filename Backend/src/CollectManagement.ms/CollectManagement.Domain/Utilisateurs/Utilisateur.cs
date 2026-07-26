using System.Runtime.CompilerServices;
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
    
    public bool IsActive { get; private set; } = true;
    
    public SocieteId SocieteId { get; private set; } // Clé étrangère
    public Societe? Societe { get; private set; }    // Navigation Property

      private Utilisateur(UtilisateurId utilisateurId,
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
            utilisateurId: utilisateurId,
            nomUtilisateur: nomUtilisateur,
            nom: nom,
            prenom: prenom,
            email: email,
            password: password,
            roleUtilisateurId: roleUtilisateurId,
            isActive: isActive,
            societeId:societeId);
    }

    public static Utilisateur Create(UtilisateurId utilisateurId,
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
            utilisateurId: utilisateurId,
            nomUtilisateur: nomUtilisateur,
            nom: nom,
            prenom: prenom,
            email: email,
            password: password,
            roleUtilisateurId: roleUtilisateurId,
            isActive: isActive,
            societeId:societeId);
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
        Password =  password;
        RoleUtilisateurId = roleUtilisateurId;
        IsActive = isActive;
        SocieteId = societeId;
    }

    public void Update(RoleUtilisateur roleUtilisateur)
    {
        RoleUtilisateur = roleUtilisateur;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Utilisateur() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}

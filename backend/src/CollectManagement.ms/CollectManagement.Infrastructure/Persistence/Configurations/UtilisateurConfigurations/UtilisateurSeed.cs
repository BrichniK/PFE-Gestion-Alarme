using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagementDomain.Societes.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Configurations.UtilisateurConfigurations;

public static class UtilisateurSeed
{
    public static List<Utilisateur> Data =>
        new()
        {
            Utilisateur.Create(
                utilisateurId: new UtilisateurId(Ulid.Parse("01HC85BM5QVRW7ABRV33TR1GQ0")),
                nomUtilisateur: "root",
                nom:"CST",
                prenom:"CST",
                email:"root@cst.tn",
                
                //aymen
                password:"E2CF9A6F4CFCA46F74FC0E4CF7A5B278D3C20D9178E0DB936DBB3CF8E614C89E4D1C33229F39A457014D2D581CAA3DCE7F49C53803A176A4F891A9EB1D5A34BA",
                null,
                true,
                societeId:new SocieteId(Ulid.Parse("01HC85BM5QVRW7ABRV33TR1GQ0"))
                
            )
        };
    
}
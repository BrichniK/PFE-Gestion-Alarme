using CollectManagement.Application.Features.Societes.Commands.CreateSociete;
using CollectManagement.Application.Features.Societes.Queries.GetOneSociete;
using CollectManagement.Application.Features.Societes.Queries.GetPagedListSociete;
using CollectManagementDomain.Societes;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CollectManagement.Application.Features.Societes.Mapping;

public class SocieteMapping :IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Societe, CreateSocieteReponse>()
            .Map(d=>d.SocieteId,
                s=>s.SocieteId.Value)
            ;

        config.NewConfig<Societe, GetPagedListSocieteDto>()
            .Map(d=>d.SocieteId,
                s=>s.SocieteId.Value)
            .Map(d=>d.LogoPath,
                s=>s.LogoPath)
            .Map(d=>d.Nom,
                s=>s.Nom)
            .Map(d=>d.Capital,
                s=>s.Capital)
            .Map(d=>d.Rne,
                s=>s.Rne)
            .Map(d=>d.Adresse,
                s=>s.Adresse)
            .Map(d=>d.Telephone1,
                s=>s.Telephone1)
            .Map(d=>d.Telephone2,
                s=>s.Telephone2)
            .Map(d=>d.Fax1,
                s=>s.Fax1)
            .Map(d=>d.Fax2,
                s=>s.Fax2)
            .Map(d=>d.Email,
                s=>s.Email)
            .Map(d=>d.CodeSociete,
                s=>s.CodeSociete)
            ;

        config.NewConfig<Societe, GetOneSocieteResponse>()
            .Map(d=>d.SocieteId,
                s=>s.SocieteId.Value)
            .Map(d=>d.LogoPath,
                s=>s.LogoPath)
            .Map(d=>d.Nom,
                s=>s.Nom)
            .Map(d=>d.Capital,
                s=>s.Capital)
            .Map(d=>d.Rne,
                s=>s.Rne)
            .Map(d=>d.Adresse,
                s=>s.Adresse)
            .Map(d=>d.Telephone1,
                s=>s.Telephone1)
            .Map(d=>d.Telephone2,
                s=>s.Telephone2)
            .Map(d=>d.Fax1,
                s=>s.Fax1)
            .Map(d=>d.Fax2,
                s=>s.Fax2)
            .Map(d=>d.Email,
                s=>s.Email)
            .Map(d=>d.CodeSociete,
                s=>s.CodeSociete)
            ;
    }
}
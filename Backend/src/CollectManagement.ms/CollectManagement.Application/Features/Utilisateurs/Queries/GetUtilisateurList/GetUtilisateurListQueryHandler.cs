using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;

namespace CollectManagement.Application.Features.Utilisateurs.Queries.GetUtilisateurList;

public sealed class GetUtilisateurListQueryHandler
    : IRequestHandler<GetUtilisateurListQuery, GetUtilisateurListResponse>
{
    private readonly IUtilisateurRepository _utilisateurRepository;

    public GetUtilisateurListQueryHandler(IUtilisateurRepository utilisateurRepository)
    {
        _utilisateurRepository = utilisateurRepository;
    }

    public async Task<GetUtilisateurListResponse> Handle(GetUtilisateurListQuery request, CancellationToken cancellationToken)
    {
        var (utilisateurs, length) = await _utilisateurRepository
            .GetPagedListAsync(
                request.Search,
                request.Sort,
                request.Order,
                request.Page,
                request.Size,
                cancellationToken)
            .ConfigureAwait(false);

        return new GetUtilisateurListResponse(
            utilisateurs.Select(s=> new GetUtilisateurDto(
                s.UtilisateurId.Value,
                s.NomUtilisateur,
                s.Nom,
                s.Prenom,
                s.Email,
                s.RoleUtilisateurId?.Value,
                s.RoleUtilisateur?.LibelleRoleUtilisateur,
                s.IsActive,
                s.SocieteId.Value
            )).ToList(),
            length);
    }
}
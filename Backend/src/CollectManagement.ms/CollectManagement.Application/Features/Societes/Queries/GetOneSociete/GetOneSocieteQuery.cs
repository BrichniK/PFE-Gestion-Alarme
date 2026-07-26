namespace CollectManagement.Application.Features.Societes.Queries.GetOneSociete;

public record GetOneSocieteQuery(
    Ulid SocieteId
    ):IRequest<GetOneSocieteResponse>;
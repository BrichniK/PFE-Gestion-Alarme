namespace CollectManagement.Application.Features.Types.Queries.GetOneType;

public record GetOneTypeQuery(Ulid TypeId) : IRequest<GetOneTypeResponse>;

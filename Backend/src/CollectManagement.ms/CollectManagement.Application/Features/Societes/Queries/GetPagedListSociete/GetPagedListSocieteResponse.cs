namespace CollectManagement.Application.Features.Societes.Queries.GetPagedListSociete;

public record GetPagedListSocieteResponse(
    List<GetPagedListSocieteDto> Societes,
    int length
    );
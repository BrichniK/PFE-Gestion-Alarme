namespace CollectManagement.Application.Features.Groupes.Queries.GetPagedListGroupe;

public record GetPagedListGroupeResponse(
    List<GetPagedListGroupeDto> Groupes,
    int Length
);

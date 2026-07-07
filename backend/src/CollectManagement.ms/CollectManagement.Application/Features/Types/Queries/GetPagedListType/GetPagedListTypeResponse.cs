namespace CollectManagement.Application.Features.Types.Queries.GetPagedListType;

public record GetPagedListTypeResponse(
    List<GetPagedListTypeDto> Types,
    int Length
);

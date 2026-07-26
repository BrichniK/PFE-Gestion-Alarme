using CollectManagement.Application.Features.SMS.Queries.GetSMSList.DTOs;

namespace CollectManagement.Application.Features.SMS.Queries.GetSMSList;

public record GetSMSListResponse(
    IReadOnlyList<SMSDto> SMSs,
    int Length
);

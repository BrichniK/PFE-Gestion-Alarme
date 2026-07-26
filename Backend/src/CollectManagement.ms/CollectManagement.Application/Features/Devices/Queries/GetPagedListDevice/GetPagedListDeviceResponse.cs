namespace CollectManagement.Application.Features.Devices.Queries.GetPagedListDevice;

public record GetPagedListDeviceResponse(
    List<GetPagedListDeviceDto> Devices,
    int Length
);

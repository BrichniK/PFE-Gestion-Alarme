namespace CollectManagement.Application.Features.Devices.Queries.GetOneDevice;

public record GetOneDeviceQuery(Ulid DeviceId) : IRequest<GetOneDeviceResponse>;

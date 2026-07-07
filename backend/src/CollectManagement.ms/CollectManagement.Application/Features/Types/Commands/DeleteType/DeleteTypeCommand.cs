namespace CollectManagement.Application.Features.Types.Commands.DeleteType;

public record DeleteTypeCommand(Ulid TypeId) : IRequest;

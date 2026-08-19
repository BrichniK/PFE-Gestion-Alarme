using MediatR;

namespace CollectManagement.Application.Features.AI.Chat;

public sealed record ChatWithAiQuery(
    string Message
) : IRequest<ChatWithAiResponse>;
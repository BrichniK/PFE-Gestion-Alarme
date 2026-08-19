using Carter;
using CollectManagement.Application.Common;
using CollectManagement.Application.Features.AI.Chat;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CollectManagement.WebAPI.EndPoints;

public sealed class AiEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "cm/ai/chat",
                async (
                    [FromBody] ChatWithAiQuery query,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var response = await sender.Send(
                        query,
                        cancellationToken);

                    return Results.Ok(
                        new ApiResponse<ChatWithAiResponse>(
                            response));
                })
            .WithName("AiChat")
            .WithTags("AI");
    }
}
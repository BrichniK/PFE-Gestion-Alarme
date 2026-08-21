using CollectManagement.Infrastructure.AI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CollectManagement.Tests.Infrastructure.Repositories.AI;

public class GeminiAiServiceTests
{
    private readonly Mock<ILogger<GeminiAiService>> _logger;

    public GeminiAiServiceTests()
    {
        _logger = new Mock<ILogger<GeminiAiService>>();
    }

    private GeminiAiService CreateService(
        string apiKey = "test-api-key",
        string model = "gemini-2.5-flash")
    {
        var options = Options.Create(
            new GeminiOptions
            {
                ApiKey = apiKey,
                Model = model
            });

        return new GeminiAiService(
            options,
            _logger.Object);
    }

    [Fact]
    public async Task GenerateResponseAsync_Should_Throw_When_ApiKey_Is_Missing()
    {
        // Arrange
        var service = CreateService(
            apiKey: "",
            model: "gemini-2.5-flash");

        // Act
        Func<Task> act = async () =>
            await service.GenerateResponseAsync(
                "Quel est l'état de MACHINE001 ?",
                "Contexte de test",
                CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Gemini API key is not configured.");
    }

    [Fact]
    public async Task GenerateResponseAsync_Should_Throw_When_UserMessage_Is_Empty()
    {
        // Arrange
        var service = CreateService();

        // Act
        Func<Task> act = async () =>
            await service.GenerateResponseAsync(
                "",
                "Contexte de test",
                CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*The user message cannot be empty.*");
    }

    [Fact]
    public async Task GenerateResponseAsync_Should_Throw_When_UserMessage_Is_Whitespace()
    {
        // Arrange
        var service = CreateService();

        // Act
        Func<Task> act = async () =>
            await service.GenerateResponseAsync(
                "   ",
                "Contexte de test",
                CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*The user message cannot be empty.*");
    }

    [Fact]
    public async Task GenerateResponseAsync_Should_Throw_When_Context_Is_Empty()
    {
        // Arrange
        var service = CreateService();

        // Act
        Func<Task> act = async () =>
            await service.GenerateResponseAsync(
                "Quel est l'état de MACHINE001 ?",
                "",
                CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*The AI context cannot be empty.*");
    }

    [Fact]
    public async Task GenerateResponseAsync_Should_Throw_When_Context_Is_Whitespace()
    {
        // Arrange
        var service = CreateService();

        // Act
        Func<Task> act = async () =>
            await service.GenerateResponseAsync(
                "Quel est l'état de MACHINE001 ?",
                "   ",
                CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*The AI context cannot be empty.*");
    }

    [Fact]
    public async Task GenerateResponseAsync_Should_Not_Throw_ApiKey_Exception_When_ApiKey_Is_Configured()
    {
        // Arrange
        var service = CreateService(
            apiKey: "fake-test-key");

        // Act
        Func<Task> act = async () =>
            await service.GenerateResponseAsync(
                "Question de test",
                "Contexte de test",
                CancellationToken.None);

        // Assert
        // Le but de ce test est uniquement de vérifier
        // que la validation locale de la clé API passe.
        //
        // L'appel Gemini peut ensuite échouer car nous
        // utilisons volontairement une fausse clé.
        //
        // Le service transforme cette erreur en
        // InvalidOperationException avec son message métier.
        var exception = await act.Should()
            .ThrowAsync<InvalidOperationException>();

        exception.Which.Message
            .Should()
            .Be("Le service d'intelligence artificielle est temporairement indisponible.");
    }
}
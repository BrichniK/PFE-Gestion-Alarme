using CollectManagement.Application.Features.Groupes.Commands.CreateGroupe;
using CollectManagement.Application.Interfaces.Groupes;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Groupes;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Groupes.Commands;

public class CreateGroupeCommandHandlerTests
{
    

    private readonly Mock<IGroupeRepository> _repository;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IImageService> _imageService;


    public CreateGroupeCommandHandlerTests()
    {
        _repository = new Mock<IGroupeRepository>();
        _mapper = new Mock<IMapper>();
        _imageService = new Mock<IImageService>();
    }
    [Fact]
    public async Task Handle_Should_Create_Groupe()
    {
        // Arrange

        var repository =
            new Mock<IGroupeRepository>();

        var mapper =
            new Mock<IMapper>();


        var response =
            new CreateGroupeResponse(
                Ulid.NewUlid()
            );


        mapper
            .Setup(x => x.Map<CreateGroupeResponse>(
                It.IsAny<Groupe>()))
            .Returns(response);



        _repository
            .Setup(x => x.AddAsync(
                It.IsAny<Groupe>(),
                It.IsAny<CancellationToken>()))
            .Returns((Groupe groupe, CancellationToken _) =>
                new ValueTask<Groupe>(groupe));



        var handler =
            new CreateGroupeCommandHandler(
                repository.Object,
                mapper.Object);



        var command =
            new CreateGroupeCommand(
                "Equipe A",
                "#FF0000",
                new List<Ulid>()
            );



        // Act

        var result =
            await handler.Handle(
                command,
                CancellationToken.None);



        // Assert


        result.Should()
            .NotBeNull();


        repository.Verify(
            x => x.AddAsync(
                It.IsAny<Groupe>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
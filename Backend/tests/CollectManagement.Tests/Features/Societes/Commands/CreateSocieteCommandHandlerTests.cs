using FluentAssertions;
using Mapster;
using Moq;
using CollectManagement.Application.Features.Societes.Commands.CreateSociete;
using CollectManagement.Application.Interfaces.Societes;
using CollectManagement.Application.Interfaces.Services;
using CollectManagementDomain.Societes;
using MapsterMapper;

namespace CollectManagement.Tests.Features.Societes.Commands;

public class CreateSocieteCommandHandlerTests
{
    private readonly Mock<ISocieteRepository> _repository;
    private readonly Mock<IImageService> _imageService;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IPasswordService> _passwordService;


    public CreateSocieteCommandHandlerTests()
    {
        _repository = new();
        _imageService = new();
        _mapper = new();
        _passwordService = new();
    }


    [Fact]
    public async Task Handle_Should_Create_Societe()
    {

        var response = new CreateSocieteReponse(
            Ulid.NewUlid()
        );


        _mapper
            .Setup(x => x.Map<CreateSocieteReponse>(It.IsAny<Societe>()))
            .Returns(response);


        var handler = new CreateSocieteCommandHandler(
            _repository.Object,
            _mapper.Object,
            _imageService.Object,
            Mock.Of<CollectManagement.Application.Interfaces.Repositories.Utilisateurs.IUtilisateurRepository>(),
            _passwordService.Object
        );


        var command = new CreateSocieteCommand(
            "",
            null,
            null,
            "Test Societe",
            "MF001",
            "RNE001",
            1000,
            DateTime.Now,
            "71111111",
            null,
            null,
            null,
            "test@test.com",
            "Adresse Test",
            "SOC001"
        );


        var result = await handler.Handle(
            command,
            CancellationToken.None
        );


        result.Should().NotBeNull();


        _repository.Verify(
            x=>x.AddAsync(
                It.IsAny<Societe>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

    }
}
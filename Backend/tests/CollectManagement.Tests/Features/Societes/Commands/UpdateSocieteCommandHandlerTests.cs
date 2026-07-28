using CollectManagement.Application.Features.Societes.Commands.UpdateSociete;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Application.Interfaces.Societes;
using CollectManagementDomain.Societes;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.Societes.Commands;


public class UpdateSocieteCommandHandlerTests
{

    private readonly Mock<ISocieteRepository> _repository;
    private readonly Mock<IImageService> _imageService;


    public UpdateSocieteCommandHandlerTests()
    {
        _repository = new Mock<ISocieteRepository>();

        _imageService = new Mock<IImageService>();
    }



    [Fact]
    public async Task Handle_Should_Update_Societe_When_Command_Is_Valid()
    {

        // Arrange

        var societeId = Ulid.NewUlid();


        var command = new UpdateSocieteCommand(

            SocieteId: societeId,

            LogoPath:"",
            LogoData:null,
            LogoExtension:null,

            Nom:"Societe Test",

            MatriculeFiscal:"MF001",

            Rne:"RNE001",

            Capital:5000,

            DateOverture:DateTime.Now,

            Telephone1:"71111111",

            Telephone2:null,

            Fax1:null,

            Fax2:null,

            Email:"test@test.com",

            Adresse:"Tunis",

            CodeSociete:"SOC001"

        );



        _repository
            .Setup(x=>x.UpdateBulkAsync(
                It.IsAny<Societe>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask);



        var handler =
            new UpdateSocieteCommandHandler(
                _repository.Object,
                _imageService.Object
            );



        // Act

        await handler.Handle(
            command,
            CancellationToken.None
        );



        // Assert


        _repository.Verify(
            x=>x.UpdateBulkAsync(
                It.IsAny<Societe>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

    }

}
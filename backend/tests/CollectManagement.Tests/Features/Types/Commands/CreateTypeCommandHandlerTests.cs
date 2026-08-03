using CollectManagement.Application.Features.Types.Commands.CreateType;
using CollectManagement.Application.Interfaces.Repositories.Types;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Types.Commands;

public class CreateTypeCommandHandlerTests
{
    private readonly Mock<ITypeRepository> _repository;
    private readonly Mock<IMapper> _mapper;


    public CreateTypeCommandHandlerTests()
    {
        _repository = new Mock<ITypeRepository>();
        _mapper = new Mock<IMapper>();
    }


    [Fact]
    public async Task Handle_Should_Create_Type()
    {
        var response = new CreateTypeResponse(
            Ulid.NewUlid()
        );


        _mapper
            .Setup(x => x.Map<CreateTypeResponse>(
                It.IsAny<CollectManagement.Domain.Types.Type>()))
            .Returns(response);



        var handler = new CreateTypeCommandHandler(
            _repository.Object,
            _mapper.Object);



        var command = new CreateTypeCommand(
            "ELEC",
            "Electricite",
            30
        );



        var result = await handler.Handle(
            command,
            CancellationToken.None);



        result.Should().NotBeNull();


        _repository.Verify(
            x => x.AddAsync(
                It.IsAny<CollectManagement.Domain.Types.Type>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
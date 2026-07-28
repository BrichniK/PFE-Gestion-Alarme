using CollectManagement.Application.Features.Employees.Commands.CreateEmployee;
using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Employess;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Employees.Commands;


public class CreateEmployeeCommandHandlerTests
{

    private readonly Mock<IEmployeeRepository> _repository;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IImageService> _imageService;


    public CreateEmployeeCommandHandlerTests()
    {
        _repository = new Mock<IEmployeeRepository>();
        _mapper = new Mock<IMapper>();
        _imageService = new Mock<IImageService>();
    }



    [Fact]
    public async Task Handle_Should_Create_Employee_Without_Image()
    {

        var command = new CreateEmployeeCommand(
            "Khalil",
            "Brichni",
            71111111,
            "RFID001",
            "test@test.com",
            null,
            null,
            null
        );


        _repository
            .Setup(x => x.AddAsync(
                It.IsAny<Employee>(),
                It.IsAny<CancellationToken>()))
            .Returns((Employee employee, CancellationToken _) =>
                new ValueTask<Employee>(employee));



        _mapper
            .Setup(x => x.Map<CreateEmployeeResponse>(
                It.IsAny<Employee>()))
            .Returns(new CreateEmployeeResponse(Ulid.NewUlid()));



        var handler = new CreateEmployeeCommandHandler(
            _repository.Object,
            _mapper.Object,
            _imageService.Object
        );


        var result = await handler.Handle(
            command,
            CancellationToken.None);



        result.Should().NotBeNull();


        _repository.Verify(
            x=>x.AddAsync(
                It.IsAny<Employee>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

    }

}
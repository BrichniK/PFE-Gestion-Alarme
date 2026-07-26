using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;

using IMapper = MapsterMapper.IMapper;

namespace CollectManagement.Application.Features.Employees.Commands.CreateEmployee;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, CreateEmployeeResponse>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;
    private readonly IImageService _imageService;

    public CreateEmployeeCommandHandler(
        IEmployeeRepository employeeRepository,
        IMapper mapper,
        IImageService imageService)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
        _imageService = imageService;
    }
    
    
    public async Task<CreateEmployeeResponse> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        string? imageName = null;
        var employeeId = new EmployeeId(Ulid.NewUlid());

        if (request.LogoData is not null)
        {
            imageName = $"{employeeId.Value}.{request.LogoExtension}";
            await _imageService.SaveImage(
                request.LogoData,
                "employee",
                imageName,
                cancellationToken
            ).ConfigureAwait(false);
        }

        var employee = Employee.Create(
            employeeId,
            request.Nom,
            request.Prenom,
            request.Phone,
            request.Rfid,
            request.Email,
            imageName
        );

        await _employeeRepository
            .AddAsync(employee, cancellationToken)
            .ConfigureAwait(false);

        return _mapper.Map<CreateEmployeeResponse>(employee);
    }
}


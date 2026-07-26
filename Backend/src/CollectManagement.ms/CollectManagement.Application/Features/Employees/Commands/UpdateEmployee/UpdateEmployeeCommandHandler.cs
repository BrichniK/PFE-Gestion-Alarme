using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;

namespace CollectManagement.Application.Features.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand>

{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IImageService _imageService;

    public UpdateEmployeeCommandHandler(
        IEmployeeRepository employeeRepository,
        IImageService imageService)
    {
        _employeeRepository = employeeRepository;
        _imageService = imageService;
    }

    public async Task Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employeeId = new EmployeeId(request.EmployeeId);
        string? newImageName = request.LogoPath;
        if (!string.IsNullOrEmpty(request.LogoData) && !string.IsNullOrEmpty(request.LogoExtension))
        {
            newImageName = $"{request.EmployeeId}.{request.LogoExtension}";
            await _imageService.SaveImage(
                request.LogoData,
                "employee",
                newImageName,
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
            newImageName
        );

        await _employeeRepository.UpdateBulkAsync(employee, cancellationToken)
            .ConfigureAwait(false);
    }
}
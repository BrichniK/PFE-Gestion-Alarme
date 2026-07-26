using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Domain.Employess.ObjectValues;

namespace CollectManagement.Application.Features.Employees.Queries.GetOneEmployee;

public class GetOneEmployeeQueryHandler
    : IRequestHandler<GetOneEmployeeQuery, GetOneEmployeeResponse>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;

    public GetOneEmployeeQueryHandler(IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }

    public async Task<GetOneEmployeeResponse> Handle(GetOneEmployeeQuery request, CancellationToken cancellationToken)
    {
        var employeeId = new EmployeeId(request.EmployeeId);

        var employee = await _employeeRepository
            .GetOneAsync(employeeId, cancellationToken)
            .ConfigureAwait(false) ?? throw new NotFoundException("Employee NotFound.");

        return _mapper.Map<GetOneEmployeeResponse>(employee);
    }
}

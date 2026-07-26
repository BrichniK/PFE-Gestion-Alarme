using CollectManagement.Application.Interfaces.Employees;


namespace CollectManagement.Application.Features.Employees.Queries.GetPagedListEmployee;

public class GetPagedListEmployeeQueryHandler
    : IRequestHandler<GetPagedListEmployeeQuery, GetPagedListEmployeeResponse>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;

    public GetPagedListEmployeeQueryHandler(IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }

    public async Task<GetPagedListEmployeeResponse> Handle(GetPagedListEmployeeQuery request, CancellationToken cancellationToken)
    {
        var (listEmployee, length) = await _employeeRepository
            .GetPagedListAsync(
                request.Search,
                request.Sort,
                request.Order,
                request.Page,
                request.Size,
                cancellationToken
            )
            .ConfigureAwait(false);

        return new GetPagedListEmployeeResponse(
            _mapper.Map<List<GetPagedListEmployeeDto>>(listEmployee),
            length
        );
    }
}

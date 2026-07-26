using CollectManagement.Application.Interfaces.Repositories.JoursFeries;
using CollectManagement.Domain.JoursFeries;
using CollectManagement.Domain.JoursFeries.ValueObjects;

namespace CollectManagement.Application.Features.JoursFeries.Commands.CreateJourFerie;

public class CreateJourFerieCommandHandler
    : IRequestHandler<CreateJourFerieCommand, CreateJourFerieResponse>
{
    private readonly IJourFerieRepository _jourFerieRepository;
    private readonly IMapper _mapper;

    public CreateJourFerieCommandHandler(
        IJourFerieRepository jourFerieRepository,
        IMapper mapper)
    {
        _jourFerieRepository = jourFerieRepository;
        _mapper = mapper;
    }

    public async Task<CreateJourFerieResponse> Handle(CreateJourFerieCommand request, CancellationToken cancellationToken)
    {
        var jourFerieId = new JourFerieId(Ulid.NewUlid());

        var jourFerie = JourFerie.Create(
            jourFerieId,
            request.Date,
            request.Label
        );

        await _jourFerieRepository
            .AddAsync(jourFerie, cancellationToken)
            .ConfigureAwait(false);

        return _mapper.Map<CreateJourFerieResponse>(jourFerie);
    }
}

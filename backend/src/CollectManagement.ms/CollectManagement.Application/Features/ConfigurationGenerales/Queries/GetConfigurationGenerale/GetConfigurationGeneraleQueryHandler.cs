using CollectManagement.Application.Interfaces.Repositories.ConfigurationGenerales;

namespace CollectManagement.Application.Features.ConfigurationGenerales.Queries.GetConfigurationGenerale;

public class GetConfigurationGeneraleQueryHandler
    : IRequestHandler<GetConfigurationGeneraleQuery, GetConfigurationGeneraleResponse>
{
    private readonly IConfigurationGeneraleRepository _configurationGeneraleRepository;

    public GetConfigurationGeneraleQueryHandler(IConfigurationGeneraleRepository configurationGeneraleRepository)
    {
        _configurationGeneraleRepository = configurationGeneraleRepository;
    }

    public async Task<GetConfigurationGeneraleResponse> Handle(
        GetConfigurationGeneraleQuery request,
        CancellationToken cancellationToken)
    {
        var config = await _configurationGeneraleRepository
            .GetConfigurationAsync(cancellationToken)
            .ConfigureAwait(false);

        if (config is null)
        {
            return new GetConfigurationGeneraleResponse(null, false, false, true, true, 1d, 1d, 1d, 1d);
        }

        return new GetConfigurationGeneraleResponse(
            config.ConfigurationGeneraleId.Value,
            config.EcraserEmployeMaintenance,
            config.AccepterSeulementEmployesPlanifies,
            config.DiagnostiqueObligatoire,
            config.MonitoringPourcentageSurSommeDurees,
            config.CoefficientGaugeD1,
            config.CoefficientGaugeD2,
            config.CoefficientGaugeD3,
            config.CoefficientGaugeD4
        );
    }
}

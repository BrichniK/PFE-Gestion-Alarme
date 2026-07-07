using CollectManagement.Application.Interfaces.Repositories.ConfigurationGenerales;
using CollectManagement.Domain.ConfigurationGenerales;
using CollectManagement.Domain.ConfigurationGenerales.ValueObjects;

namespace CollectManagement.Application.Features.ConfigurationGenerales.Commands.UpdateConfigurationGenerale;

public class UpdateConfigurationGeneraleCommandHandler
    : IRequestHandler<UpdateConfigurationGeneraleCommand, UpdateConfigurationGeneraleResponse>
{
    private readonly IConfigurationGeneraleRepository _configurationGeneraleRepository;

    public UpdateConfigurationGeneraleCommandHandler(IConfigurationGeneraleRepository configurationGeneraleRepository)
    {
        _configurationGeneraleRepository = configurationGeneraleRepository;
    }

    public async Task<UpdateConfigurationGeneraleResponse> Handle(
        UpdateConfigurationGeneraleCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _configurationGeneraleRepository
            .GetConfigurationAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            var newId = new ConfigurationGeneraleId(Ulid.NewUlid());
            var config = ConfigurationGenerale.Create(
                newId,
                request.EcraserEmployeMaintenance,
                request.AccepterSeulementEmployesPlanifies,
                request.DiagnostiqueObligatoire,
                request.MonitoringPourcentageSurSommeDurees,
                request.CoefficientGaugeD1,
                request.CoefficientGaugeD2,
                request.CoefficientGaugeD3,
                request.CoefficientGaugeD4);

            await _configurationGeneraleRepository
                .AddAsync(config, cancellationToken)
                .ConfigureAwait(false);

            return new UpdateConfigurationGeneraleResponse(newId.Value);
        }

        existing.Update(
            request.EcraserEmployeMaintenance,
            request.AccepterSeulementEmployesPlanifies,
            request.DiagnostiqueObligatoire,
            request.MonitoringPourcentageSurSommeDurees,
            request.CoefficientGaugeD1,
            request.CoefficientGaugeD2,
            request.CoefficientGaugeD3,
            request.CoefficientGaugeD4);
        await _configurationGeneraleRepository
            .UpdateBulkAsync(existing, cancellationToken)
            .ConfigureAwait(false);

        return new UpdateConfigurationGeneraleResponse(existing.ConfigurationGeneraleId.Value);
    }
}

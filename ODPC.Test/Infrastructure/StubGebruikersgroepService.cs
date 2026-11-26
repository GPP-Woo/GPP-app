using ODPC.Features;
using ODPC.Features.Publicaties;

namespace ODPC.Test.Infrastructure
{
    public class StubGebruikersgroepService(IReadOnlyList<string> waardelijstUuids, bool isGebruiker = true) : IGebruikersgroepService
    {
        public Task<IReadOnlyList<string>> GetWaardelijstUuidsAsync(Guid? gebruikersgroepUuid, CancellationToken token) =>
            Task.FromResult(waardelijstUuids);

        public Task<bool> IsGebruikersgroepGebruikerAsync(Guid gebruikersgroepUuid, CancellationToken token) =>
            Task.FromResult(isGebruiker);

        public Task<EigenaarGroep?> TryAndGetEigenaarGroepFromOdpcAsync(Guid publicatieUuid, CancellationToken token) =>
            Task.FromResult<EigenaarGroep?>(null);

        public Task<bool> IsGeautoriseerdVoorInzageProcedureAsync(Guid? gebruikersgroepUuid, CancellationToken token) =>
            Task.FromResult(true);
    }
}

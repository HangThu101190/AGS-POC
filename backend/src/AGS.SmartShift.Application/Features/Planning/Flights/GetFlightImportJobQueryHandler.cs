using AGS.SmartShift.Application.Contracts.Planning;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Planning.Flights;

public sealed class GetFlightImportJobQueryHandler : IRequestHandler<GetFlightImportJobQuery, FlightImportJobDto?>
{
    private readonly IFlightImportJobRepository _jobs;

    public GetFlightImportJobQueryHandler(IFlightImportJobRepository jobs) => _jobs = jobs;

    public async Task<FlightImportJobDto?> Handle(GetFlightImportJobQuery request, CancellationToken cancellationToken)
    {
        var job = await _jobs.GetByIdAsync(request.JobId, cancellationToken).ConfigureAwait(false);
        if (job is null)
        {
            return null;
        }

        var result = EnqueueFlightImportCommandHandler.TryParseResult(job.ResultJson);
        return EnqueueFlightImportCommandHandler.Map(job, result);
    }
}

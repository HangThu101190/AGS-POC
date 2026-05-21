using MediatR;

namespace AGS.SmartShift.Application.Features.Attendance;

public sealed record PostLocationSamplesCommand(IReadOnlyList<LocationSampleInput> Samples)
    : IRequest<LocationSamplesResultDto>;

public sealed class LocationSampleInput
{
    public double Lat { get; init; }
    public double Lng { get; init; }
    public DateTime? CapturedAtUtc { get; init; }
}

public sealed class LocationSamplesResultDto
{
    public int Accepted { get; init; }
    public DateTime? LastCapturedAtUtc { get; init; }
}

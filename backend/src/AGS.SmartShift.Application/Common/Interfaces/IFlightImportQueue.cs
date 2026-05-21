namespace AGS.SmartShift.Application.Common.Interfaces;

public sealed record FlightImportWorkItem(
    Guid JobId,
    string WeekId,
    int? DayIdx,
    byte[] FileBytes);

public interface IFlightImportQueue
{
    ValueTask EnqueueAsync(FlightImportWorkItem item, CancellationToken cancellationToken = default);

    IAsyncEnumerable<FlightImportWorkItem> ReadAllAsync(CancellationToken cancellationToken);
}

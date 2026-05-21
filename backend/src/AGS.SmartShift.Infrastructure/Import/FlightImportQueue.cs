using System.Threading.Channels;
using AGS.SmartShift.Application.Common.Interfaces;

namespace AGS.SmartShift.Infrastructure.Import;

public sealed class FlightImportQueue : IFlightImportQueue
{
    private readonly Channel<FlightImportWorkItem> _channel = Channel.CreateUnbounded<FlightImportWorkItem>();

    public ValueTask EnqueueAsync(FlightImportWorkItem item, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(item, cancellationToken);

    public IAsyncEnumerable<FlightImportWorkItem> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}

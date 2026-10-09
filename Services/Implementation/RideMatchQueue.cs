using Services.Interfaces;
using System.Threading.Channels;

namespace Services.Implementation
{
    public class RideMatchQueue : IRideMatchQueue
    {
        private readonly Channel<Guid> channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

        public ValueTask EnqueueAsync(Guid rideId, CancellationToken cancellationToken = default) => channel.Writer.WriteAsync(rideId, cancellationToken);

        public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken) => channel.Reader.ReadAllAsync(cancellationToken);
    }
}

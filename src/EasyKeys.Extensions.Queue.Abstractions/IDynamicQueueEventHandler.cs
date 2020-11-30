using System.Threading;
using System.Threading.Tasks;

namespace EasyKeys.Extensions.Queue.Abstractions
{
    public interface IDynamicQueueEventHandler
    {
        Task HandleAsync(dynamic eventData, CancellationToken cancellationToken = default);
    }
}

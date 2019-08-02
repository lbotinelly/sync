using System.Threading.Tasks;

namespace Sync.Service
{
    public interface ISyncService
    {
        Task<ISyncServiceResult> Start();
    }
}
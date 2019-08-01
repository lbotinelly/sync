using Microsoft.Extensions.Options;

namespace Sync.Service
{
    public class SyncService : ISyncService
    {
        #region Implementation of ISyncService

        public void DoSync() { }

        private SyncServiceOptions _options;

        public SyncService(IOptionsMonitor<SyncServiceOptions> options) { _options = options.CurrentValue; }

        #endregion
    }
}
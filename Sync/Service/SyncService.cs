using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Sync.Service
{
    public class SyncService : ISyncService
    {
        public class Result : ISyncServiceResult { }


        #region Implementation of ISyncService

        private SyncServiceOptions _options;
        private ILogger<SyncService> _log;

        public SyncService(IOptionsMonitor<SyncServiceOptions> options, ILogger<SyncService> logger)
        {
            _options = options.CurrentValue;
            _log = logger;
        }

        #endregion

        #region Implementation of ISyncService

        public async Task<ISyncServiceResult> Start()
        {
            var result = new Result();

            Log.Information("Starting");

            var taskQueue = new List<Task>();


            // Preparing monitor tasks
            foreach (var optionsGroup in _options.Groups)
            {
                taskQueue.Add(new GroupMonitor(optionsGroup).Monitor());
            }

            await Task.WhenAll(taskQueue);

            return result;

        }

        #endregion
    }
}
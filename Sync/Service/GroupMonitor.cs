using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Sync.Service
{
    public class GroupMonitor
    {
        private readonly SyncServiceOptions.Group _optionsGroup;

        private readonly Dictionary<FileSystemWatcher, string> _watcherMap = new Dictionary<FileSystemWatcher, string>();

        private readonly Timer CooldownTimer = new Timer();
        private EState State = EState.Waiting;

        private void SwitchWatchers(bool state)
        {
            foreach (var watcherMapKey in _watcherMap.Keys) { watcherMapKey.EnableRaisingEvents = state; }
        }

        public GroupMonitor(SyncServiceOptions.Group optionsGroup)
        {

            _optionsGroup = optionsGroup;

            CooldownTimer = new Timer(_optionsGroup.Cooldown.Seconds * 1000) { AutoReset = true };
            CooldownTimer.Elapsed += CooldownEvent;
        }

        private FileSystemWatcher EventSource { get; set; }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // A change happened. Two possible scenarios:

            // We were waiting for something to happen, so now let's enter cooldown; if no more changes happen then start sync'ing

            // We're syncing, and must ignore changes.

            if (_optionsGroup.Ignore?.Contains(e.Name) == true)
            {
                Log.Information("Ignored: " + e.ChangeType + " " + e.FullPath);
                return;
            }


            switch (State)
            {
                case EState.Waiting:



                    Log.Debug(e.ChangeType + " " + e.FullPath);
                    PrepareForSync((FileSystemWatcher)source);

                    break;
                case EState.CoolingDown:
                    CooldownTimer.Stop();
                    CooldownTimer.Start();
                    break;
                case EState.Syncing:
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void PrepareForSync(FileSystemWatcher source)
        {

            if (_optionsGroup.Unidirectional && _watcherMap.Keys.ToList()[0] != source) return;

            EventSource = source;

            //Log.Information("SYNCSCHED " + _watcherMap[source]);

            State = EState.CoolingDown;
            CooldownTimer.Enabled = true;
            CooldownTimer.Stop();
            CooldownTimer.Start();
        }

        public Task Monitor()
        {
            // Prepare watchers

            foreach (var folder in _optionsGroup.Folders)
            {
                var watcher = new FileSystemWatcher
                {
                    Path = folder,
                    NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    Filter = "*.*"
                };

                watcher.Changed += OnChanged;

                _watcherMap.Add(watcher, folder);
            }

            //Done? Let's do this.

            SwitchWatchers(true);

            if (_optionsGroup.Unidirectional) PrepareForSync(_watcherMap.Keys.ToList()[0]);

            Log.Information($"{_optionsGroup.Mode.ToString()}ing:");
            foreach (var f in _optionsGroup.Folders)
            {
                Log.Debug($"    {f}");

            }

            return Task.CompletedTask;
        }

        private void CooldownEvent(object source, ElapsedEventArgs e)
        {
            State = EState.Syncing;

            CooldownTimer.Stop();
            CooldownTimer.Enabled = false;

            // We reached Cooldown. Let's start copying.

            // Determine the source

            var _source = _watcherMap[EventSource];

            var _destinations = _watcherMap.Where(i => i.Key != EventSource).Select(i => i.Value).ToList();

            // Now sync all destinations.

            SwitchWatchers(false);

            foreach (var destination in _destinations)
            {

                Log.Information($"{_optionsGroup.Mode.ToString().ToUpper()} {_source} -> {destination}");

                var process = _optionsGroup.Mode == SyncServiceOptions.Group.Emode.Upsert ? SyncProcess(_source, destination) : MirrorProcess(_source, destination);

                // Log.Debug(process.StartInfo.FileName + " " + process.StartInfo.Arguments);

                process.Start();
                process.WaitForExit();
            }

            State = EState.Waiting;

            SwitchWatchers(true);
        }

        private Process SyncProcess(string source, string destination)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "XCOPY",
                    Arguments = $"{source} {destination} /E /D /C /Y {(!_optionsGroup.IgnoreHidden ? " /H" : "")}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (sender, data) =>
            {
                Log.Information(data.Data);
            };
            process.ErrorDataReceived += (sender, data) =>
            {
                Log.Error(data.Data);
            };

            process.Exited += (sender, args) =>
            {
                Log.Information($"SYNCEND   {source} -> {destination}");
            };

            return process;
        }

        private Process MirrorProcess(string source, string destination)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "ROBOCOPY",
                    Arguments = $"\"{source}\" \"{destination}\" /MIR /FFT /R:0 /Z /NFL /NDL /XD \".svn\" \".git\"  {(_optionsGroup.IgnoreHidden ? " /XA:SHT" : " /XA:ST")}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (sender, data) => { Log.Information(data.Data); };
            process.ErrorDataReceived += (sender, data) => { Log.Error(data.Data); };

            process.Exited += (sender, args) =>
            {
                Log.Information($"SYNCEND   {source} -> {destination}");
            };

            return process;
        }

        private enum EState
        {
            Waiting,
            CoolingDown,
            Syncing
        }
    }
}
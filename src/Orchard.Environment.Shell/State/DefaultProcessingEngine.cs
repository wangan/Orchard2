using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Shell.Builders;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Shell.State
{
    public class DefaultProcessingEngine : Component, IProcessingEngine
    {
        private readonly IShellContextFactory _shellContextFactory;
        private readonly ILogger _logger;

        private readonly ContextState<IList<Entry>> _entries;

        public DefaultProcessingEngine(IShellContextFactory shellContextFactory,
            ILogger<DefaultProcessingEngine> logger)
        {
            _shellContextFactory = shellContextFactory;
            _logger = logger;

            _entries = new ContextState<IList<Entry>>("DefaultProcessingEngine.Entries", () => new List<Entry>());
        }

        public string AddTask(ShellSettings shellSettings, ShellDescriptor shellDescriptor, string eventName, Dictionary<string, object> parameters)
        {

            var entry = new Entry
            {
                ShellSettings = shellSettings,
                ShellDescriptor = shellDescriptor,
                MessageName = eventName,
                EventData = parameters,
                TaskId = Guid.NewGuid().ToString("n"),
                ProcessId = Guid.NewGuid().ToString("n"),
            };

            _logger.LogInformation("Adding event {0} to process {1} for shell {2}",
                eventName,
                entry.ProcessId,
                shellSettings.Name);

            _entries.GetState().Add(entry);

            return entry.ProcessId;
        }


        public class Entry
        {
            public string ProcessId { get; set; }
            public string TaskId { get; set; }

            public ShellSettings ShellSettings { get; set; }
            public ShellDescriptor ShellDescriptor { get; set; }
            public string MessageName { get; set; }
            public Dictionary<string, object> EventData { get; set; }
        }


        public bool AreTasksPending()
        {
            return _entries.GetState().Any();
        }

        public void ExecuteNextTask()
        {

            Entry entry;
            if (!_entries.GetState().Any())
                return;
            entry = _entries.GetState().First();
            _entries.GetState().Remove(entry);
            Execute(entry);
        }

        private void Execute(Entry entry)
        {
            var shellContext = _shellContextFactory.CreateDescribedContext(entry.ShellSettings, entry.ShellDescriptor);
            using (shellContext)
            {
                var eventBus = shellContext.ServiceProvider.GetService<IEventBus>();
                _logger.LogInformation("Executing event {0} in process {1} for shell {2}",
                                    entry.MessageName,
                                    entry.ProcessId,
                                    entry.ShellSettings.Name);
                eventBus.NotifyAsync(entry.MessageName, entry.EventData);
            }
        }
    }
}

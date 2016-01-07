﻿using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Environment.Shell.Descriptor.Settings
{
    public class ShellDescriptorManager : IShellDescriptorManager
    {
        private readonly ShellSettings _shellSettings;
        private readonly IEventBus _eventBus;
        private readonly ISession _session;
        private readonly ILogger _logger;

        public ShellDescriptorManager(
            ShellSettings shellSettings,
            IEventBus eventBus,
            ISession session,
            ILogger<ShellDescriptorManager> logger)
        {
            _shellSettings = shellSettings;
            _eventBus = eventBus;
            _session = session;
            _logger = logger;
        }

        public async Task<ShellDescriptor> GetShellDescriptor()
        {
            return await _session.QueryAsync<ShellDescriptor>().FirstOrDefault();
        }

        public async Task UpdateShellDescriptor(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters)
        {
            var shellDescriptorRecord = await GetShellDescriptor();
            var serialNumber = shellDescriptorRecord == null ? 0 : shellDescriptorRecord.SerialNumber;
            if (priorSerialNumber != serialNumber)
            {
                throw new InvalidOperationException("Invalid serial number for shell descriptor");
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Updating shell descriptor for shell '{0}'...", _shellSettings.Name);
            }
            if (shellDescriptorRecord == null)
            {
                shellDescriptorRecord = new ShellDescriptor { SerialNumber = 1 };
                _session.Save(shellDescriptorRecord);
            }
            else
            {
                shellDescriptorRecord.SerialNumber++;
            }

            shellDescriptorRecord.Features.Clear();
            foreach (var feature in enabledFeatures)
            {
                shellDescriptorRecord.Features.Add(new ShellFeature { Name = feature.Name });
            }
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Enabled features for shell '{0}' set: {1}.", _shellSettings.Name, string.Join(", ", enabledFeatures.Select(feature => feature.Name)));
            }

            shellDescriptorRecord.Parameters.Clear();
            foreach (var parameter in parameters)
            {
                shellDescriptorRecord.Parameters.Add(new ShellParameter
                {
                    Component = parameter.Component,
                    Name = parameter.Name,
                    Value = parameter.Value
                });
            }
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Parameters for shell '{0}' set: {1}.", _shellSettings.Name, string.Join(", ", parameters.Select(parameter => parameter.Name + "-" + parameter.Value)));
            }
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Shell descriptor updated for shell '{0}'.", _shellSettings.Name);
            }

            await _eventBus.NotifyAsync<IShellDescriptorManagerEventHandler>(e => e.Changed(shellDescriptorRecord, _shellSettings.Name));
        }
    }
}
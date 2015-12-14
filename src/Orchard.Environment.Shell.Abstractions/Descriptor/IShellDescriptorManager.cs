﻿using Orchard.DependencyInjection;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Environment.Shell.Descriptor
{
    /// <summary>
    /// Service resolved out of the shell container. Primarily used by host.
    /// </summary>
    public interface IShellDescriptorManager : IDependency
    {
        /// <summary>
        /// Uses shell-specific database or other resources to return
        /// the current "correct" configuration. The host will use this information
        /// to reinitialize the shell.
        /// </summary>
        Task<ShellDescriptor> GetShellDescriptor();

        /// <summary>
        /// Alters databased information to match information passed as arguments.
        /// Prior SerialNumber used for optimistic concurrency, and an exception
        /// should be thrown if the number in storage doesn't match what's provided.
        /// </summary>
        Task UpdateShellDescriptor(
            int priorSerialNumber,
            IEnumerable<ShellFeature> enabledFeatures,
            IEnumerable<ShellParameter> parameters);
    }

    public interface IShellDescriptorManagerEventHandler : IEventHandler
    {
        void Changed(ShellDescriptor descriptor, string tenant);
    }
}
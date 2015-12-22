using Orchard.Environment.Extensions.Features;
using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using System.Threading.Tasks;

namespace Orchard.Tests.Stubs
{
    public class StubFeatureManager : IFeatureManager
    {
        public FeatureDependencyNotificationHandler FeatureDependencyNotification
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public Task<IEnumerable<string>> DisableFeatures(IEnumerable<string> featureIds)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> DisableFeatures(IEnumerable<string> featureIds, bool force)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> EnableFeatures(IEnumerable<string> featureIds)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> EnableFeatures(IEnumerable<string> featureIds, bool force)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FeatureDescriptor>> GetAvailableFeatures()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetDependentFeatures(string featureId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FeatureDescriptor>> GetDisabledFeatures()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FeatureDescriptor>> GetEnabledFeatures()
        {
            throw new NotImplementedException();
        }
    }
}
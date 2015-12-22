using Orchard.Environment.Extensions.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Environment.Extensions.Features
{
    public static class FeatureManagerExtensions
    {
        public static async Task<Feature> GetFeature(this IFeatureManager featureManager, string id)
        {
            var features = await featureManager.GetAvailableFeatures();

            var feature = features.FirstOrDefault(x => x.Id == id);

            if (feature == null)
            {
                return null;
            }

            return new Feature
            {
                Descriptor = feature
            };
        }
    }
}
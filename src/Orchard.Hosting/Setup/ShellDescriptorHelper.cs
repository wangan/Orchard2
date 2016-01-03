using Orchard.Environment.Shell.Descriptor.Models;

namespace Orchard.Hosting.Setup
{
    public static class ShellDescriptorHelper
    {
        public static ShellDescriptor SetupDescriptor = new ShellDescriptor
        {
            SerialNumber = -1,
            Features = new[] {
                    new ShellFeature { Name = "Orchard.Logging.Console" },
                    new ShellFeature { Name = "Orchard.Setup" },
                    new ShellFeature { Name = "Orchard.Recipes" }
                },
        };
    }
}

using System.IO;
using ClientUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContractTester.Tests
{
    public static class Testing
    {
        private static readonly IServiceScopeFactory ScopeFactory;
        public static IConfigurationRoot Configuration { get; }

        static Testing()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            Configuration = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            var startup = new Startup(Configuration);
            var services = new ServiceCollection();

            startup.ConfigureServices(services);

            var rootContainer = services.BuildServiceProvider();
            ScopeFactory = rootContainer.GetService<IServiceScopeFactory>();
        }

        public static ContractTesterSettings GetContractTesterApplicationConfigurationSettings()
        {
            var contractTesterSettings = new ContractTesterSettings();
            var appSettings = Configuration.GetSection(nameof(ContractTesterSettings));
            appSettings.Bind(contractTesterSettings);

            return contractTesterSettings;
        }
    }
}
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Threading;

namespace TokenPriceUpdate.HttpApi.Client.ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var application = AbpApplicationFactory.Create<TokenPriceUpdateConsoleApiClientModule>(options =>
            {
                var builder = new ConfigurationBuilder();
                builder.AddJsonFile("appsettings.json", false);
                builder.AddJsonFile("appsettings.secrets.json", true);
                options.Services.ReplaceConfiguration(builder.Build());
            }))
            {
                application.Initialize();

                var demo = application.ServiceProvider.GetRequiredService<ClientDemoService>();
                AsyncHelper.RunSync(() => demo.RunAsync());

                Console.WriteLine("Press ENTER to stop application...");
                Console.ReadLine();
            }
        }
    }
}

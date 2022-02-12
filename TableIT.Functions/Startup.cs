using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

[assembly: FunctionsStartup(typeof(TableIT.Functions.Startup))]
namespace TableIT.Functions;
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services
            .AddMvcCore()
            .AddApplicationPart(Assembly.GetExecutingAssembly())
            .AddViewLocalization();
    }
}
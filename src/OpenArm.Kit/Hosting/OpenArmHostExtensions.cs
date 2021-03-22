using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    public static class HostExtensions
    {
        public static async Task InitializeRepositoriesAsync(this IHost host)
        {
            await host.Services.GetRequiredService<OpenArmServiceCollectionExtensions.RepositoryInitializer>().InititalizeAsync();
        }
    }
}
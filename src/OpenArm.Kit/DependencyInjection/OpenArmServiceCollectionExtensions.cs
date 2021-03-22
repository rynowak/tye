using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenArm.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OpenArmServiceCollectionExtensions
    {
        public static IServiceCollection AddRepository<TRepository, TImplmenentation>(this IServiceCollection services)
            where TRepository : class, IRepository
            where TImplmenentation : class, TRepository
        {
            services.TryAddSingleton<RepositoryInitializer>();

            services.TryAddSingleton<TRepository, TImplmenentation>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IRepository, TRepository>(s => s.GetRequiredService<TRepository>()));

            return services;
        }

        internal class RepositoryInitializer
        {
            private readonly IEnumerable<IRepository> repositories;

            public RepositoryInitializer(IEnumerable<IRepository> repositories)
            {
                this.repositories = repositories;
            }

            public async Task InititalizeAsync()
            {
                foreach (var repository in repositories)
                {
                    await repository.InitializeAsync();
                }
            }
        }
    }
}
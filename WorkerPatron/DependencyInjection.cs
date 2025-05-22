using Application;
using Infrastructure;

namespace WorkerPatron
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWorkerServices(this IServiceCollection services)
        {            
            services.AddHostedService<Worker>();
            services.AddHostedService<GuaneWorker>();
            
            services.AddApplication();            
            services.AddInfrastructure();

            return services;
        }
    }
}

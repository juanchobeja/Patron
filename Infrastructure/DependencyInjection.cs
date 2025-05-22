using Core.Abstractions;
using Core.Entities;
using Infrastructure.Connection;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Conexión a SQL Server
            services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

            // Repositorios
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IClientesRepository, ClientesRepository>();
            services.AddScoped<IFileTrazabilidadRepository, FileTrazabilidadRepository>();
            services.AddScoped<IFormatFileRepository, FormatFileRepository>();
            services.AddScoped<IConsecutivoEtiquetaRepository, ConsecutivoEtiquetaRepository>();
            services.AddScoped<IConfiguradorIntegradorRepository, ConfiguradorIntegradorRepository>();
            services.AddScoped<IFestivosRepository, FestivosRepository>();
            services.AddScoped<IRepository<Batch>, BatchRepository>();
            services.AddScoped<IRepository<Solicitud>, SolicitudRepository>();
            services.AddScoped<IRepository<Documento>, DocumentoRepository>();
            services.AddScoped<ILogRepository, LogRepository>();
            

            return services;
        }
    }
}

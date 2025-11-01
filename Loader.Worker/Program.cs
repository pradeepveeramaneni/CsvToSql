using Common.Observability;
using Loader.Sql;
using Loader.Worker.Services;
using Microsoft.EntityFrameworkCore;

namespace Loader.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            Logging.ConfigureSerilog(builder);

            builder.Services.AddDbContextFactory<SalesDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Sql")));

            builder.Services.Configure<LoaderSettings>(builder.Configuration.GetSection("Loader"));
            builder.Services.AddSingleton<LoaderService>();
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }

    public sealed class LoaderSettings
    {
        public required string KafkaBootstrap { get; init; }
        public string TopicIn { get; init; } = "csv.rows.transformed";
        public string GroupId { get; init; } = "loader-worker";
    }

    public sealed class Worker : BackgroundService
    {
        private readonly LoaderService _svc;
        public Worker(LoaderService svc)
        {
            _svc = svc;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => _svc.RunAsync(stoppingToken);
    }

}
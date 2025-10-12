
using Common.Observability;
using Confluent.Kafka;
using Ingest.Api.Services;

namespace Ingest.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Logging.ConfigureSerilog(builder);


            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //builder.Services.AddHealthChecks().AddKafka(builder.Configuration["Ingest:KafkaBootstrap"]);

            builder.Services.AddHealthChecks()
                .AddKafka(
                    new ProducerConfig
                    {
                        BootstrapServers = builder.Configuration["Ingest:KafkaBootstrap"]
                    },
                    name: "kafka",
                    timeout: TimeSpan.FromSeconds(5),
                    tags: new[] { "messaging", "broker" });

            builder.Services.Configure<IngestSettings>(builder.Configuration.GetSection("Ingest"));
            builder.Services.AddSingleton<IngestService>();


            builder.WebHost.ConfigureKestrel(o =>
            {
                o.Limits.MaxRequestBodySize = builder.Configuration.GetValue<long>("Ingest:MaxFileSizeBytes", 524_288_000);
                o.AddServerHeader = false;
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }



            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapHealthChecks("/health");


            app.MapControllers();

            app.Run();
        }
    }

    public sealed class IngestSettings
    {
        public required string KafkaBootstrap { get; init; }
        public string TopicRaw { get; init; } = "csv.raw.rows";
        public long MaxFileSizeBytes { get; init; } = 524_288_000;
    }
}

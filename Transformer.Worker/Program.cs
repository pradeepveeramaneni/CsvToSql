using Common.Contracts;
using Common.Observability;
using FluentValidation;
using Transformer.Worker.Validation;

namespace Transformer.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            Logging.ConfigureSerilog(builder);
            builder.Services.AddSingleton<TransformerService>();
            builder.Services.AddSingleton<IValidator<RawRowEvent>, RawRowValidator>();
            builder.Services.AddHostedService<Worker>();


            var host = builder.Build();
            host.Run();
        }
    }

    public sealed class TransformerSettings
    {
        public required string KafkaBootstrap { get; init; }

        public string TopicIn { get; init; } = "csv.raw.rows";
        public string TopicOut { get; init; } = "csv.rows.transformed";
        public string TopicDlq { get; init; } = "csv.rows.dlq";

        public string GroupId { get; init; } = "transformer-worker";



    }

}
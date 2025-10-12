using Common.Contracts;
using Common.Messaging;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Ingest.Api.Services
{
    public sealed class IngestService
    {

        private readonly IngestSettings _cfg;

        public IngestService(IOptions<IngestSettings> cfg)
        {
            _cfg = cfg.Value;
        }

        public async Task<object> StreamCsvToKafka(IFormFile file, CancellationToken ct)
        {
            using var stream=file.OpenReadStream();
            using var reader=new StreamReader(stream, detectEncodingFromByteOrderMarks:true);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                BadDataFound=null,
                DetectDelimiter=true,
                IgnoreBlankLines=true,
                TrimOptions=TrimOptions.Trim,
                MissingFieldFound=null,

            });

            csv.Read();
            csv.ReadHeader();
            var header=csv.HeaderRecord??Array.Empty<string>();

            using var producer = new KafkaProducer<RawRowEvent>(_cfg.KafkaBootstrap, _cfg.TopicRaw, clientId: "ingest-api");

            int lineNo = 0, published = 0;

            while (await csv.ReadAsync())
            {
                ct.ThrowIfCancellationRequested();
                lineNo++;

                // Build string[] columns in declared order for consistency
                var cols = new[]
                {
                csv.GetField("Product"),
                csv.GetField("Company"),
                csv.GetField("Category"),
                csv.GetField("Units Sold"),
                csv.GetField("Revenue"),
                csv.GetField("Cost of Goods Sold"),
                csv.GetField("Profit"),
                csv.GetField("Purchase Date"),
                csv.GetField("Customer Name"),
                csv.GetField("Customer State"),
                csv.GetField("Customer City"),
                csv.GetField("Customer Zip Code"),
                csv.GetField("Order ID"),
            };
                var rowId = $"{file.FileName}#{lineNo}";
                var evt = new RawRowEvent(rowId, file.FileName, lineNo, cols, DateTimeOffset.UtcNow);
                await producer.ProduceAsync(rowId, evt, ct);
                published++;
            }

            return new { file = file.FileName, header, published };

        }
    }
}

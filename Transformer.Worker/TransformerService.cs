using Common.Contracts;
using Common.Messaging;
using FluentValidation;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transformer.Worker
{
    public sealed class TransformerService
    {
        private readonly TransformerSettings _cfg;
        private readonly IValidator<RawRowEvent> _validator;

        public TransformerService(IOptions<TransformerSettings> cfg, IValidator<RawRowEvent> validator)
        {
            _cfg = cfg.Value;
            _validator = validator;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            using var consumer = new KafkaConsumer<RawRowEvent>(_cfg.KafkaBootstrap, _cfg.GroupId);

            using var outProducer= new KafkaProducer<TransformedSaleEvent>(_cfg.KafkaBootstrap, _cfg.TopicOut, "transformer-out");

            using var dlqProducer = new KafkaProducer<RawRowEvent>(_cfg.KafkaBootstrap, _cfg.TopicDlq, "transformer-dlq");

            consumer.Subscribe(_cfg.TopicIn);

            while (!ct.IsCancellationRequested)
            {
                var (cr, raw) = consumer.Consume(ct);
                try
                {
                    if (raw is null) 
                        {
                        consumer.Commit(cr); continue;
                    }

                    var v = await _validator.ValidateAsync(raw, ct);
                    if (!v.IsValid)
                    {
                        await dlqProducer.ProduceAsync(cr.Message.Key!, raw, ct);
                        consumer.Commit(cr);
                        continue;
                    }

                    var c = raw.Columns;
                    var evt = new TransformedSaleEvent
                    (
                        RowId: raw.RowId,
                    OrderId: c[12],
                    Product: c[0],
                    Company: c[1],
                    Category: c[2],
                    UnitsSold: decimal.Parse(c[3]),
                    Revenue: decimal.Parse(c[4]),
                    Cogs: decimal.Parse(c[5]),
                    Profit: decimal.Parse(c[6]),
                    PurchaseDate: DateOnly.Parse(c[7]),
                    CustomerName: c[8],
                    CustomerState: c[9],
                    CustomerCity: c[10],
                    CustomerZip: c[11],
                    TransformedAtUtc: DateTimeOffset.UtcNow
                    );
                    await outProducer.ProduceAsync(evt.RowId, evt, ct);
                    consumer.Commit(cr);
                }
                catch
                {
                    if(raw is not null)
                    {
                        await dlqProducer.ProduceAsync(cr.Message.Key??raw.RowId, raw, ct);
                    }
                    consumer.Commit(cr);
                }
            }
            consumer.close();


        }
    }


    
}


using Common.Contracts;
using Common.Messaging;
using Loader.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loader.Worker.Services
{
    public sealed class LoaderService
    {
        private readonly Loader.Worker.LoaderSettings _cfg;
        private readonly IDbContextFactory<SalesDbContext> _dbFactory;

        public LoaderService(IOptions<Loader.Worker.LoaderSettings> cfg,IDbContextFactory<SalesDbContext> dbFactory)
        {
         _cfg = cfg.Value;
         _dbFactory= dbFactory;
        }


        public async Task RunAsync(CancellationToken ct)
        {
            using var consumer = new KafkaConsumer<TransformedSaleEvent>(_cfg.KafkaBootstrap, _cfg.GroupId);
            consumer.Subscribe(_cfg.TopicIn);

            var policy = Policy
                .Handle<DbUpdateException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(6, attempt => TimeSpan.FromMilliseconds(200 * attempt));

            while (!ct.IsCancellationRequested)
            {
                var (cr, evt) = consumer.Consume(ct);
                if (evt is null)
                {
                    consumer.Commit(cr); continue;
                }
                try
                {
                    await policy.ExecuteAsync(async () =>
                    {
                        await using var db = await _dbFactory.CreateDbContextAsync(ct);

                        bool processed = await db.ProcessedEvents.AsNoTracking().AnyAsync(x => x.RowId == evt.RowId, ct);

                        if (processed) return;

                        db.Sales.Add(new Sale
                        {
                            RowId = evt.RowId,
                            OrderId = evt.OrderId,
                            Product = evt.Product,
                            Company = evt.Company,
                            Category = evt.Category,
                            UnitsSold = evt.UnitsSold,
                            Revenue = evt.Revenue,
                            Cogs = evt.Cogs,
                            Profit = evt.Profit,
                            PurchaseDate = evt.PurchaseDate,
                            CustomerName = evt.CustomerName,
                            CustomerState = evt.CustomerState,
                            CustomerCity = evt.CustomerCity,
                            CustomerZip = evt.CustomerZip
                        }
                            );
                        db.ProcessedEvents.Add(new ProcessedEvent { RowId = evt.RowId });
                        await db.SaveChangesAsync(ct);
                    });

                    consumer.Commit(cr);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"DB write failed for{evt.RowId}: {ex.Message}");
                }
            

            }
            consumer.close();
        }
    }
}

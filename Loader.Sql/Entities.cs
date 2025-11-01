using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loader.Sql
{
    public class Sale
    {
        public long Id { get; set; }
        public string RowId { get; set; } = default!;
        public string OrderId { get; set; } = default!;
        public string Product { get; set; } = default!;
        public string Company { get; set; } = default!;
        public string Category { get; set; } = default!;
        public decimal UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cogs { get; set; }
        public decimal Profit { get; set; }
        public DateOnly PurchaseDate { get; set; }
        public string CustomerName { get; set; } = default!;
        public string CustomerState { get; set; } = default!;
        public string CustomerCity { get; set; } = default!;
        public string CustomerZip { get; set; } = default!;
        public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    }

    public class ProcessedEvent
    {
        public string RowId { get; set; } = default!;
        public DateTimeOffset ProcessedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    }
}

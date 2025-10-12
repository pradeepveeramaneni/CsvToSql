using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Contracts
{

    public record RawRowEvent(
        string RowId,
        string SourceFile,
        int LineNumber,
        string[] Columns,
        DateTimeOffset IngestedATUtc);

    public record TransformedSaleEvent(
        string RowId,
        string OrderId,
        string Product,
        string Company,
        string Category,
        decimal UnitsSold,
        decimal Revenue,
        decimal Cogs,
        decimal Profit,
        DateOnly PurchaseDate,
        string CustomerName,
        string CustomerState,
        string CustomerCity,
        string CustomerZip,
        DateTimeOffset TransformedAtUtc);

        
        
}

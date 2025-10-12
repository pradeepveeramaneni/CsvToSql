using Ingest.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ingest.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsvController : ControllerBase
    {

        private readonly IngestService _svc;
        public CsvController(IngestService svc) => _svc = svc;

        [HttpPost("upload")]
        [RequestSizeLimit(bytes: 524_288_000)]
        public async Task<IActionResult> Upload([FromForm] CsvUploadRequest req, CancellationToken ct)
        {
            if (req.File is null || req.File.Length == 0)
                return BadRequest("Upload a CSV file.");

            var result = await _svc.StreamCsvToKafka(req.File, ct);
            return Ok(result);
        }

        public sealed class CsvUploadRequest
        {
            public IFormFile? File { get; set; }
        }


    }

}

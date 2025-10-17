namespace Transformer.Worker
{
    public class Worker : BackgroundService
    {
        private readonly TransformerService _svc;
        public Worker(TransformerService svc) => _svc = svc;

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => _svc.RunAsync(stoppingToken);

    }
}

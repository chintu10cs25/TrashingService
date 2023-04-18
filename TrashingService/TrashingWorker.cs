using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TrashingService
{
    public class TrashingWorker : BackgroundService
    {
        private readonly ILogger<TrashingWorker> _logger;
        private readonly TrashingProcessor _processor;
        public TrashingWorker(ILogger<TrashingWorker> logger, TrashingProcessor processor)
        {
            _logger = logger;
            _processor = processor;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Trashing service running at: {time}", DateTimeOffset.Now);
                //await Task.Delay(1*60*1000, stoppingToken);
                 await _processor.StartASync();
            }
        }
    }
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
                _processor.Start();
            }
        }
    }
}

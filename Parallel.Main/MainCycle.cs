using System.Threading;
using System.Threading.Tasks;
using MessageObjectRouter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReflectorO;

namespace Parallel.Main
{
    public class MainCycle : BackgroundService
    {
        private readonly ILogger<MainCycle> _logger;
        private readonly IConfiguration _configuration;
        private readonly IElector _elector;

        public MainCycle(ILogger<MainCycle> logger, IConfiguration configuration, IElector elector)
        {
            _logger = logger;
            _configuration = configuration;
            _elector = elector;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
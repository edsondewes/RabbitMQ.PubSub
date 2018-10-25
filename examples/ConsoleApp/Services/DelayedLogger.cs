using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ConsoleApp.Services
{
    public class DelayedLogger
    {
        private readonly ILogger<DelayedLogger> _logger;

        public DelayedLogger(ILogger<DelayedLogger> logger)
        {
            _logger = logger;
        }

        public async Task Log(string message, object[] args, CancellationToken cancellationToken)
        {
            await Task.Delay(500, cancellationToken);
            _logger.LogInformation(message, args);
        }
    }
}

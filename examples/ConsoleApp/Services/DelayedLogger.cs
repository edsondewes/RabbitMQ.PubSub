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

        public async Task Log(string message, params object[] args)
        {
            await Task.Delay(10);
            _logger.LogInformation(message, args);
        }
    }
}

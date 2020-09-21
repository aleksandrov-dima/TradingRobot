using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingRobot.Models;
using TradingRobot.Strategy;

namespace TradingRobot.Services
{
    public class SandboxRobotTradingService : IHostedService, IDisposable
    {
        private readonly ILogger<SandboxRobotTradingService> _logger;
        private Timer? _timer;
        private SandboxRobot _sandboxRobot;

        public SandboxRobotTradingService(ILogger<SandboxRobotTradingService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Robot Trading Service running.");
            
            string token = File.ReadAllText("token.txt").Trim();
            _sandboxRobot = new SandboxRobot(new EasyStrategy(new AutoSettingProvider()), token);
            
            _timer = new Timer(DoWork, null, TimeSpan.Zero, 
                TimeSpan.FromSeconds(3));

            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                await _sandboxRobot.StartTradeAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Message{e.Message}; Innerexception {e.InnerException}; StackTrace {e.StackTrace}");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Robot Trading Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            await _sandboxRobot.RemoveAccountAsync();
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
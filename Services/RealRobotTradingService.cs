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
    public class RealRobotTradingService : IHostedService
    {
        private readonly ILogger<RealRobotTradingService> _logger;
        private Timer _timer;
        private RealRobot _realRobot;
        private int _counterRun = 0;

        public RealRobotTradingService(ILogger<RealRobotTradingService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Real Robot Trading Service running.");
            
            string token = File.ReadAllText("realtoken.txt").Trim();
            _realRobot = new RealRobot(new EasyStrategy(new AutoSettingProvider()), token);
            
            _timer = new Timer(DoWork, null, TimeSpan.Zero, 
                TimeSpan.FromSeconds(3));

            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                Console.WriteLine($"Real Robot run {_counterRun++}");
                await _realRobot.StartTradeAsync();
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
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Network;
using TradingRobot.Models;
using TradingRobot.Services;

namespace TradingRobot.Strategy
{
    public abstract class ITradingStrategy
    {
        private readonly ISettingProvider _settingProvider;
        protected IContext? _context;

        private string? _accountId;

        public ISettingProvider SettingProvider => _settingProvider;

        protected ITradingStrategy(ISettingProvider settingProvider)
        {
            _settingProvider = settingProvider;
        }

        public string AccountId
        {
            get => _accountId!;
            set => _accountId = value;
        }

        public IContext Context
        {
            get => _context!;
            set => _context = value;
        }

        /// <summary>
        /// Выполнить операцию BUY/SELL
        /// </summary>
        /// <param name="tradePosition"></param>
        /// <returns></returns>
        public abstract Task PerformBuyOrSell(TradePosition tradePosition);
    }
}
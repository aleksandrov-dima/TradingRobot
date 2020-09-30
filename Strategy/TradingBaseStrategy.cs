using System;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Network;
using TradingRobot.Models;
using TradingRobot.Services;

namespace TradingRobot.Strategy
{
    public abstract class TradingBaseStrategy: ITrading
    {
        private readonly ISettingProvider _settingProvider;

        public ISettingProvider SettingProvider => _settingProvider;
        public string AccountId { get; set; }
        public IContext Context { get; set; }

        protected Action<TradeOperationInfo> ActionOperation;
        public event Action<TradeOperationInfo> OnActionOperation
        {
            add => ActionOperation += value;
            remove => ActionOperation -= value;
        }
        
        protected TradingBaseStrategy(ISettingProvider settingProvider)
        {
            _settingProvider = settingProvider;
        }

        public abstract Task PerformBuyOrSell(TradePosition tradePosition);
    }
}
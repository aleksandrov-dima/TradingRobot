using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TradingRobot.Models;
using TradingRobot.Services;

namespace TradingRobot.Strategy
{
    /// <summary>
    /// Самая простая стратегия торговли.
    /// Основана на достижении пороговых точек покупки и продажи
    /// </summary>
    public class EasyBaseStrategy: TradingBaseStrategy
    {
        public EasyBaseStrategy(ISettingProvider settingProvider) : base(settingProvider)
        {
        }

        /// <summary>
        /// Выполнить операцию BUY/SELL
        /// </summary>
        /// <param name="tradePosition"></param>
        /// <returns></returns>
        public override async Task PerformBuyOrSell(TradePosition tradePosition)
        {
            var options = tradePosition.Options as EasyTradeOptions;

            tradePosition.IsStateChanged = false;
            MarketOrder marketOrder = null;
            
            if (tradePosition.PrevOperationType == OperationType.Buy)
            {
                //текущая цена превысила ProfitThreshold (цена подскочила, продадим)
                //или упала на StopLossThreshold (продаем пока ещё больше не упало)
                if (options != null && (tradePosition.LastPrice >= options.ProfitThreshold 
                                        || tradePosition.LastPrice <= options.StopLossThreshold))
                {
                    marketOrder = new MarketOrder(tradePosition.PortfolioPosition.Figi,
                        tradePosition.PortfolioPosition.Lots,
                        OperationType.Sell,
                        AccountId);

                    tradePosition.IsStateChanged = true;
                }
            }
            else if(tradePosition.PrevOperationType == OperationType.Sell)
            {
                //хорошая цена , надо покупать (докупать)
                if (options != null && tradePosition.LastPrice <= options.DipThreshold 
                                        /*|| tradePosition.LastPrice >= options.UpwardTrendThreshold*/)
                {
                    //TODO: здесь надо вычислять сколько лотов можем купить

                    marketOrder = new MarketOrder(tradePosition.PortfolioPosition.Figi,
                        1,
                        OperationType.Buy,
                        AccountId);
                    
                    tradePosition.IsStateChanged = true;
                }
            }

            if (tradePosition.IsStateChanged && marketOrder != null)
            {
                var placedMarketOrder = await Context.PlaceMarketOrderAsync(marketOrder);
                
                TradeOperationInfo tradeOperationInfo = new TradeOperationInfo();
                tradeOperationInfo.MarketOrder = marketOrder;
                tradeOperationInfo.PlacedMarkedOrder = placedMarketOrder;

                ActionOperation?.BeginInvoke(tradeOperationInfo, null, null);
            }
        }
    }
}
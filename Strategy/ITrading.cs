using System.Threading.Tasks;
using TradingRobot.Models;

namespace TradingRobot.Strategy
{
    public interface ITrading
    {
        /// <summary>
        /// Выполнить операцию BUY/SELL
        /// </summary>
        /// <param name="tradePosition"></param>
        /// <returns></returns>
        Task PerformBuyOrSell(TradePosition tradePosition);
    }
}
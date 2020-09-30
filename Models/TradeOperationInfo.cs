using Tinkoff.Trading.OpenApi.Models;

namespace TradingRobot.Models
{
    /// <summary>
    /// Информация об операции BUY/SELL
    /// </summary>
    public class TradeOperationInfo
    {
        /// <summary>
        /// Заявка на операцию
        /// </summary>
        public MarketOrder MarketOrder { get; set; }
        
        /// <summary>
        /// Результат операции
        /// </summary>
        public PlacedMarketOrder PlacedMarkedOrder { get; set; }
    }
}
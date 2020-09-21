using System.Threading.Tasks;
using TradingRobot.Models;

namespace TradingRobot.Services
{
    public interface ISettingProvider
    {
        public string[] TradeFigis { get; }
        Task SetOptionsTradingPosition(TradePosition tradePosition);
    }
}
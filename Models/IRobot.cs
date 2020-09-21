using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;

namespace TradingRobot.Models
{
    public abstract class IRobot
    {
        public abstract Task StartTradeAsync();
        public abstract Task GetBalanceAsync();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradingRobot.Models;

namespace TradingRobot.Services
{
    public class AutoSettingProvider : ISettingProvider
    {
        /// <summary>
        /// Разница в цене % от которой зависят пороговые точки
        /// </summary>
        private decimal _diffPercent = 1;

        /// <summary>
        /// Список активов FIGI для торгов
        /// </summary>
        private string[] _tradeFigis;

        public AutoSettingProvider()
        {
            _tradeFigis = new[]{"BBG004730ZJ9"};
        }
        
        public Task SetOptionsTradingPosition(TradePosition tradePosition)
        {
            EasyTradeOptions easyTradeOptions = new EasyTradeOptions();
            
            //порог изменения цены, который влияет на пересмотр настроек
            decimal diffThreshold = _diffPercent * (tradePosition.AvgPrice / 100);

            easyTradeOptions.DipThreshold = tradePosition.LastPrice - diffThreshold * 1.5m;
            easyTradeOptions.StopLossThreshold = tradePosition.LastPrice - diffThreshold;
            easyTradeOptions.UpwardTrendThreshold = tradePosition.LastPrice + diffThreshold;
            easyTradeOptions.ProfitThreshold = tradePosition.LastPrice + diffThreshold;

            tradePosition.Options = easyTradeOptions;

            return Task.CompletedTask;
        }
        
        public string[] TradeFigis
        {
            get => _tradeFigis;
            set => _tradeFigis = value;
        }
    }
}
using System;

namespace TradingRobot.Models
{
    public class EasyTradeOptions : ITradeOptions
    {
        private Decimal _dipThreshold;
        private Decimal _upwardTrendThreshold;
        private Decimal _stopLossThreshold;
        private Decimal _profitThreshold;
        
        /// <summary>
        /// Бот выполняет операцию покупки в том случае, если цена уменьшилась на значение, большее чем DipThreshold
        /// Бот будет пытаться купить актив по заниженной цене, ожидая роста цены и возможности выгодной продажи актива.
        /// </summary>
        public decimal DipThreshold
        {
            get => _dipThreshold;
            set => _dipThreshold = value;
        }

        /// <summary>
        /// Бот покупает актив в том случае, если цена выросла на значение, превышающее UpwardTrendThreshold
        /// Его цель заключается в том, чтобы выявить восходящий тренд и не пропустить возможность покупки до ещё большего роста цены.
        /// </summary>
        public decimal UpwardTrendThreshold
        {
            get => _upwardTrendThreshold;
            set => _upwardTrendThreshold = value;
        }

        /// <summary>
        /// Бот продаёт актив, если цена стала выше цены, вычисленной на основе значения ProfitThreshold
        /// Мы продаём актив по цене, которая выше той, что была в момент его покупки.
        /// </summary>
        public decimal ProfitThreshold
        {
            get => _profitThreshold;
            set => _profitThreshold = value;
        }

        /// <summary>
        /// Это пороговое значение используется для закрытия позиции с убытком.
        /// Цель этой операции — предотвращение более сильных потерь.
        /// </summary>
        public decimal StopLossThreshold
        {
            get => _stopLossThreshold;
            set => _stopLossThreshold = value;
        }
    }
}
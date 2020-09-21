using System;
using Tinkoff.Trading.OpenApi.Models;

namespace TradingRobot.Models
{
    public class TradePosition
    {
        private Portfolio.Position? _portfolioPosition;
        private readonly string _figi;


        private OperationType _nextOperationType;
        private OperationType _prevOperationType;
        private bool _isStateChanged;


        private Decimal _avgPrice;
        private Decimal _profitSum;
        private Decimal _lastPrice;

        public TradePosition(string figi)
        {
            _figi = figi;
            _isStateChanged = true;
        }

        public Portfolio.Position? PortfolioPosition
        {
            get => _portfolioPosition;
            set
            {
                _portfolioPosition = value;
                if (_portfolioPosition != null)
                {
                    _avgPrice = _portfolioPosition.AveragePositionPrice.Value;
                    _profitSum = _portfolioPosition.ExpectedYield.Value;
                }
            }
        }

        public string Figi => _figi;
        
        public ITradeOptions? Options { get; set; }

        /// <summary>
        /// Операция, которая будет выполняться следующей (BUY/SELL) 
        /// </summary>
        public OperationType NextOperationType
        {
            get => _nextOperationType;
            set => _nextOperationType = value;
        }

        /// <summary>
        /// Предыдущая выполненная операция (BUY/SELL) 
        /// </summary>
        public OperationType PrevOperationType
        {
            get => _prevOperationType;
            set => _prevOperationType = value;
        }

        /// <summary>
        /// Средняя цена актива
        /// учитываются все операции покупки и продажи
        /// </summary>
        public decimal AvgPrice
        {
            get => _avgPrice;
            set => _avgPrice = value;
        }

        /// <summary>
        /// Текущая рыночная цена
        /// </summary>
        public decimal LastPrice
        {
            get => _lastPrice;
            set => _lastPrice = value;
        }

        /// <summary>
        /// Общая Прибыль/убыль по позиции
        /// </summary>
        public decimal ProfitSum
        {
            get => _profitSum;
            set => _profitSum = value;
        }

        /// <summary>
        /// Состояние актива было изменено
        /// </summary>
        public bool IsStateChanged 
        { 
            get => _isStateChanged;
            set => _isStateChanged = value;
        }
    }
}
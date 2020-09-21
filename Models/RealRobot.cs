using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TradingRobot.Strategy;

namespace TradingRobot.Models
{
    /// <summary>
    /// Торговый робот для реального счета
    /// </summary>
    public class RealRobot : IRobot
    {
        private readonly Context _context;
        private ITradingStrategy _tradingStrategy;
        private string _accountId = "2033152246";
        private IList<TradePosition> _tradePositions;
        public RealRobot(ITradingStrategy tradingStrategy, string token)
        {
            var connection = ConnectionFactory.GetConnection(token);
            _context = connection.Context;
            _tradingStrategy = tradingStrategy;
            _tradingStrategy.Context = _context;
            _tradePositions = new List<TradePosition>();
        }

        public override async Task StartTradeAsync()
        {
            //инициализируем список активов, с которыми будет торговать бот
            await InitialTradePositions();

            //Вычисление текущего состояния активов
            await CalcCurrentStateOnPosition();

            //пересчитываем пороговые точки для совершения операций BUY/SELL
            await CalcPositionOptions();
            
            //выполняем покупку или продажу
            await PerformBuyOrSell();
        }
        
        /// <summary>
        /// Инициализация списка инструментов для торговли
        /// </summary>
        /// <returns></returns>
        private async Task InitialTradePositions()
        {
            //получаем текущий портфель на счете 
            var portfolio = await _context.PortfolioAsync(_accountId);
            
            foreach (var tradeFigi in _tradingStrategy.SettingProvider.TradeFigis)
            {
                TradePosition currentTradePosition = _tradePositions.FirstOrDefault(p => p.Figi == tradeFigi);
                if (currentTradePosition == null)
                {
                    currentTradePosition = new TradePosition(tradeFigi);
                    _tradePositions.Add(currentTradePosition);
                }
                //обновляем портфелльную позицию
                currentTradePosition.PortfolioPosition = portfolio.Positions.FirstOrDefault(x => x.Figi == tradeFigi);
            }
        }

        /// <summary>
        /// Вычисление текущего состояния активов
        /// </summary>
        /// <returns></returns>
        private async Task CalcCurrentStateOnPosition()
        {
            //TODO: переделать получение списка операций за интервал вместо конкретных дат
            DateTime fromDate = DateTime.Parse("2020-08-01T00:00:00").ToUniversalTime();
            DateTime toDate = DateTime.UtcNow;

            foreach (var tradePosition in _tradePositions)
            {
                if (tradePosition.IsStateChanged)
                {
                    //список всех оперций по позиции, чтобы узнать последнюю операцию
                    var operations =
                        await _context.OperationsAsync(fromDate, toDate, tradePosition.PortfolioPosition.Figi, _accountId);

                    //узнаем тип последней операции (Buy или Sell)
                    var lastOperationList = operations.OrderByDescending(x => x.Date).ToList();
                    var lastOperation = lastOperationList.FirstOrDefault();
                    tradePosition.PrevOperationType = lastOperation != null && lastOperation.OperationType == ExtendedOperationType.Buy
                        ? OperationType.Buy
                        : OperationType.Sell;

                    await CalcLastPrice(tradePosition);
                }
                else
                {
                    await CalcLastPrice(tradePosition);
                }
            }
        }

        /// <summary>
        /// Получение последней цены
        /// </summary>
        /// <param name="tradePosition"></param>
        /// <returns></returns>
        private async Task CalcLastPrice(TradePosition tradePosition)
        {
            //Получаем текущую цену - это цена LastPrice, которая передается вместе со стаканом заявок
            var orderbook = await _context.MarketOrderbookAsync(tradePosition.PortfolioPosition.Figi, 1);
            tradePosition.LastPrice = orderbook.LastPrice;

            if (tradePosition.PortfolioPosition == null)
            {
                tradePosition.AvgPrice = tradePosition.LastPrice;
                tradePosition.ProfitSum = Decimal.Zero;
                Console.WriteLine($"Figi: {tradePosition.Figi} avgprice: {tradePosition.AvgPrice} lastprice: {tradePosition.LastPrice}");
            }
            else
            {
                Console.WriteLine($"Ticker: {tradePosition.PortfolioPosition.Ticker} balance:{tradePosition.PortfolioPosition.Balance} avgprice: {tradePosition.AvgPrice} lastprice: {tradePosition.LastPrice} profit: {tradePosition.ProfitSum} prevoperation: {tradePosition.PrevOperationType}");
            }
        }
        
        /// <summary>
        /// Пересчет пороговых значений покупки/продажи для каждой позиции
        /// </summary>
        /// <returns></returns>
        private async Task CalcPositionOptions()
        {
            foreach (var position in _tradePositions)
            {
                await _tradingStrategy.SettingProvider.SetOptionsTradingPosition(position);
            }
        }
        
        /// <summary>
        /// Выполнить операцию покупки или продажи
        /// </summary>
        /// <returns></returns>
        private async Task PerformBuyOrSell()
        {
            foreach (var tradePosition in _tradePositions)
            {
                await _tradingStrategy.PerformBuyOrSell(tradePosition);
            }
        }

        public override async Task GetBalanceAsync()
        {
            Console.WriteLine("Balance");
        }
    }
}
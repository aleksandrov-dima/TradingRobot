using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using TradingRobot.Services;
using TradingRobot.Strategy;

namespace TradingRobot.Models
{
    /// <summary>
    /// Торговый робот для песочницы
    /// </summary>
    public class SandboxRobot : IRobot
    {
        private static readonly Random _random = new Random();
        private readonly SandboxContext _context;
        private ITradingStrategy _tradingStrategy;
        private string? _accountId;
        private IList<TradePosition> _tradePositions;


        public SandboxRobot(ITradingStrategy tradingStrategy, string token)
        {
            var connection = ConnectionFactory.GetSandboxConnection(token);
            _context = connection.Context;
            _tradingStrategy = tradingStrategy;
            _tradingStrategy.Context = _context;
            _tradePositions = new List<TradePosition>();
        }
        
        public override async Task StartTradeAsync()
        {
            //регистрируем аккаунт в песочнице если это еще не сделано
            if (string.IsNullOrEmpty(_accountId))
                await RegisterSandboxAccount();

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
        /// Регистрация нового аккаунта в песочнице
        /// </summary>
        /// <returns></returns>
        private async Task RegisterSandboxAccount()
        {
            var sandboxAccount = await _context.RegisterAsync(BrokerAccountType.Tinkoff);
            _accountId = sandboxAccount.BrokerAccountId;
            Console.WriteLine($"AccauntId:{_accountId}");
        }

        /// <summary>
        /// Инициализация списка активов портфеля
        /// </summary>
        /// <returns></returns>
        private async Task InitialTradePositions()
        {
            //получаем текущий портфель на счете 
            var portfolio = await _context.PortfolioAsync(_accountId);
            if (portfolio.Positions.Count == 0)
            {
                await InitialBalance();

                await InitialTradePositions();
            }
            else
            {
                foreach (var tradeFigi in _tradingStrategy.SettingProvider.TradeFigis)
                {
                    var tradePosition = new TradePosition(tradeFigi);
                    tradePosition.PortfolioPosition = portfolio.Positions.FirstOrDefault(x => x.Figi == tradeFigi);
                
                    _tradePositions.Add(tradePosition);
                }   
            }
        }
        

        /// <summary>
        /// Начальная инициализация портфеля
        /// </summary>
        /// <returns></returns>
        private async Task InitialBalance()
        {
            //инициализируем начальный баланс случайными значениями
            foreach (var currency in new[] {Currency.Rub, Currency.Usd, Currency.Eur})
                await _context.SetCurrencyBalanceAsync(currency, _random.Next(2, 10) * 100_000, _accountId);

            //покупаем акции Сбербанка 1 лот = 10 акций
            await _context.PlaceMarketOrderAsync(new MarketOrder("BBG0047315Y7", 1, OperationType.Buy, _accountId));
        }

        /// <summary>
        /// Вычисление текущего состояния активов
        /// </summary>
        /// <returns></returns>
        private async Task CalcCurrentStateOnPosition()
        {
            DateTime fromDate = DateTime.Parse("2020-08-01T00:00:00").ToUniversalTime();
            DateTime toDate = DateTime.UtcNow;

            foreach (var tradePosition in _tradePositions)
            {
                if (tradePosition.IsStateChanged)
                {
                    //список всех оперций по позиции
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

        public async ValueTask RemoveAccountAsync()
        {
            if (_accountId != null)
            {
                await _context.RemoveAsync(_accountId);
                Console.WriteLine($"Account {_accountId} is deleted");
            }
        }
    }
}
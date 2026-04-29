using System;
using System.Collections.Generic;

namespace Convert.Money
{
    public class CurrencyService
    {
        private readonly Dictionary<string, Currency> _currencies;

        public CurrencyService()
        {
            _currencies = new Dictionary<string, Currency>(StringComparer.OrdinalIgnoreCase)
            {
                { "KZT", new Currency("KZT", 1m) },
                { "USD", new Currency("USD", 470m) },
                { "EUR", new Currency("EUR", 510m) }
            };
        }

        public IReadOnlyCollection<Currency> GetCurrencies()
        {
            return _currencies.Values;
        }

        public decimal Convert(decimal amount, string fromCode, string toCode)
        {
            if (!_currencies.ContainsKey(fromCode) || !_currencies.ContainsKey(toCode))
            {
                throw new ArgumentException("Выбрана неизвестная валюта.");
            }

            decimal amountInKzt = amount * _currencies[fromCode].ToKztRate;
            return amountInKzt / _currencies[toCode].ToKztRate;
        }

        public void RefreshRates()
        {
            _currencies["USD"].ToKztRate = 472m;
            _currencies["EUR"].ToKztRate = 512m;
        }

        public decimal GetRateToKzt(string code)
        {
            if (!_currencies.ContainsKey(code))
            {
                throw new ArgumentException("Выбрана неизвестная валюта.");
            }

            return _currencies[code].ToKztRate;
        }
    }
}

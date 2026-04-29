namespace Convert.Money
{
    public class Currency
    {
        public Currency(string code, decimal toKztRate)
        {
            Code = code;
            ToKztRate = toKztRate;
        }

        public string Code { get; set; }

        public decimal ToKztRate { get; set; }
    }
}

namespace _02_Inheritance
{
    public class Customer : Person
    {
        public string CustomerNumber { get; }
        public DateTime RegistrationDate { get; }
        public decimal TotalSpent { get; private set; }
        public string CustomerTier { get; protected set; }

        private static int _nextCustomerNumber = 10000;

        public Customer(string firstName, string lastName, DateTime birthDate, string email)
            : base(firstName, lastName, birthDate, email)
        {
            CustomerNumber = $"CUST-{_nextCustomerNumber++}";
            RegistrationDate = DateTime.Now;
            TotalSpent = 0m;
            CustomerTier = "Bronze";
        }

        public override string FullName => $"{base.FullName} ({CustomerNumber})";

        public override string GetPersonalInfo()
        {
            string baseInfo = base.GetPersonalInfo();
            return $"{baseInfo}, Customer Since: {RegistrationDate:yyyy-MM-dd}, Tier: {CustomerTier}";
        }

        public virtual decimal GetDiscount()
        {
            return CustomerTier switch
            {
                "Bronze" => 0.02m, // 2%
                "Silver" => 0.05m, // 5%
                "Gold" => 0.10m,   // 10%
                "Platinum" => 0.15m, // 15%
                _ => 0m
            };
        }

        public virtual void AddPurchase(decimal amount)
        {
            TotalSpent += amount;
            UpdateCustomerTier();
            Console.WriteLine($"Purchase of ${amount:F2} added for {FullName}. Total spent: ${TotalSpent:F2}");
        }

        protected virtual void UpdateCustomerTier()
        {
            string oldTier = CustomerTier;
            CustomerTier = TotalSpent switch
            {
                >= 10000 => "Platinum",
                >= 5000 => "Gold",
                >= 1000 => "Silver",
                _ => "Bronze"
            };

            if (oldTier != CustomerTier)
            {
                Console.WriteLine($"{FullName} upgraded from {oldTier} to {CustomerTier} tier!");
            }
        }
    }
}

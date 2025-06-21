namespace _02_Inheritance
{
    // SECOND LEVEL INHERITENCE
    public class Manager1 : Employee
    {
        public List<Employee> DirectReports { get; }
        public decimal ManagementBonus { get; set; }
        public string ManagementLevel { get; set; } // Senior, Director, VP, etc.

        public Manager1(string firstName, string lastName, DateTime birthDate, string email,
                      string department, decimal baseSalary, string managementLevel = "Manager")
            : base(firstName, lastName, birthDate, email, department, baseSalary, managementLevel)
        {
            DirectReports = new List<Employee>();
            ManagementBonus = 0.10m; // 10% management bonus
            ManagementLevel = managementLevel;
        }

        // Override salary calculation to include management bonus
        public override decimal CalculateMonthlySalary()
        {
            decimal baseMonthlySalary = base.CalculateMonthlySalary();
            decimal managementBonusAmount = baseMonthlySalary * ManagementBonus;
            return baseMonthlySalary + managementBonusAmount;
        }

        // Override yearly bonus based on team performance
        public override decimal CalculateYearlyBonus()
        {
            decimal baseBonus = base.CalculateYearlyBonus();
            decimal teamBonus = DirectReports.Count * 500m; // $500 per direct report
            return baseBonus + teamBonus;
        }
        public override string GetWorkDescription()
        {
            string baseDescription = base.GetWorkDescription();
            return $"{baseDescription}. Manages {DirectReports.Count} employees.";
        }
        // Manager-specific methods
        public void AddDirectReport(Employee employee)
        {
            if (employee == null) throw new ArgumentNullException(nameof(employee));
            if (employee == this) throw new ArgumentException("Manager cannot manage themselves");

            DirectReports.Add(employee);
            Console.WriteLine($"{employee.FullName} now reports to {FullName}");
        }

        public void ConductPerformanceReview(Employee employee, string review)
        {
            if (!DirectReports.Contains(employee))
            {
                Console.WriteLine($"{employee.FullName} is not a direct report of {FullName}");
                return;
            }

            Console.WriteLine($"Performance Review for {employee.FullName} by {FullName}:");
            Console.WriteLine($"Review: {review}");
        }

        public void ApproveBudget(decimal amount)
        {
            Console.WriteLine($"{FullName} approved budget of ${amount:F2} for {Department}");
        }

    }
}

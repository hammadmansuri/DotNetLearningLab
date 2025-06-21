namespace _02_Inheritance
{
    public class Employee : Person
    {
        public int EmployeeId { get; }
        public string Department { get; set; }
        public DateTime HireDate { get; }
        public decimal BaseSalary { get; protected set; }
        public string JobTitle { get; protected set; }

        private static int _nextEmployeeId = 5000;

        // Constructor using base keyword
        public Employee(string firstName, string lastName, DateTime birthDate, string email,
                       string department, decimal baseSalary, string jobTitle)
            : base(firstName, lastName, birthDate, email) // Call parent constructor
        {
            EmployeeId = _nextEmployeeId++;
            Department = department ?? throw new ArgumentNullException(nameof(department));
            HireDate = DateTime.Now;
            BaseSalary = baseSalary;
            JobTitle = jobTitle ?? "Employee";
        }

        // Override virtual property from base class
        public override string FullName => $"{base.FullName} (EMP-{EmployeeId})";

        // Override virtual method from base class
        public override string GetPersonalInfo()
        {
            string baseInfo = base.GetPersonalInfo(); // Call parent implementation
            return $"{baseInfo}, Department: {Department}, Title: {JobTitle}";
        }
        // Virtual methods specific to employees - can be overridden by derived classes
        public virtual decimal CalculateMonthlySalary()
        {
            return BaseSalary / 12;
        }

        public virtual decimal CalculateYearlyBonus()
        {
            return BaseSalary * 0.05m; // 5% default bonus
        }

        public virtual string GetWorkDescription()
        {
            return $"Works in {Department} as {JobTitle}";
        }
        public virtual void PromoteEmployee(string newTitle, decimal salaryIncrease)
        {
            string oldTitle = JobTitle;
            decimal oldSalary = BaseSalary;

            JobTitle = newTitle;
            BaseSalary += salaryIncrease;

            Console.WriteLine($"{FullName} promoted from {oldTitle} to {JobTitle}");
            Console.WriteLine($"Salary increased from ${oldSalary:F2} to ${BaseSalary:F2}");
        }

        // Method to access protected member from base class
        public void SetEmployeeSSN(string ssn)
        {
            SetSSN(ssn); // Can access protected method from base class
        }

        // Override contact info update with employee-specific logic
        public override void UpdateContactInfo(string email, string phone)
        {
            base.UpdateContactInfo(email, phone); // Call parent method
            Console.WriteLine($"HR notified of contact change for employee {EmployeeId}");
        }

        // Calculate years of service
        public int GetYearsOfService()
        {
            return DateTime.Now.Year - HireDate.Year;
        }
    }
}

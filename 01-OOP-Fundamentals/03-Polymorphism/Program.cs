using System;
using System.Collections.Generic;

namespace PolymorphismDemo
{
    // =============================================================================
    // 1. COMPILE-TIME POLYMORPHISM (Method Overloading)
    // =============================================================================

    public class Calculator
    {
        // Method Overloading - Same name, different parameters
        public int Add(int a, int b)
        {
            return a + b;
        }

        public double Add(double a, double b)
        {
            return a + b;
        }

        public int Add(int a, int b, int c)
        {
            return a + b + c;
        }

        public string Add(string a, string b)
        {
            return a + b;
        }
    }

    // =============================================================================
    // 2. OPERATOR OVERLOADING
    // =============================================================================

    public struct Money
    {
        public decimal Amount { get; }

        public Money(decimal amount) => Amount = amount;

        // Operator overloading
        public static Money operator +(Money left, Money right)
        {
            return new Money(left.Amount + right.Amount);
        }

        public static Money operator *(Money money, decimal multiplier)
        {
            return new Money(money.Amount * multiplier);
        }

        public override string ToString() => $"${Amount:F2}";
    }

    // =============================================================================
    // 3. RUNTIME POLYMORPHISM (Virtual Methods & Overriding)
    // =============================================================================

    public abstract class Employee
    {
        public string Name { get; set; }
        public decimal BaseSalary { get; set; }

        protected Employee(string name, decimal baseSalary)
        {
            Name = name;
            BaseSalary = baseSalary;
        }

        // Virtual method - can be overridden
        public virtual decimal CalculateBonus()
        {
            return BaseSalary * 0.10m; // 10% default bonus
        }

        // Virtual method
        public virtual string GetJobDescription()
        {
            return "General employee duties";
        }

        // Abstract method - must be implemented
        public abstract string GetDepartment();

        public void DisplayInfo()
        {
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Department: {GetDepartment()}");
            Console.WriteLine($"Job: {GetJobDescription()}");
            Console.WriteLine($"Salary: ${BaseSalary:F2}");
            Console.WriteLine($"Bonus: ${CalculateBonus():F2}");
            Console.WriteLine();
        }
    }

    public class Developer : Employee
    {
        public Developer(string name, decimal baseSalary)
            : base(name, baseSalary) { }

        // Override virtual method
        public override decimal CalculateBonus()
        {
            return BaseSalary * 0.15m; // 15% bonus for developers
        }

        // Override virtual method
        public override string GetJobDescription()
        {
            return "Writes code and builds software";
        }

        // Implement abstract method
        public override string GetDepartment()
        {
            return "Engineering";
        }
    }

    public class Manager : Employee
    {
        public int TeamSize { get; set; }

        public Manager(string name, decimal baseSalary, int teamSize)
            : base(name, baseSalary)
        {
            TeamSize = teamSize;
        }

        // Override virtual method
        public override decimal CalculateBonus()
        {
            decimal baseBonus = BaseSalary * 0.20m; // 20% base bonus
            decimal teamBonus = TeamSize * 500m;    // $500 per team member
            return baseBonus + teamBonus;
        }

        // Override virtual method
        public override string GetJobDescription()
        {
            return $"Manages {TeamSize} team members";
        }

        // Implement abstract method
        public override string GetDepartment()
        {
            return "Management";
        }
    }

    public class SalesRep : Employee
    {
        public decimal SalesTarget { get; set; }

        public SalesRep(string name, decimal baseSalary, decimal salesTarget)
            : base(name, baseSalary)
        {
            SalesTarget = salesTarget;
        }

        // Override virtual method
        public override decimal CalculateBonus()
        {
            return SalesTarget * 0.05m; // 5% of sales target
        }

        // Override virtual method
        public override string GetJobDescription()
        {
            return $"Sells products, target: ${SalesTarget:F2}";
        }

        // Implement abstract method
        public override string GetDepartment()
        {
            return "Sales";
        }
    }

    // =============================================================================
    // 4. INTERFACE-BASED POLYMORPHISM
    // =============================================================================

    public interface IPayable
    {
        decimal CalculatePay();
        void ProcessPayment();
    }

    public class FullTimeEmployee : Employee, IPayable
    {
        public FullTimeEmployee(string name, decimal baseSalary)
            : base(name, baseSalary) { }

        public override string GetDepartment() => "Full-Time Staff";

        public decimal CalculatePay()
        {
            return BaseSalary + CalculateBonus();
        }

        public void ProcessPayment()
        {
            Console.WriteLine($"Processing monthly payment of ${CalculatePay():F2} for {Name}");
        }
    }

    public class Contractor : IPayable
    {
        public string Name { get; set; }
        public decimal HourlyRate { get; set; }
        public int HoursWorked { get; set; }

        public Contractor(string name, decimal hourlyRate, int hoursWorked)
        {
            Name = name;
            HourlyRate = hourlyRate;
            HoursWorked = hoursWorked;
        }

        public decimal CalculatePay()
        {
            return HourlyRate * HoursWorked;
        }

        public void ProcessPayment()
        {
            Console.WriteLine($"Processing contractor payment of ${CalculatePay():F2} for {Name}");
        }
    }

    // =============================================================================
    // DEMONSTRATION
    // =============================================================================

    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== POLYMORPHISM DEMONSTRATION ===\n");

            // 1. Compile-time Polymorphism (Method Overloading)
            Console.WriteLine("1. METHOD OVERLOADING:");
            var calc = new Calculator();
            Console.WriteLine($"Add(5, 3) = {calc.Add(5, 3)}");
            Console.WriteLine($"Add(5.5, 3.2) = {calc.Add(5.5, 3.2)}");
            Console.WriteLine($"Add(1, 2, 3) = {calc.Add(1, 2, 3)}");
            Console.WriteLine($"Add(\"Hello\", \"World\") = {calc.Add("Hello", "World")}");

            // 2. Operator Overloading
            Console.WriteLine("\n2. OPERATOR OVERLOADING:");
            Money salary = new Money(5000m);
            Money bonus = new Money(1000m);
            Console.WriteLine($"Salary: {salary}");
            Console.WriteLine($"Bonus: {bonus}");
            Console.WriteLine($"Total: {salary + bonus}");
            Console.WriteLine($"Double bonus: {bonus * 2}");

            // 3. Runtime Polymorphism (Virtual Methods)
            Console.WriteLine("\n3. RUNTIME POLYMORPHISM:");
            Employee[] employees = {
                new Developer("Alice", 80000m),
                new Manager("Bob", 90000m, 5),
                new SalesRep("Charlie", 60000m, 100000m)
            };

            // Same method call, different behavior based on actual type
            foreach (Employee emp in employees)
            {
                emp.DisplayInfo(); // Polymorphic call
            }

            // 4. Interface Polymorphism
            Console.WriteLine("4. INTERFACE POLYMORPHISM:");
            IPayable[] payables = {
                new FullTimeEmployee("David", 75000m),
                new Contractor("Eve", 50m, 160) // 160 hours
            };

            foreach (IPayable payable in payables)
            {
                payable.ProcessPayment(); // Polymorphic call
            }

            Console.WriteLine("\n=== KEY TAKEAWAYS ===");
            Console.WriteLine("• Method Overloading: Same name, different parameters (compile-time)");
            Console.WriteLine("• Operator Overloading: Custom behavior for operators");
            Console.WriteLine("• Virtual/Override: Different implementations at runtime");
            Console.WriteLine("• Interfaces: Common contract, different implementations");
            Console.WriteLine("• Abstract: Forces derived classes to implement methods");
        }
    }
}
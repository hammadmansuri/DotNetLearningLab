InheritanceDemo.RunDemo();

// BASE CLASS - Contains common properties and methods for all people
public class Person
{
    // Protected members - accessible to derived classes
    protected DateTime _birthDate;
    protected string _ssn; // Social Security Number

    // Private members - NOT accessible to derived classes
    private static int _nextPersonId = 1000;

    // Public properties
    public int PersonId { get; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }

    // Virtual property - can be overridden
    public virtual string FullName => $"{FirstName} {LastName}";

    // Calculated property using protected field
    public int Age => DateTime.Now.Year - _birthDate.Year -
        (DateTime.Now.DayOfYear < _birthDate.DayOfYear ? 1 : 0);

    // Constructor
    public Person(string firstName, string lastName, DateTime birthDate, string email)
    {
        PersonId = _nextPersonId++;
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        _birthDate = birthDate;
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }

    // Virtual methods - can be overridden by derived classes
    public virtual string GetContactInfo()
    {
        return $"Email: {Email}, Phone: {PhoneNumber ?? "Not provided"}";
    }

    public virtual string GetPersonalInfo()
    {
        return $"ID: {PersonId}, Name: {FullName}, Age: {Age}";
    }

    public virtual void UpdateContactInfo(string email, string phone)
    {
        Email = email ?? Email;
        PhoneNumber = phone ?? PhoneNumber;
        Console.WriteLine($"Contact info updated for {FullName}");
    }

    // Regular method - cannot be overridden unless marked virtual
    public void DisplayBasicInfo()
    {
        Console.WriteLine($"{PersonId}: {FullName} ({Age} years old)");
    }

    // Protected method - only derived classes can access
    protected string GetInternalInfo()
    {
        return $"SSN: {_ssn ?? "Not set"}, Birth Date: {_birthDate:yyyy-MM-dd}";
    }

    // Protected method for setting sensitive data
    protected void SetSSN(string ssn)
    {
        _ssn = ssn;
    }

    public override string ToString()
    {
        return GetPersonalInfo();
    }
}

// FIRST LEVEL INHERITANCE
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

// SECOND LEVEL INHERITANCE - Specific Employee Types
public class Manager : Employee
{
    public List<Employee> DirectReports { get; }
    public decimal ManagementBonus { get; set; }
    public string ManagementLevel { get; set; } // Senior, Director, VP, etc.

    public Manager(string firstName, string lastName, DateTime birthDate, string email,
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

public class Developer : Employee
{
    public List<string> ProgrammingLanguages { get; }
    public string Specialization { get; set; }
    public int ProjectsCompleted { get; private set; }

    public Developer(string firstName, string lastName, DateTime birthDate, string email,
                    decimal baseSalary, string specialization = "Full Stack")
        : base(firstName, lastName, birthDate, email, "Engineering", baseSalary, "Software Developer")
    {
        ProgrammingLanguages = new List<string>();
        Specialization = specialization;
        ProjectsCompleted = 0;
    }

    // Override work description
    public override string GetWorkDescription()
    {
        string languages = ProgrammingLanguages.Count > 0
            ? string.Join(", ", ProgrammingLanguages)
            : "Various languages";
        return $"Develops software using {languages}. Specialization: {Specialization}";
    }

    // Override yearly bonus based on projects completed
    public override decimal CalculateYearlyBonus()
    {
        decimal baseBonus = base.CalculateYearlyBonus();
        decimal projectBonus = ProjectsCompleted * 1000m; // $1000 per completed project
        return baseBonus + projectBonus;
    }

    public void AddProgrammingLanguage(string language)
    {
        if (!ProgrammingLanguages.Contains(language))
        {
            ProgrammingLanguages.Add(language);
            Console.WriteLine($"{FullName} added {language} to their skill set");
        }
    }

    public void CompleteProject(string projectName)
    {
        ProjectsCompleted++;
        Console.WriteLine($"{FullName} completed project: {projectName}. Total projects: {ProjectsCompleted}");
    }

    public void CodeReview(string feedback)
    {
        Console.WriteLine($"Code review feedback for {FullName}: {feedback}");
    }
}

public class Designer : Employee
{
    public List<string> DesignTools { get; }
    public string DesignSpecialty { get; set; }
    public int DesignsCreated { get; private set; }

    public Designer(string firstName, string lastName, DateTime birthDate, string email,
                   decimal baseSalary, string designSpecialty = "UI/UX")
        : base(firstName, lastName, birthDate, email, "Design", baseSalary, "Designer")
    {
        DesignTools = new List<string> { "Figma", "Adobe Creative Suite" };
        DesignSpecialty = designSpecialty;
        DesignsCreated = 0;
    }

    public override string GetWorkDescription()
    {
        string tools = string.Join(", ", DesignTools);
        return $"Creates {DesignSpecialty} designs using {tools}";
    }

    public override decimal CalculateYearlyBonus()
    {
        decimal baseBonus = base.CalculateYearlyBonus();
        decimal designBonus = DesignsCreated * 200m; // $200 per design
        return baseBonus + designBonus;
    }

    public void CreateDesign(string designName)
    {
        DesignsCreated++;
        Console.WriteLine($"{FullName} created design: {designName}. Total designs: {DesignsCreated}");
    }

    public void AddDesignTool(string tool)
    {
        if (!DesignTools.Contains(tool))
        {
            DesignTools.Add(tool);
            Console.WriteLine($"{FullName} learned new design tool: {tool}");
        }
    }
}

// THIRD LEVEL INHERITANCE
public class SeniorDeveloper : Developer
{
    public List<string> Certifications { get; }
    public bool IsArchitect { get; set; }
    public int MenteeCount { get; private set; }

    public SeniorDeveloper(string firstName, string lastName, DateTime birthDate, string email,
                          decimal baseSalary, string specialization = "System Architecture")
        : base(firstName, lastName, birthDate, email, baseSalary, specialization)
    {
        JobTitle = "Senior Software Developer";
        Certifications = new List<string>();
        IsArchitect = false;
        MenteeCount = 0;
    }

    // Override to include architect responsibilities
    public override string GetWorkDescription()
    {
        string baseDescription = base.GetWorkDescription();
        string architectInfo = IsArchitect ? " Also serves as System Architect." : "";
        return $"{baseDescription}{architectInfo} Mentors {MenteeCount} junior developers.";
    }

    // Higher bonus for senior developers
    public override decimal CalculateYearlyBonus()
    {
        decimal baseBonus = base.CalculateYearlyBonus();
        decimal seniorBonus = IsArchitect ? 5000m : 2000m;
        decimal mentorBonus = MenteeCount * 500m;
        return baseBonus + seniorBonus + mentorBonus;
    }

    public void MentorDeveloper(Developer juniorDev)
    {
        MenteeCount++;
        Console.WriteLine($"{FullName} is now mentoring {juniorDev.FullName}");
    }

    public void AddCertification(string certification)
    {
        Certifications.Add(certification);
        Console.WriteLine($"{FullName} earned certification: {certification}");
    }

    public void DesignArchitecture(string systemName)
    {
        if (!IsArchitect)
        {
            Console.WriteLine($"{FullName} is not designated as an architect");
            return;
        }
        Console.WriteLine($"{FullName} designed architecture for {systemName}");
    }
}

// SEALED CLASS - Cannot be inherited further
public sealed class CEO : Manager
{
    public decimal StockOptions { get; set; }
    public string CompanyVision { get; set; }

    public CEO(string firstName, string lastName, DateTime birthDate, string email, decimal baseSalary)
        : base(firstName, lastName, birthDate, email, "Executive", baseSalary, "Chief Executive Officer")
    {
        ManagementLevel = "CEO";
        StockOptions = 100000m; // Default stock options
    }

    public override decimal CalculateMonthlySalary()
    {
        // CEO gets significant bonus
        decimal baseSalary = base.CalculateMonthlySalary();
        return baseSalary * 2; // Double the management salary
    }

    public override decimal CalculateYearlyBonus()
    {
        // CEO bonus based on company performance (simplified)
        return BaseSalary * 0.50m; // 50% of base salary
    }

    public void SetCompanyDirection(string vision)
    {
        CompanyVision = vision;
        Console.WriteLine($"CEO {FullName} set new company vision: {vision}");
    }

    public void ApproveExecutiveDecision(string decision)
    {
        Console.WriteLine($"CEO {FullName} approved: {decision}");
    }
}

// Premium Customer - Second level inheritance from Customer
public class PremiumCustomer : Customer
{
    public string PersonalShopper { get; set; }
    public bool HasConciergeService { get; set; }

    public PremiumCustomer(string firstName, string lastName, DateTime birthDate, string email)
        : base(firstName, lastName, birthDate, email)
    {
        CustomerTier = "Gold"; // Start at Gold level
        HasConciergeService = true;
    }

    // Override discount to always give premium treatment
    public override decimal GetDiscount()
    {
        decimal baseDiscount = base.GetDiscount();
        return baseDiscount + 0.05m; // Additional 5% for premium customers
    }

    // Override purchase method to include premium services
    public override void AddPurchase(decimal amount)
    {
        base.AddPurchase(amount);
        if (HasConciergeService && amount > 1000)
        {
            Console.WriteLine($"Concierge service activated for large purchase by {FullName}");
        }
    }

    public void RequestPersonalShopper()
    {
        PersonalShopper = "Available";
        Console.WriteLine($"Personal shopper assigned to {FullName}");
    }
}

// This would cause a compiler error because CEO is sealed:
// public class SuperCEO : CEO { } // ❌ Error!

// DEMONSTRATION CLASS
public class InheritanceDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("=== EMPLOYEE/PERSON INHERITANCE DEMONSTRATION ===\n");

        // Create objects of different types
        var people = new List<Person>
            {
                new Person("John", "Doe", new DateTime(1985, 5, 15), "john.doe@email.com"),
                new Employee("Jane", "Smith", new DateTime(1990, 3, 22), "jane.smith@company.com",
                           "HR", 65000m, "HR Specialist"),
                new Manager("Bob", "Johnson", new DateTime(1975, 8, 10), "bob.j@company.com",
                          "Engineering", 95000m, "Engineering Manager"),
                new Developer("Alice", "Wilson", new DateTime(1992, 11, 5), "alice.w@company.com",
                            80000m, "Backend"),
                new Designer("Carol", "Brown", new DateTime(1988, 1, 30), "carol.b@company.com",
                           70000m, "UI/UX"),
                new SeniorDeveloper("David", "Lee", new DateTime(1980, 7, 18), "david.l@company.com",
                                  120000m, "System Architecture"),
                new CEO("Michael", "Chen", new DateTime(1965, 12, 3), "michael.c@company.com", 250000m),
                new Customer("Sarah", "Davis", new DateTime(1995, 4, 12), "sarah.d@email.com"),
                new PremiumCustomer("Robert", "Miller", new DateTime(1970, 9, 25), "robert.m@email.com")
            };

        // Demonstrate polymorphism
        Console.WriteLine("1. POLYMORPHIC BEHAVIOR - GetPersonalInfo():");
        foreach (Person person in people)
        {
            Console.WriteLine($"- {person.GetPersonalInfo()}");
        }

        Console.WriteLine("\n2. POLYMORPHIC BEHAVIOR - Contact Info:");
        foreach (Person person in people)
        {
            Console.WriteLine($"- {person.FullName}: {person.GetContactInfo()}");
        }

        Console.WriteLine("\n3. TYPE CHECKING AND CASTING:");
        foreach (Person person in people)
        {
            // Using 'is' operator with pattern matching
            switch (person)
            {
                case CEO ceo:
                    Console.WriteLine($"👑 {ceo.FullName} - CEO with ${ceo.CalculateMonthlySalary():F2} monthly salary");
                    break;
                case SeniorDeveloper seniorDev:
                    Console.WriteLine($"💻 {seniorDev.FullName} - Senior Dev, Architect: {seniorDev.IsArchitect}");
                    break;
                case Manager manager:
                    Console.WriteLine($"👔 {manager.FullName} - Manages {manager.DirectReports.Count} people");
                    break;
                case Developer dev:
                    Console.WriteLine($"⚡ {dev.FullName} - {dev.GetWorkDescription()}");
                    break;
                case Designer designer:
                    Console.WriteLine($"🎨 {designer.FullName} - {designer.GetWorkDescription()}");
                    break;
                case Employee emp:
                    Console.WriteLine($"📋 {emp.FullName} - {emp.GetWorkDescription()}");
                    break;
                case PremiumCustomer premiumCust:
                    Console.WriteLine($"💎 {premiumCust.FullName} - Premium customer, {premiumCust.GetDiscount():P} discount");
                    break;
                case Customer cust:
                    Console.WriteLine($"🛍️ {cust.FullName} - {cust.CustomerTier} tier customer");
                    break;
                default:
                    Console.WriteLine($"👤 {person.FullName} - Regular person");
                    break;
            }
        }

        Console.WriteLine("\n4. INHERITANCE HIERARCHY DEMONSTRATION:");

        // Get the senior developer and demonstrate inheritance chain
        var seniorDev1 = people.OfType<SeniorDeveloper>().First();
        seniorDev1.AddProgrammingLanguage("C#");
        seniorDev1.AddProgrammingLanguage("Python");
        seniorDev1.AddCertification("AWS Solutions Architect");
        seniorDev1.IsArchitect = true;
        seniorDev1.CompleteProject("Microservices Migration");

        var juniorDev = people.OfType<Developer>().First();
        seniorDev1.MentorDeveloper(juniorDev);

        Console.WriteLine($"\nSenior Developer Details: {seniorDev1.GetWorkDescription()}");
        Console.WriteLine($"Monthly Salary: ${seniorDev1.CalculateMonthlySalary():F2}");
        Console.WriteLine($"Yearly Bonus: ${seniorDev1.CalculateYearlyBonus():F2}");

        Console.WriteLine("\n5. MANAGER HIERARCHY:");
        var manager1 = people.OfType<Manager>().First();
        var developer = people.OfType<Developer>().First();
        var designer1 = people.OfType<Designer>().First();

        manager1.AddDirectReport(developer);
        manager1.AddDirectReport(designer1);
        manager1.ConductPerformanceReview(developer, "Excellent work on recent projects");

        Console.WriteLine($"\nManager: {manager1.GetWorkDescription()}");
        Console.WriteLine($"Monthly Salary: ${manager1.CalculateMonthlySalary():F2}");

        Console.WriteLine("\n6. CUSTOMER HIERARCHY:");
        var customer = people.OfType<Customer>().First();
        var premiumCustomer = people.OfType<PremiumCustomer>().First();

        customer.AddPurchase(500);
        customer.AddPurchase(1200);

        premiumCustomer.AddPurchase(2000);
        premiumCustomer.RequestPersonalShopper();

        Console.WriteLine($"Regular Customer Discount: {customer.GetDiscount():P}");
        Console.WriteLine($"Premium Customer Discount: {premiumCustomer.GetDiscount():P}");

        Console.WriteLine("\n7. METHOD OVERRIDING DEMONSTRATION:");
        Console.WriteLine("Different salary calculations:");

        var employees = people.OfType<Employee>().ToList();
        foreach (var emp in employees)
        {
            Console.WriteLine($"{emp.FullName} ({emp.GetType().Name}): ${emp.CalculateMonthlySalary():F2}/month, ${emp.CalculateYearlyBonus():F2} bonus");
        }

        Console.WriteLine("\n8. SEALED CLASS DEMONSTRATION:");
        var ceo1 = people.OfType<CEO>().First();
        ceo1.SetCompanyDirection("Innovation through technology");
        ceo1.ApproveExecutiveDecision("Expand to European markets");

        Console.WriteLine($"CEO cannot be inherited further (sealed class)");

        Console.WriteLine("\n9. RUNTIME TYPE INFORMATION:");
        var randomPerson = people[4]; // Get the designer
        Console.WriteLine($"Object: {randomPerson.FullName}");
        Console.WriteLine($"Runtime Type: {randomPerson.GetType().Name}");
        Console.WriteLine($"Base Type: {randomPerson.GetType().BaseType?.Name}");
        Console.WriteLine($"Is Person: {randomPerson is Person}");
        Console.WriteLine($"Is Employee: {randomPerson is Employee}");
        Console.WriteLine($"Is Designer: {randomPerson is Designer}");
        Console.WriteLine($"Is Manager: {randomPerson is Manager}");

        Console.WriteLine("\n10. PROTECTED MEMBER ACCESS:");
        var employee = people.OfType<Employee>().First();
        employee.SetEmployeeSSN("123-45-6789");
        Console.WriteLine("Employee can access protected SetSSN method from Person class");
    }
}

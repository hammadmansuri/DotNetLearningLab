// ==========================================
// 04-ENCAPSULATION - COMPLETE IMPLEMENTATION
// ==========================================

namespace EncapsulationDemo
{
    // ==========================================
    // 1. BASIC ENCAPSULATION - PRIVATE FIELDS & PUBLIC PROPERTIES
    // ==========================================

    public class BankAccount
    {
        // Private fields - internal state hidden from outside world
        private decimal _balance;
        private string _accountNumber;
        private readonly string _accountHolderName;
        private readonly DateTime _createdDate;

        // Constructor - controlled way to initialize object
        public BankAccount(string accountHolderName, string accountNumber, decimal initialBalance = 0)
        {
            if (string.IsNullOrWhiteSpace(accountHolderName))
                throw new ArgumentException("Account holder name cannot be empty");

            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentException("Account number cannot be empty");

            if (initialBalance < 0)
                throw new ArgumentException("Initial balance cannot be negative");

            _accountHolderName = accountHolderName;
            _accountNumber = accountNumber;
            _balance = initialBalance;
            _createdDate = DateTime.Now;
        }

        // Public properties with controlled access
        public string AccountHolderName => _accountHolderName; // Read-only property

        public string AccountNumber => _accountNumber; // Read-only property

        public decimal Balance => _balance; // Read-only property - balance can't be set directly

        public DateTime CreatedDate => _createdDate; // Read-only property

        // Controlled methods to modify internal state
        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive");

            _balance += amount;
            Console.WriteLine($"Deposited ${amount:F2}. New balance: ${_balance:F2}");
        }

        public bool Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive");

            if (amount > _balance)
            {
                Console.WriteLine($"Insufficient funds. Current balance: ${_balance:F2}");
                return false;
            }

            _balance -= amount;
            Console.WriteLine($"Withdrew ${amount:F2}. New balance: ${_balance:F2}");
            return true;
        }

        public override string ToString()
        {
            return $"Account: {_accountNumber}, Holder: {_accountHolderName}, Balance: ${_balance:F2}";
        }
    }

    // ==========================================
    // 2. PROPERTY ENCAPSULATION - GET/SET WITH VALIDATION
    // ==========================================

    public class Employee
    {
        private string _firstName;
        private string _lastName;
        private decimal _salary;
        private int _age;
        private string _email;

        // Property with full validation
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("First name cannot be empty");

                if (value.Length < 2)
                    throw new ArgumentException("First name must be at least 2 characters");

                _firstName = value.Trim();
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Last name cannot be empty");

                if (value.Length < 2)
                    throw new ArgumentException("Last name must be at least 2 characters");

                _lastName = value.Trim();
            }
        }

        // Property with business logic validation
        public decimal Salary
        {
            get => _salary;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Salary cannot be negative");

                if (value > 1_000_000)
                    throw new ArgumentException("Salary cannot exceed $1,000,000");

                _salary = value;
            }
        }

        // Property with range validation
        public int Age
        {
            get => _age;
            set
            {
                if (value < 16 || value > 70)
                    throw new ArgumentOutOfRangeException(nameof(value), "Age must be between 16 and 70");

                _age = value;
            }
        }

        // Property with format validation
        public string Email
        {
            get => _email;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Email cannot be empty");

                if (!value.Contains("@") || !value.Contains("."))
                    throw new ArgumentException("Invalid email format");

                _email = value.ToLower().Trim();
            }
        }

        // Computed property - no backing field
        public string FullName => $"{FirstName} {LastName}";

        // Property that combines multiple fields
        public string DisplayName => $"{FullName} ({Email})";

        public Employee(string firstName, string lastName, int age, string email, decimal salary)
        {
            // Using properties for validation
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            Email = email;
            Salary = salary;
        }

        public override string ToString()
        {
            return $"{DisplayName}, Age: {Age}, Salary: ${Salary:F2}";
        }
    }

    // ==========================================
    // 3. ACCESS MODIFIERS DEMONSTRATION
    // ==========================================

    public class AccessModifiersDemo
    {
        // Different access levels
        public string PublicField = "Accessible everywhere";
        private string _privateField = "Only within this class";
        protected string _protectedField = "This class and derived classes";
        internal string InternalField = "Within same assembly";
        protected internal string _protectedInternalField = "Protected OR internal";
        private protected string _privateProtectedField = "Protected AND internal";

        public void PublicMethod()
        {
            Console.WriteLine("Public method - accessible everywhere");
            PrivateMethod(); // Can call private method from within class
        }

        private void PrivateMethod()
        {
            Console.WriteLine("Private method - only within this class");
            Console.WriteLine($"Accessing private field: {_privateField}");
        }

        protected void ProtectedMethod()
        {
            Console.WriteLine("Protected method - this class and derived classes");
        }

        internal void InternalMethod()
        {
            Console.WriteLine("Internal method - within same assembly");
        }

        // Demonstrate access to all fields from within the class
        public void ShowAllFields()
        {
            Console.WriteLine($"Public: {PublicField}");
            Console.WriteLine($"Private: {_privateField}");
            Console.WriteLine($"Protected: {_protectedField}");
            Console.WriteLine($"Internal: {InternalField}");
            Console.WriteLine($"Protected Internal: {_protectedInternalField}");
            Console.WriteLine($"Private Protected: {_privateProtectedField}");
        }
    }

    // Derived class to show protected access
    public class DerivedAccessDemo : AccessModifiersDemo
    {
        public void AccessParentMembers()
        {
            // Can access public and protected members
            Console.WriteLine($"From derived class - Public: {PublicField}");
            Console.WriteLine($"From derived class - Protected: {_protectedField}");

            // Can call public methods
            PublicMethod();

            // Can call protected methods
            ProtectedMethod();

            // Cannot access private members
            // Console.WriteLine(_privateField); // Compilation error
            // PrivateMethod(); // Compilation error
        }
    }

    // ==========================================
    // 4. FIELD ENCAPSULATION PATTERNS
    // ==========================================

    public class FieldEncapsulationPatterns
    {
        // Auto-implemented properties (compiler creates backing field)
        public string Name { get; set; }
        public int Age { get; private set; } // Public get, private set

        // Property with private setter and validationy
        public DateTime CreatedDate { get; private set; } = DateTime.Now;

        // Init-only property (C# 9.0+)
        public string Id { get; init; }

        // Required property (C# 11.0+)
        public required string Department { get; set; }

        // Backing field with complex logic
        private List<string> _skills = new List<string>();

        public IReadOnlyList<string> Skills => _skills.AsReadOnly();

        public void AddSkill(string skill)
        {
            if (string.IsNullOrWhiteSpace(skill))
                throw new ArgumentException("Skill cannot be empty");

            if (_skills.Contains(skill, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Skill '{skill}' already exists");

            _skills.Add(skill);
        }

        public bool RemoveSkill(string skill)
        {
            return _skills.Remove(skill);
        }

        // Property with lazy initialization
        private List<string> _certifications;
        public IReadOnlyList<string> Certifications
        {
            get
            {
                _certifications ??= new List<string>();
                return _certifications.AsReadOnly();
            }
        }

        public void AddCertification(string certification)
        {
            _certifications ??= new List<string>();

            if (!_certifications.Contains(certification))
                _certifications.Add(certification);
        }

        // Method to update age (since setter is private)
        public void UpdateAge(int newAge)
        {
            if (newAge < 0 || newAge > 150)
                throw new ArgumentOutOfRangeException(nameof(newAge), "Invalid age");

            Age = newAge;
        }
    }

    // ==========================================
    // 5. IMMUTABLE CLASS EXAMPLE
    // ==========================================

    public class ImmutablePerson
    {
        // All fields are readonly
        private readonly string _firstName;
        private readonly string _lastName;
        private readonly DateTime _birthDate;
        private readonly List<string> _hobbies;

        // Constructor is the only way to set values
        public ImmutablePerson(string firstName, string lastName, DateTime birthDate, IEnumerable<string> hobbies = null)
        {
            _firstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            _lastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            _birthDate = birthDate;
            _hobbies = new List<string>(hobbies ?? Enumerable.Empty<string>());
        }

        // Only getters, no setters
        public string FirstName => _firstName;
        public string LastName => _lastName;
        public DateTime BirthDate => _birthDate;
        public string FullName => $"{_firstName} {_lastName}";
        public int Age => DateTime.Now.Year - _birthDate.Year;

        // Return defensive copy of mutable collection
        public IReadOnlyList<string> Hobbies => _hobbies.AsReadOnly();

        // Methods that return new instances instead of modifying current instance
        public ImmutablePerson WithFirstName(string newFirstName)
        {
            return new ImmutablePerson(newFirstName, _lastName, _birthDate, _hobbies);
        }

        public ImmutablePerson WithLastName(string newLastName)
        {
            return new ImmutablePerson(_firstName, newLastName, _birthDate, _hobbies);
        }

        public ImmutablePerson AddHobby(string hobby)
        {
            var newHobbies = new List<string>(_hobbies) { hobby };
            return new ImmutablePerson(_firstName, _lastName, _birthDate, newHobbies);
        }

        public override string ToString()
        {
            var hobbiesStr = _hobbies.Any() ? $", Hobbies: {string.Join(", ", _hobbies)}" : "";
            return $"{FullName}, Age: {Age}{hobbiesStr}";
        }

        // Proper equality implementation for immutable objects
        public override bool Equals(object obj)
        {
            if (obj is not ImmutablePerson other) return false;

            return _firstName == other._firstName &&
                   _lastName == other._lastName &&
                   _birthDate == other._birthDate &&
                   _hobbies.SequenceEqual(other._hobbies);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_firstName, _lastName, _birthDate, _hobbies.Count);
        }
    }

    // ==========================================
    // 6. ENCAPSULATION WITH INTERFACES
    // ==========================================

    public interface ISecureStorage
    {
        void Store(string key, string value);
        string Retrieve(string key);
        bool Exists(string key);
        void Remove(string key);
    }

    public class SecureStorage : ISecureStorage
    {
        // Private implementation details hidden behind interface
        private readonly Dictionary<string, string> _storage = new Dictionary<string, string>();
        private readonly object _lock = new object();

        public void Store(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be empty");

            lock (_lock)
            {
                _storage[key] = EncryptValue(value);
            }
        }

        public string Retrieve(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be empty");

            lock (_lock)
            {
                if (_storage.TryGetValue(key, out string encryptedValue))
                {
                    return DecryptValue(encryptedValue);
                }
                return null;
            }
        }

        public bool Exists(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            lock (_lock)
            {
                return _storage.ContainsKey(key);
            }
        }

        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            lock (_lock)
            {
                _storage.Remove(key);
            }
        }

        // Private implementation details - encapsulated
        private string EncryptValue(string value)
        {
            // Simple encryption simulation (in real world, use proper encryption)
            if (string.IsNullOrEmpty(value)) return value;

            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));
        }

        private string DecryptValue(string encryptedValue)
        {
            // Simple decryption simulation
            if (string.IsNullOrEmpty(encryptedValue)) return encryptedValue;

            try
            {
                byte[] bytes = Convert.FromBase64String(encryptedValue);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return null; // Corrupted data
            }
        }
    }

    // ==========================================
    // DEMONSTRATION PROGRAM
    // ==========================================

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ENCAPSULATION DEMONSTRATIONS ===\n");

            // 1. Basic Encapsulation
            Console.WriteLine("1. BASIC ENCAPSULATION - Bank Account");
            DemonstrateBankAccount();
            Console.WriteLine();

            // 2. Property Encapsulation
            Console.WriteLine("2. PROPERTY ENCAPSULATION - Employee");
            DemonstrateEmployee();
            Console.WriteLine();

            // 3. Access Modifiers
            Console.WriteLine("3. ACCESS MODIFIERS");
            DemonstrateAccessModifiers();
            Console.WriteLine();

            // 4. Field Encapsulation Patterns
            Console.WriteLine("4. FIELD ENCAPSULATION PATTERNS");
            DemonstrateFieldPatterns();
            Console.WriteLine();

            // 5. Immutable Objects
            Console.WriteLine("5. IMMUTABLE OBJECTS");
            DemonstrateImmutableObjects();
            Console.WriteLine();

            // 6. Interface Encapsulation
            Console.WriteLine("6. INTERFACE ENCAPSULATION");
            DemonstrateInterfaceEncapsulation();
        }

        static void DemonstrateBankAccount()
        {
            try
            {
                var account = new BankAccount("John Doe", "ACC001", 1000);
                Console.WriteLine(account);

                account.Deposit(500);
                account.Withdraw(200);
                account.Withdraw(2000); // Should fail

                // Cannot directly access private fields
                // account._balance = 5000; // Compilation error
                // Console.WriteLine($"Balance: {account._balance}"); // Compilation error

                Console.WriteLine($"Final balance: ${account.Balance:F2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void DemonstrateEmployee()
        {
            try
            {
                var employee = new Employee("John", "Smith", 30, "john.smith@email.com", 75000);
                Console.WriteLine(employee);

                // Modify through properties with validation
                employee.Salary = 80000;
                employee.Age = 31;

                Console.WriteLine($"Updated: {employee}");
                Console.WriteLine($"Full name: {employee.FullName}");

                // This would throw an exception
                // employee.Age = 100; // ArgumentOutOfRangeException
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void DemonstrateAccessModifiers()
        {
            var demo = new AccessModifiersDemo();

            // Public access
            demo.PublicMethod();
            Console.WriteLine($"Public field: {demo.PublicField}");

            // Internal access (same assembly)
            demo.InternalMethod();
            Console.WriteLine($"Internal field: {demo.InternalField}");

            // Show all fields from within class
            demo.ShowAllFields();

            // Derived class access
            var derived = new DerivedAccessDemo();
            derived.AccessParentMembers();
        }

        static void DemonstrateFieldPatterns()
        {
            var obj = new FieldEncapsulationPatterns
            {
                Name = "Alice Johnson",
                Id = "EMP001",
                Department = "Engineering"
            };

            obj.UpdateAge(28);
            obj.AddSkill("C#");
            obj.AddSkill("Azure");
            obj.AddCertification("Microsoft Certified");

            Console.WriteLine($"Name: {obj.Name}");
            Console.WriteLine($"Age: {obj.Age}");
            Console.WriteLine($"Department: {obj.Department}");
            Console.WriteLine($"Skills: {string.Join(", ", obj.Skills)}");
            Console.WriteLine($"Certifications: {string.Join(", ", obj.Certifications)}");
        }

        static void DemonstrateImmutableObjects()
        {
            var person = new ImmutablePerson("Jane", "Doe", new DateTime(1990, 5, 15),
                new[] { "Reading", "Swimming" });

            Console.WriteLine($"Original: {person}");

            // Create new instances with changes
            var personWithNewName = person.WithFirstName("Janet");
            var personWithHobby = person.AddHobby("Cycling");

            Console.WriteLine($"With new first name: {personWithNewName}");
            Console.WriteLine($"With additional hobby: {personWithHobby}");
            Console.WriteLine($"Original unchanged: {person}");
        }

        static void DemonstrateInterfaceEncapsulation()
        {
            ISecureStorage storage = new SecureStorage();

            storage.Store("password", "mySecretPassword123");
            storage.Store("apiKey", "abc123xyz789");

            Console.WriteLine($"Password exists: {storage.Exists("password")}");
            Console.WriteLine($"Retrieved password: {storage.Retrieve("password")}");
            Console.WriteLine($"Retrieved API key: {storage.Retrieve("apiKey")}");

            storage.Remove("password");
            Console.WriteLine($"Password exists after removal: {storage.Exists("password")}");

            // Implementation details are hidden - we can't access _storage directly
        }
    }
}
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesignPatterns
{
    // =============================================================================
    // CREATIONAL PATTERNS
    // =============================================================================

    // -----------------------------------------------------------------------------
    // 1. SINGLETON PATTERN
    // -----------------------------------------------------------------------------
    // "Ensure a class has only one instance and provide global access to it"

    /// <summary>
    /// ❌ BAD EXAMPLE: Not thread-safe singleton
    /// </summary>
    public class BadSingleton
    {
        private static BadSingleton _instance;

        private BadSingleton() { }

        public static BadSingleton Instance
        {
            get
            {
                if (_instance == null) // ❌ Race condition in multi-threaded environment
                {
                    _instance = new BadSingleton();
                }
                return _instance;
            }
        }
    }

    /// <summary>
    /// ✅ GOOD EXAMPLE: Thread-safe lazy singleton
    /// </summary>
    public sealed class ConfigurationManager
    {
        private static readonly Lazy<ConfigurationManager> _instance =
            new Lazy<ConfigurationManager>(() => new ConfigurationManager());

        private readonly Dictionary<string, string> _settings;

        private ConfigurationManager()
        {
            _settings = new Dictionary<string, string>();
            LoadConfiguration();
        }

        public static ConfigurationManager Instance => _instance.Value;

        public string GetSetting(string key)
        {
            return _settings.TryGetValue(key, out var value) ? value : null;
        }

        public void SetSetting(string key, string value)
        {
            _settings[key] = value;
        }

        private void LoadConfiguration()
        {
            // Simulate loading from config file
            _settings["DatabaseConnection"] = "Server=localhost;Database=MyApp";
            _settings["ApiTimeout"] = "30";
            _settings["LogLevel"] = "Info";
        }
    }

    /// <summary>
    /// ✅ BETTER APPROACH: Dependency Injection instead of Singleton
    /// Singletons make testing difficult and violate DIP
    /// </summary>
    public interface IConfigurationService
    {
        string GetSetting(string key);
        void SetSetting(string key, string value);
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly Dictionary<string, string> _settings = new();

        public ConfigurationService()
        {
            LoadConfiguration();
        }

        public string GetSetting(string key)
        {
            return _settings.TryGetValue(key, out var value) ? value : null;
        }

        public void SetSetting(string key, string value)
        {
            _settings[key] = value;
        }

        private void LoadConfiguration()
        {
            _settings["DatabaseConnection"] = "Server=localhost;Database=MyApp";
            _settings["ApiTimeout"] = "30";
            _settings["LogLevel"] = "Info";
        }
    }

    // -----------------------------------------------------------------------------
    // 2. FACTORY PATTERN
    // -----------------------------------------------------------------------------
    // "Create objects without specifying their concrete classes"

    /// <summary>
    /// ❌ BAD EXAMPLE: Creating objects directly with switch statements
    /// </summary>
    public class BadNotificationCreator
    {
        public object CreateNotification(string type, string message)
        {
            // Violates OCP - must modify this method for new notification types
            switch (type.ToLower())
            {
                case "email":
                    return new { Type = "Email", Message = message, To = "user@example.com" };
                case "sms":
                    return new { Type = "SMS", Message = message, Phone = "+1234567890" };
                default:
                    throw new ArgumentException("Unknown notification type");
            }
        }
    }

    /// <summary>
    /// ✅ GOOD EXAMPLE: Factory Pattern following OCP and DIP
    /// </summary>
    public interface INotification
    {
        Task SendAsync(string message, string recipient);
        string GetNotificationType();
    }

    public class EmailNotification : INotification
    {
        public async Task SendAsync(string message, string recipient)
        {
            Console.WriteLine($"Sending email to {recipient}: {message}");
            await Task.Delay(100); // Simulate email sending
        }

        public string GetNotificationType() => "Email";
    }

    public class SmsNotification : INotification
    {
        public async Task SendAsync(string message, string recipient)
        {
            Console.WriteLine($"Sending SMS to {recipient}: {message}");
            await Task.Delay(50); // Simulate SMS sending
        }

        public string GetNotificationType() => "SMS";
    }

    public class PushNotification : INotification
    {
        public async Task SendAsync(string message, string recipient)
        {
            Console.WriteLine($"Sending push notification to {recipient}: {message}");
            await Task.Delay(25); // Simulate push notification
        }

        public string GetNotificationType() => "Push";
    }

    // Factory interface
    public interface INotificationFactory
    {
        INotification CreateNotification(string type);
        IEnumerable<string> GetSupportedTypes();
    }

    // Concrete factory
    public class NotificationFactory : INotificationFactory
    {
        private readonly Dictionary<string, Func<INotification>> _factories;

        public NotificationFactory()
        {
            _factories = new Dictionary<string, Func<INotification>>(StringComparer.OrdinalIgnoreCase)
            {
                { "email", () => new EmailNotification() },
                { "sms", () => new SmsNotification() },
                { "push", () => new PushNotification() }
            };
        }

        public INotification CreateNotification(string type)
        {
            if (_factories.TryGetValue(type, out var factory))
            {
                return factory();
            }

            throw new ArgumentException($"Unsupported notification type: {type}");
        }

        public IEnumerable<string> GetSupportedTypes()
        {
            return _factories.Keys;
        }

        // ✅ Easy to extend - just add new factory registration
        public void RegisterFactory(string type, Func<INotification> factory)
        {
            _factories[type] = factory;
        }
    }

    // -----------------------------------------------------------------------------
    // 3. BUILDER PATTERN
    // -----------------------------------------------------------------------------
    // "Construct complex objects step by step"

    /// <summary>
    /// ❌ BAD EXAMPLE: Constructor with too many parameters
    /// </summary>
    public class BadEmailMessage
    {
        public BadEmailMessage(string to, string from, string subject, string body,
            bool isHtml, string cc, string bcc, bool isHighPriority,
            List<string> attachments, string replyTo, DateTime? scheduledTime)
        {
            // Constructor hell - hard to use and remember parameter order
        }
    }

    /// <summary>
    /// ✅ GOOD EXAMPLE: Builder Pattern with fluent interface
    /// </summary>
    public class EmailMessage
    {
        public string To { get; internal set; }
        public string From { get; internal set; }
        public string Subject { get; internal set; }
        public string Body { get; internal set; }
        public bool IsHtml { get; internal set; }
        public List<string> CcRecipients { get; internal set; } = new();
        public List<string> BccRecipients { get; internal set; } = new();
        public bool IsHighPriority { get; internal set; }
        public List<string> Attachments { get; internal set; } = new();
        public string ReplyTo { get; internal set; }
        public DateTime? ScheduledTime { get; internal set; }

        // Private constructor - force use of builder
        internal EmailMessage() { }
    }

    public class EmailMessageBuilder
    {
        private readonly EmailMessage _message = new();

        public EmailMessageBuilder To(string to)
        {
            _message.To = to;
            return this;
        }

        public EmailMessageBuilder From(string from)
        {
            _message.From = from;
            return this;
        }

        public EmailMessageBuilder Subject(string subject)
        {
            _message.Subject = subject;
            return this;
        }

        public EmailMessageBuilder Body(string body, bool isHtml = false)
        {
            _message.Body = body;
            _message.IsHtml = isHtml;
            return this;
        }

        public EmailMessageBuilder CC(string cc)
        {
            _message.CcRecipients.Add(cc);
            return this;
        }

        public EmailMessageBuilder BCC(string bcc)
        {
            _message.BccRecipients.Add(bcc);
            return this;
        }

        public EmailMessageBuilder HighPriority()
        {
            _message.IsHighPriority = true;
            return this;
        }

        public EmailMessageBuilder Attachment(string filePath)
        {
            _message.Attachments.Add(filePath);
            return this;
        }

        public EmailMessageBuilder ReplyTo(string replyTo)
        {
            _message.ReplyTo = replyTo;
            return this;
        }

        public EmailMessageBuilder ScheduleFor(DateTime scheduledTime)
        {
            _message.ScheduledTime = scheduledTime;
            return this;
        }

        public EmailMessage Build()
        {
            // Validation
            if (string.IsNullOrEmpty(_message.To))
                throw new InvalidOperationException("To recipient is required");
            if (string.IsNullOrEmpty(_message.Subject))
                throw new InvalidOperationException("Subject is required");

            return _message;
        }

        // Static factory method for fluent start
        public static EmailMessageBuilder Create() => new();
    }

    // =============================================================================
    // STRUCTURAL PATTERNS
    // =============================================================================

    // -----------------------------------------------------------------------------
    // 4. REPOSITORY PATTERN
    // -----------------------------------------------------------------------------
    // "Encapsulate data access logic and provide a uniform interface"

    /// <summary>
    /// ❌ BAD EXAMPLE: Data access scattered throughout business logic
    /// </summary>
    public class BadUserService
    {
        public void CreateUser(string name, string email)
        {
            // Business logic mixed with data access ❌
            var sql = $"INSERT INTO Users (Name, Email) VALUES ('{name}', '{email}')";
            // Execute SQL directly...

            // More business logic...
            var updateSql = $"UPDATE Users SET LastLogin = GETDATE() WHERE Email = '{email}'";
            // Execute SQL directly...
        }
    }

    /// <summary>
    /// ✅ GOOD EXAMPLE: Repository Pattern with clean separation
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
    }

    // Repository interface (supports DIP)
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string email);
    }

    // Concrete repository implementation
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();
        private int _nextId = 1;

        public Task<User> GetByIdAsync(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(user);
        }

        public Task<User> GetByEmailAsync(string email)
        {
            var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }

        public Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            var activeUsers = _users.Where(u => u.IsActive);
            return Task.FromResult(activeUsers);
        }

        public Task<User> CreateAsync(User user)
        {
            user.Id = _nextId++;
            user.CreatedAt = DateTime.UtcNow;
            _users.Add(user);
            return Task.FromResult(user);
        }

        public Task<User> UpdateAsync(User user)
        {
            var existingIndex = _users.FindIndex(u => u.Id == user.Id);
            if (existingIndex >= 0)
            {
                _users[existingIndex] = user;
                return Task.FromResult(user);
            }
            return Task.FromResult<User>(null);
        }

        public Task<bool> DeleteAsync(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _users.Remove(user);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> ExistsAsync(string email)
        {
            var exists = _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(exists);
        }
    }

    // Business service using repository (follows DIP)
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> RegisterUserAsync(string name, string email)
        {
            if (await _userRepository.ExistsAsync(email))
            {
                throw new InvalidOperationException("User already exists");
            }

            var user = new User
            {
                Name = name,
                Email = email,
                IsActive = true
            };

            return await _userRepository.CreateAsync(user);
        }

        public async Task UpdateLastLoginAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user != null)
            {
                user.LastLogin = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }
        }
    }

    // -----------------------------------------------------------------------------
    // 5. ADAPTER PATTERN
    // -----------------------------------------------------------------------------
    // "Allow incompatible interfaces to work together"

    /// <summary>
    /// Legacy payment system that we can't modify
    /// </summary>
    public class LegacyPaymentSystem
    {
        public bool ProcessCreditCard(string cardNumber, string expiry, double amount)
        {
            Console.WriteLine($"Legacy system: Processing ${amount} for card ending in {cardNumber.Substring(cardNumber.Length - 4)}");
            return true; // Simulate successful payment
        }

        public string GetTransactionId()
        {
            return $"LEGACY_{DateTime.Now.Ticks}";
        }
    }

    /// <summary>
    /// Modern payment interface that our application expects
    /// </summary>
    public interface IPaymentProcessor
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        string GetProcessorName();
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
    }

    public class PaymentResult
    {
        public bool IsSuccessful { get; set; }
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// ✅ ADAPTER: Makes legacy system compatible with modern interface
    /// </summary>
    public class LegacyPaymentAdapter : IPaymentProcessor
    {
        private readonly LegacyPaymentSystem _legacySystem;

        public LegacyPaymentAdapter(LegacyPaymentSystem legacySystem)
        {
            _legacySystem = legacySystem;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                // Adapt modern request to legacy system format
                var success = _legacySystem.ProcessCreditCard(
                    request.CardNumber,
                    request.ExpiryDate,
                    (double)request.Amount
                );

                await Task.Delay(100); // Simulate async operation

                if (success)
                {
                    return new PaymentResult
                    {
                        IsSuccessful = true,
                        TransactionId = _legacySystem.GetTransactionId()
                    };
                }
                else
                {
                    return new PaymentResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = "Legacy payment processing failed"
                    };
                }
            }
            catch (Exception ex)
            {
                return new PaymentResult
                {
                    IsSuccessful = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public string GetProcessorName() => "Legacy Payment Adapter";
    }

    // -----------------------------------------------------------------------------
    // 6. DECORATOR PATTERN
    // -----------------------------------------------------------------------------
    // "Add new functionality to objects without altering their structure"

    /// <summary>
    /// ❌ BAD EXAMPLE: Using inheritance for optional features
    /// </summary>
    public class BasicCoffee
    {
        public virtual decimal GetCost() => 2.00m;
        public virtual string GetDescription() => "Basic Coffee";
    }

    public class CoffeeWithMilk : BasicCoffee
    {
        public override decimal GetCost() => base.GetCost() + 0.50m;
        public override string GetDescription() => base.GetDescription() + ", Milk";
    }

    // What if we want coffee with milk AND sugar? Need another class!
    // This leads to class explosion ❌

    /// <summary>
    /// ✅ GOOD EXAMPLE: Decorator Pattern for flexible composition
    /// </summary>
    public interface ICoffee
    {
        decimal GetCost();
        string GetDescription();
    }

    public class SimpleCoffee : ICoffee
    {
        public decimal GetCost() => 2.00m;
        public string GetDescription() => "Simple Coffee";
    }

    // Base decorator
    public abstract class CoffeeDecorator : ICoffee
    {
        protected readonly ICoffee _coffee;

        protected CoffeeDecorator(ICoffee coffee)
        {
            _coffee = coffee;
        }

        public virtual decimal GetCost() => _coffee.GetCost();
        public virtual string GetDescription() => _coffee.GetDescription();
    }

    // Concrete decorators
    public class MilkDecorator : CoffeeDecorator
    {
        public MilkDecorator(ICoffee coffee) : base(coffee) { }

        public override decimal GetCost() => base.GetCost() + 0.50m;
        public override string GetDescription() => base.GetDescription() + ", Milk";
    }

    public class SugarDecorator : CoffeeDecorator
    {
        public SugarDecorator(ICoffee coffee) : base(coffee) { }

        public override decimal GetCost() => base.GetCost() + 0.25m;
        public override string GetDescription() => base.GetDescription() + ", Sugar";
    }

    public class WhipDecorator : CoffeeDecorator
    {
        public WhipDecorator(ICoffee coffee) : base(coffee) { }

        public override decimal GetCost() => base.GetCost() + 0.75m;
        public override string GetDescription() => base.GetDescription() + ", Whipped Cream";
    }

    // Real-world example: Logging decorator for repository
    public class LoggingUserRepositoryDecorator : IUserRepository
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public LoggingUserRepositoryDecorator(IUserRepository userRepository, ILogger logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            _logger.Log($"Getting user by ID: {id}");
            var user = await _userRepository.GetByIdAsync(id);
            _logger.Log($"Found user: {user?.Name ?? "Not found"}");
            return user;
        }

        public async Task<User> CreateAsync(User user)
        {
            _logger.Log($"Creating user: {user.Name}");
            var createdUser = await _userRepository.CreateAsync(user);
            _logger.Log($"Created user with ID: {createdUser.Id}");
            return createdUser;
        }

        // Implement other methods similarly...
        public Task<User> GetByEmailAsync(string email) => _userRepository.GetByEmailAsync(email);
        public Task<IEnumerable<User>> GetActiveUsersAsync() => _userRepository.GetActiveUsersAsync();
        public Task<User> UpdateAsync(User user) => _userRepository.UpdateAsync(user);
        public Task<bool> DeleteAsync(int id) => _userRepository.DeleteAsync(id);
        public Task<bool> ExistsAsync(string email) => _userRepository.ExistsAsync(email);
    }

    // =============================================================================
    // BEHAVIORAL PATTERNS
    // =============================================================================

    // -----------------------------------------------------------------------------
    // 7. STRATEGY PATTERN
    // -----------------------------------------------------------------------------
    // "Define family of algorithms and make them interchangeable"

    /// <summary>
    /// ❌ BAD EXAMPLE: Hard-coded algorithm selection
    /// </summary>
    public class BadShippingCalculator
    {
        public decimal CalculateShipping(decimal weight, string method)
        {
            // Violates OCP - must modify this method for new shipping methods
            switch (method.ToLower())
            {
                case "standard":
                    return weight * 1.5m;
                case "express":
                    return weight * 3.0m;
                case "overnight":
                    return weight * 5.0m;
                default:
                    throw new ArgumentException("Unknown shipping method");
            }
        }
    }

    /// <summary>
    /// ✅ GOOD EXAMPLE: Strategy Pattern following OCP
    /// </summary>
    public interface IShippingStrategy
    {
        decimal CalculateShipping(decimal weight, decimal distance);
        string GetMethodName();
        TimeSpan GetEstimatedDeliveryTime();
    }

    public class StandardShippingStrategy : IShippingStrategy
    {
        public decimal CalculateShipping(decimal weight, decimal distance)
        {
            return (weight * 1.5m) + (distance * 0.1m);
        }

        public string GetMethodName() => "Standard Shipping";
        public TimeSpan GetEstimatedDeliveryTime() => TimeSpan.FromDays(5);
    }

    public class ExpressShippingStrategy : IShippingStrategy
    {
        public decimal CalculateShipping(decimal weight, decimal distance)
        {
            return (weight * 3.0m) + (distance * 0.2m);
        }

        public string GetMethodName() => "Express Shipping";
        public TimeSpan GetEstimatedDeliveryTime() => TimeSpan.FromDays(2);
    }

    public class OvernightShippingStrategy : IShippingStrategy
    {
        public decimal CalculateShipping(decimal weight, decimal distance)
        {
            return (weight * 5.0m) + (distance * 0.5m) + 10.0m; // Base overnight fee
        }

        public string GetMethodName() => "Overnight Shipping";
        public TimeSpan GetEstimatedDeliveryTime() => TimeSpan.FromDays(1);
    }

    public class ShippingCalculator
    {
        private readonly Dictionary<string, IShippingStrategy> _strategies;

        public ShippingCalculator()
        {
            _strategies = new Dictionary<string, IShippingStrategy>(StringComparer.OrdinalIgnoreCase)
            {
                { "standard", new StandardShippingStrategy() },
                { "express", new ExpressShippingStrategy() },
                { "overnight", new OvernightShippingStrategy() }
            };
        }

        public ShippingQuote CalculateShipping(string method, decimal weight, decimal distance)
        {
            if (!_strategies.TryGetValue(method, out var strategy))
            {
                throw new ArgumentException($"Unknown shipping method: {method}");
            }

            return new ShippingQuote
            {
                Method = strategy.GetMethodName(),
                Cost = strategy.CalculateShipping(weight, distance),
                EstimatedDelivery = DateTime.Now.Add(strategy.GetEstimatedDeliveryTime())
            };
        }

        // ✅ Easy to extend - register new strategies
        public void RegisterStrategy(string name, IShippingStrategy strategy)
        {
            _strategies[name] = strategy;
        }

        public IEnumerable<string> GetAvailableMethods()
        {
            return _strategies.Keys;
        }
    }

    public class ShippingQuote
    {
        public string Method { get; set; }
        public decimal Cost { get; set; }
        public DateTime EstimatedDelivery { get; set; }
    }

    // -----------------------------------------------------------------------------
    // 8. OBSERVER PATTERN
    // -----------------------------------------------------------------------------
    // "Define one-to-many dependency between objects"

    /// <summary>
    /// ✅ Observer Pattern implementation
    /// </summary>
    public interface IOrderObserver
    {
        Task OnOrderCreatedAsync(Order order);
        Task OnOrderStatusChangedAsync(Order order, OrderStatus oldStatus);
        string GetObserverName();
    }

    public class Order : INotifyPropertyChanged
    {
        private OrderStatus _status;
        private readonly List<IOrderObserver> _observers = new();

        public int Id { get; set; }
        public string CustomerEmail { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }

        public OrderStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    var oldStatus = _status;
                    _status = value;
                    OnPropertyChanged();

                    // Notify observers
                    NotifyStatusChanged(oldStatus);
                }
            }
        }

        public void AddObserver(IOrderObserver observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(IOrderObserver observer)
        {
            _observers.Remove(observer);
        }

        public async Task NotifyCreatedAsync()
        {
            foreach (var observer in _observers)
            {
                try
                {
                    await observer.OnOrderCreatedAsync(this);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Observer {observer.GetObserverName()} failed: {ex.Message}");
                }
            }
        }

        private async void NotifyStatusChanged(OrderStatus oldStatus)
        {
            foreach (var observer in _observers)
            {
                try
                {
                    await observer.OnOrderStatusChangedAsync(this, oldStatus);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Observer {observer.GetObserverName()} failed: {ex.Message}");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum OrderStatus
    {
        Created,
        Paid,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

    // Concrete observers
    public class EmailNotificationObserver : IOrderObserver
    {
        public async Task OnOrderCreatedAsync(Order order)
        {
            Console.WriteLine($"📧 Sending order confirmation email to {order.CustomerEmail}");
            await Task.Delay(100); // Simulate email sending
        }

        public async Task OnOrderStatusChangedAsync(Order order, OrderStatus oldStatus)
        {
            Console.WriteLine($"📧 Sending status update email: {oldStatus} → {order.Status}");
            await Task.Delay(100);
        }

        public string GetObserverName() => "Email Notification";
    }

    public class InventoryObserver : IOrderObserver
    {
        public async Task OnOrderCreatedAsync(Order order)
        {
            Console.WriteLine($"📦 Reserving inventory for order {order.Id}");
            await Task.Delay(50);
        }

        public async Task OnOrderStatusChangedAsync(Order order, OrderStatus oldStatus)
        {
            if (order.Status == OrderStatus.Cancelled)
            {
                Console.WriteLine($"📦 Releasing inventory for cancelled order {order.Id}");
                await Task.Delay(50);
            }
        }

        public string GetObserverName() => "Inventory Management";
    }

    public class AnalyticsObserver : IOrderObserver
    {
        public async Task OnOrderCreatedAsync(Order order)
        {
            Console.WriteLine($"📊 Recording order analytics: ${order.TotalAmount}");
            await Task.Delay(25);
        }

        public async Task OnOrderStatusChangedAsync(Order order, OrderStatus oldStatus)
        {
            Console.WriteLine($"📊 Recording status change analytics: {oldStatus} → {order.Status}");
            await Task.Delay(25);
        }

        public string GetObserverName() => "Analytics";
    }

    // -----------------------------------------------------------------------------
    // 9. COMMAND PATTERN
    // -----------------------------------------------------------------------------
    // "Encapsulate requests as objects, enabling parameterization and queuing"

    /// <summary>
    /// Command interface
    /// </summary>
    public interface ICommand
    {
        Task ExecuteAsync();
        Task UndoAsync();
        string GetDescription();
    }

    /// <summary>
    /// Receiver - the object that performs the work
    /// </summary>
    public class BankAccount
    {
        public string AccountNumber { get; }
        public decimal Balance { get; private set; }
        private readonly List<string> _transactionHistory = new();

        public BankAccount(string accountNumber, decimal initialBalance = 0)
        {
            AccountNumber = accountNumber;
            Balance = initialBalance;
        }

        public void Deposit(decimal amount)
        {
            Balance += amount;
            _transactionHistory.Add($"Deposited ${amount:F2} - Balance: ${Balance:F2}");
            Console.WriteLine($"💰 Deposited ${amount:F2} to {AccountNumber}. New balance: ${Balance:F2}");
        }

        public void Withdraw(decimal amount)
        {
            if (Balance >= amount)
            {
                Balance -= amount;
                _transactionHistory.Add($"Withdrew ${amount:F2} - Balance: ${Balance:F2}");
                Console.WriteLine($"💸 Withdrew ${amount:F2} from {AccountNumber}. New balance: ${Balance:F2}");
            }
            else
            {
                throw new InvalidOperationException("Insufficient funds");
            }
        }

        public void Transfer(BankAccount toAccount, decimal amount)
        {
            Withdraw(amount);
            toAccount.Deposit(amount);
            Console.WriteLine($"🔄 Transferred ${amount:F2} from {AccountNumber} to {toAccount.AccountNumber}");
        }

        public IEnumerable<string> GetTransactionHistory() => _transactionHistory.AsReadOnly();
    }

    /// <summary>
    /// Concrete Commands
    /// </summary>
    public class DepositCommand : ICommand
    {
        private readonly BankAccount _account;
        private readonly decimal _amount;

        public DepositCommand(BankAccount account, decimal amount)
        {
            _account = account;
            _amount = amount;
        }

        public Task ExecuteAsync()
        {
            _account.Deposit(_amount);
            return Task.CompletedTask;
        }

        public Task UndoAsync()
        {
            _account.Withdraw(_amount);
            Console.WriteLine($"🔙 Undoing deposit of ${_amount:F2}");
            return Task.CompletedTask;
        }

        public string GetDescription() => $"Deposit ${_amount:F2} to {_account.AccountNumber}";
    }

    public class WithdrawCommand : ICommand
    {
        private readonly BankAccount _account;
        private readonly decimal _amount;

        public WithdrawCommand(BankAccount account, decimal amount)
        {
            _account = account;
            _amount = amount;
        }

        public Task ExecuteAsync()
        {
            _account.Withdraw(_amount);
            return Task.CompletedTask;
        }

        public Task UndoAsync()
        {
            _account.Deposit(_amount);
            Console.WriteLine($"🔙 Undoing withdrawal of ${_amount:F2}");
            return Task.CompletedTask;
        }

        public string GetDescription() => $"Withdraw ${_amount:F2} from {_account.AccountNumber}";
    }

    public class TransferCommand : ICommand
    {
        private readonly BankAccount _fromAccount;
        private readonly BankAccount _toAccount;
        private readonly decimal _amount;

        public TransferCommand(BankAccount fromAccount, BankAccount toAccount, decimal amount)
        {
            _fromAccount = fromAccount;
            _toAccount = toAccount;
            _amount = amount;
        }

        public Task ExecuteAsync()
        {
            _fromAccount.Transfer(_toAccount, _amount);
            return Task.CompletedTask;
        }

        public Task UndoAsync()
        {
            _toAccount.Transfer(_fromAccount, _amount);
            Console.WriteLine($"🔙 Undoing transfer of ${_amount:F2}");
            return Task.CompletedTask;
        }

        public string GetDescription() => $"Transfer ${_amount:F2} from {_fromAccount.AccountNumber} to {_toAccount.AccountNumber}";
    }

    /// <summary>
    /// Invoker - manages and executes commands
    /// </summary>
    public class BankTransactionManager
    {
        private readonly Stack<ICommand> _commandHistory = new();
        private readonly Queue<ICommand> _commandQueue = new();

        public async Task ExecuteCommandAsync(ICommand command)
        {
            try
            {
                await command.ExecuteAsync();
                _commandHistory.Push(command);
                Console.WriteLine($"✅ Executed: {command.GetDescription()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to execute: {command.GetDescription()} - {ex.Message}");
            }
        }

        public async Task UndoLastCommandAsync()
        {
            if (_commandHistory.Count > 0)
            {
                var lastCommand = _commandHistory.Pop();
                try
                {
                    await lastCommand.UndoAsync();
                    Console.WriteLine($"↩️ Undone: {lastCommand.GetDescription()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to undo: {lastCommand.GetDescription()} - {ex.Message}");
                    // Put it back if undo failed
                    _commandHistory.Push(lastCommand);
                }
            }
            else
            {
                Console.WriteLine("No commands to undo");
            }
        }

        public void QueueCommand(ICommand command)
        {
            _commandQueue.Enqueue(command);
            Console.WriteLine($"📋 Queued: {command.GetDescription()}");
        }

        public async Task ExecuteQueuedCommandsAsync()
        {
            Console.WriteLine($"🚀 Executing {_commandQueue.Count} queued commands...");

            while (_commandQueue.Count > 0)
            {
                var command = _commandQueue.Dequeue();
                await ExecuteCommandAsync(command);
                await Task.Delay(100); // Simulate processing time
            }
        }

        public IEnumerable<string> GetCommandHistory()
        {
            return _commandHistory.Reverse().Select(cmd => cmd.GetDescription());
        }
    }

    // =============================================================================
    // ENTERPRISE PATTERNS
    // =============================================================================

    // -----------------------------------------------------------------------------
    // 10. UNIT OF WORK PATTERN
    // -----------------------------------------------------------------------------
    // "Maintain a list of objects affected by a business transaction and coordinate changes"

    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IOrderRepository Orders { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }

    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(int id);
        Task<Order> CreateAsync(Order order);
        Task<Order> UpdateAsync(Order order);
        Task<IEnumerable<Order>> GetByCustomerEmailAsync(string email);
    }

    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders = new();
        private int _nextId = 1;

        public Task<Order> GetByIdAsync(int id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            return Task.FromResult(order);
        }

        public Task<Order> CreateAsync(Order order)
        {
            order.Id = _nextId++;
            _orders.Add(order);
            return Task.FromResult(order);
        }

        public Task<Order> UpdateAsync(Order order)
        {
            var existingIndex = _orders.FindIndex(o => o.Id == order.Id);
            if (existingIndex >= 0)
            {
                _orders[existingIndex] = order;
                return Task.FromResult(order);
            }
            return Task.FromResult<Order>(null);
        }

        public Task<IEnumerable<Order>> GetByCustomerEmailAsync(string email)
        {
            var orders = _orders.Where(o => o.CustomerEmail.Equals(email, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(orders);
        }
    }

    public class InMemoryUnitOfWork : IUnitOfWork
    {
        private readonly List<Action> _changes = new();
        private bool _isInTransaction = false;
        private bool _disposed = false;

        public IUserRepository Users { get; }
        public IOrderRepository Orders { get; }

        public InMemoryUnitOfWork()
        {
            Users = new InMemoryUserRepository();
            Orders = new InMemoryOrderRepository();
        }

        public async Task<int> SaveChangesAsync()
        {
            if (_isInTransaction)
            {
                // In a real implementation, this would save all changes atomically
                Console.WriteLine($"💾 Saving {_changes.Count} changes within transaction");
            }
            else
            {
                Console.WriteLine($"💾 Saving {_changes.Count} changes");
            }

            var changeCount = _changes.Count;
            _changes.Clear();

            await Task.Delay(50); // Simulate database save
            return changeCount;
        }

        public Task BeginTransactionAsync()
        {
            _isInTransaction = true;
            Console.WriteLine("🔄 Transaction started");
            return Task.CompletedTask;
        }

        public Task CommitTransactionAsync()
        {
            if (_isInTransaction)
            {
                _isInTransaction = false;
                Console.WriteLine("✅ Transaction committed");
            }
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync()
        {
            if (_isInTransaction)
            {
                _changes.Clear();
                _isInTransaction = false;
                Console.WriteLine("❌ Transaction rolled back");
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_isInTransaction)
                {
                    RollbackTransactionAsync().Wait();
                }
                _disposed = true;
                Console.WriteLine("🧹 Unit of Work disposed");
            }
        }
    }

    // Service using Unit of Work
    public class OrderManagementService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderManagementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Order> CreateOrderWithUserAsync(string customerName, string customerEmail, decimal amount)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Create or get user
                var user = await _unitOfWork.Users.GetByEmailAsync(customerEmail);
                if (user == null)
                {
                    user = new User
                    {
                        Name = customerName,
                        Email = customerEmail,
                        IsActive = true
                    };
                    user = await _unitOfWork.Users.CreateAsync(user);
                }

                // Create order
                var order = new Order
                {
                    CustomerEmail = customerEmail,
                    TotalAmount = amount,
                    Status = OrderStatus.Created,
                    CreatedAt = DateTime.UtcNow
                };
                order = await _unitOfWork.Orders.CreateAsync(order);

                // Save all changes atomically
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                Console.WriteLine($"✅ Created order {order.Id} for user {user.Name}");
                return order;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Console.WriteLine($"❌ Failed to create order: {ex.Message}");
                throw;
            }
        }
    }

    // -----------------------------------------------------------------------------
    // 11. SPECIFICATION PATTERN
    // -----------------------------------------------------------------------------
    // "Encapsulate business rules as reusable and combinable specifications"

    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T candidate);
        ISpecification<T> And(ISpecification<T> other);
        ISpecification<T> Or(ISpecification<T> other);
        ISpecification<T> Not();
    }

    public abstract class Specification<T> : ISpecification<T>
    {
        public abstract bool IsSatisfiedBy(T candidate);

        public ISpecification<T> And(ISpecification<T> other)
        {
            return new AndSpecification<T>(this, other);
        }

        public ISpecification<T> Or(ISpecification<T> other)
        {
            return new OrSpecification<T>(this, other);
        }

        public ISpecification<T> Not()
        {
            return new NotSpecification<T>(this);
        }
    }

    // Composite specifications
    public class AndSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public AndSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override bool IsSatisfiedBy(T candidate)
        {
            return _left.IsSatisfiedBy(candidate) && _right.IsSatisfiedBy(candidate);
        }
    }

    public class OrSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public OrSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override bool IsSatisfiedBy(T candidate)
        {
            return _left.IsSatisfiedBy(candidate) || _right.IsSatisfiedBy(candidate);
        }
    }

    public class NotSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _specification;

        public NotSpecification(ISpecification<T> specification)
        {
            _specification = specification;
        }

        public override bool IsSatisfiedBy(T candidate)
        {
            return !_specification.IsSatisfiedBy(candidate);
        }
    }

    // Business rule specifications
    public class ActiveUserSpecification : Specification<User>
    {
        public override bool IsSatisfiedBy(User user)
        {
            return user != null && user.IsActive;
        }
    }

    public class RecentUserSpecification : Specification<User>
    {
        private readonly TimeSpan _timeSpan;

        public RecentUserSpecification(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
        }

        public override bool IsSatisfiedBy(User user)
        {
            return user != null && user.CreatedAt > DateTime.UtcNow.Subtract(_timeSpan);
        }
    }

    public class EmailDomainSpecification : Specification<User>
    {
        private readonly string _domain;

        public EmailDomainSpecification(string domain)
        {
            _domain = domain;
        }

        public override bool IsSatisfiedBy(User user)
        {
            return user != null &&
                   user.Email != null &&
                   user.Email.EndsWith($"@{_domain}", StringComparison.OrdinalIgnoreCase);
        }
    }

    // Order specifications
    public class HighValueOrderSpecification : Specification<Order>
    {
        private readonly decimal _threshold;

        public HighValueOrderSpecification(decimal threshold)
        {
            _threshold = threshold;
        }

        public override bool IsSatisfiedBy(Order order)
        {
            return order != null && order.TotalAmount >= _threshold;
        }
    }

    public class RecentOrderSpecification : Specification<Order>
    {
        private readonly TimeSpan _timeSpan;

        public RecentOrderSpecification(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
        }

        public override bool IsSatisfiedBy(Order order)
        {
            return order != null && order.CreatedAt > DateTime.UtcNow.Subtract(_timeSpan);
        }
    }

    // Service using specifications
    public class CustomerAnalyticsService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;

        public CustomerAnalyticsService(IUserRepository userRepository, IOrderRepository orderRepository)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<User>> GetEligibleUsersForPromotionAsync()
        {
            // Business rule: Active users from company domain who joined recently
            var eligibleUserSpec = new ActiveUserSpecification()
                .And(new EmailDomainSpecification("company.com"))
                .And(new RecentUserSpecification(TimeSpan.FromDays(30)));

            var allUsers = await _userRepository.GetActiveUsersAsync();
            return allUsers.Where(user => eligibleUserSpec.IsSatisfiedBy(user));
        }

        public async Task<IEnumerable<Order>> GetHighValueRecentOrdersAsync()
        {
            // Business rule: High value orders from the last week
            var highValueRecentSpec = new HighValueOrderSpecification(1000m)
                .And(new RecentOrderSpecification(TimeSpan.FromDays(7)));

            // In a real implementation, this would filter at the database level
            var recentOrders = new List<Order>(); // Get from repository
            return recentOrders.Where(order => highValueRecentSpec.IsSatisfiedBy(order));
        }
    }

    // =============================================================================
    // SUPPORTING INTERFACES AND LOGGER
    // =============================================================================

    public interface ILogger
    {
        void Log(string message);
        void LogError(string message, Exception exception = null);
    }

    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[LOG] {DateTime.Now:HH:mm:ss} - {message}");
        }

        public void LogError(string message, Exception exception = null)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} - {message}");
            if (exception != null)
            {
                Console.WriteLine($"Exception: {exception.Message}");
            }
        }
    }

    // =============================================================================
    // DEMONSTRATION PROGRAM
    // =============================================================================

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== DESIGN PATTERNS DEMONSTRATION ===\n");

            DemonstrateCreationalPatterns();
            Console.WriteLine("\n" + new string('=', 80) + "\n");

            await DemonstrateStructuralPatterns();
            Console.WriteLine("\n" + new string('=', 80) + "\n");

            await DemonstrateBehavioralPatterns();
            Console.WriteLine("\n" + new string('=', 80) + "\n");

            await DemonstrateEnterprisePatterns();
        }

        private static void DemonstrateCreationalPatterns()
        {
            Console.WriteLine("1. CREATIONAL PATTERNS");
            Console.WriteLine("Creating objects in flexible and reusable ways\n");

            // Singleton Pattern
            Console.WriteLine("=== Singleton Pattern ===");
            var config1 = ConfigurationManager.Instance;
            var config2 = ConfigurationManager.Instance;
            Console.WriteLine($"Same instance: {ReferenceEquals(config1, config2)}");
            Console.WriteLine($"Database setting: {config1.GetSetting("DatabaseConnection")}");

            // Factory Pattern
            Console.WriteLine("\n=== Factory Pattern ===");
            var factory = new NotificationFactory();
            foreach (var type in factory.GetSupportedTypes())
            {
                var notification = factory.CreateNotification(type);
                Console.WriteLine($"Created: {notification.GetNotificationType()}");
            }

            // Builder Pattern
            Console.WriteLine("\n=== Builder Pattern ===");
            var email = EmailMessageBuilder
                .Create()
                .To("john@example.com")
                .From("noreply@company.com")
                .Subject("Welcome to our service!")
                .Body("<h1>Welcome!</h1><p>Thanks for joining us.</p>", isHtml: true)
                .CC("manager@company.com")
                .HighPriority()
                .Attachment("welcome_guide.pdf")
                .Build();

            Console.WriteLine($"Built email: {email.Subject} to {email.To}");
            Console.WriteLine($"High priority: {email.IsHighPriority}, Attachments: {email.Attachments.Count}");
        }

        private static async Task DemonstrateStructuralPatterns()
        {
            Console.WriteLine("2. STRUCTURAL PATTERNS");
            Console.WriteLine("Composing objects into larger structures\n");

            // Repository Pattern
            Console.WriteLine("=== Repository Pattern ===");
            IUserRepository userRepo = new InMemoryUserRepository();
            var userService = new UserService(userRepo);

            var user = await userService.RegisterUserAsync("John Doe", "john@example.com");
            Console.WriteLine($"Registered user: {user.Name} (ID: {user.Id})");

            // Adapter Pattern
            Console.WriteLine("\n=== Adapter Pattern ===");
            var legacySystem = new LegacyPaymentSystem();
            IPaymentProcessor processor = new LegacyPaymentAdapter(legacySystem);

            var paymentRequest = new PaymentRequest
            {
                Amount = 99.99m,
                Currency = "USD",
                CardNumber = "1234567890123456",
                ExpiryDate = "12/25"
            };

            var result = await processor.ProcessPaymentAsync(paymentRequest);
            Console.WriteLine($"Payment result: {result.IsSuccessful}, Transaction: {result.TransactionId}");

            // Decorator Pattern
            Console.WriteLine("\n=== Decorator Pattern ===");
            ICoffee coffee = new SimpleCoffee();
            coffee = new MilkDecorator(coffee);
            coffee = new SugarDecorator(coffee);
            coffee = new WhipDecorator(coffee);

            Console.WriteLine($"Coffee: {coffee.GetDescription()}");
            Console.WriteLine($"Total cost: ${coffee.GetCost():F2}");
        }

        private static async Task DemonstrateBehavioralPatterns()
        {
            Console.WriteLine("3. BEHAVIORAL PATTERNS");
            Console.WriteLine("Algorithms and assignment of responsibilities\n");

            // Strategy Pattern
            Console.WriteLine("=== Strategy Pattern ===");
            var shippingCalculator = new ShippingCalculator();
            var methods = new[] { "standard", "express", "overnight" };

            foreach (var method in methods)
            {
                var quote = shippingCalculator.CalculateShipping(method, 5.0m, 100.0m);
                Console.WriteLine($"{quote.Method}: ${quote.Cost:F2} - Delivery: {quote.EstimatedDelivery:MM/dd/yyyy}");
            }

            // Observer Pattern
            Console.WriteLine("\n=== Observer Pattern ===");
            var order = new Order
            {
                Id = 1,
                CustomerEmail = "customer@example.com",
                TotalAmount = 150.00m,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Created
            };

            // Add observers
            order.AddObserver(new EmailNotificationObserver());
            order.AddObserver(new InventoryObserver());
            order.AddObserver(new AnalyticsObserver());

            await order.NotifyCreatedAsync();
            order.Status = OrderStatus.Paid;
            order.Status = OrderStatus.Shipped;

            // Command Pattern
            Console.WriteLine("\n=== Command Pattern ===");
            var account1 = new BankAccount("ACC001", 1000m);
            var account2 = new BankAccount("ACC002", 500m);
            var transactionManager = new BankTransactionManager();

            // Execute commands
            await transactionManager.ExecuteCommandAsync(new DepositCommand(account1, 200m));
            await transactionManager.ExecuteCommandAsync(new WithdrawCommand(account1, 100m));
            await transactionManager.ExecuteCommandAsync(new TransferCommand(account1, account2, 300m));

            Console.WriteLine($"Account 1 balance: ${account1.Balance:F2}");
            Console.WriteLine($"Account 2 balance: ${account2.Balance:F2}");

            // Undo last operation
            await transactionManager.UndoLastCommandAsync();
            Console.WriteLine($"After undo - Account 1: ${account1.Balance:F2}, Account 2: ${account2.Balance:F2}");
        }

        private static async Task DemonstrateEnterprisePatterns()
        {
            Console.WriteLine("4. ENTERPRISE PATTERNS");
            Console.WriteLine("Patterns for complex business applications\n");

            // Unit of Work Pattern
            Console.WriteLine("=== Unit of Work Pattern ===");
            using var unitOfWork = new InMemoryUnitOfWork();
            var orderManagement = new OrderManagementService(unitOfWork);

            var newOrder = await orderManagement.CreateOrderWithUserAsync(
                "Jane Smith", "jane@company.com", 299.99m);

            Console.WriteLine($"Created order {newOrder.Id} for ${newOrder.TotalAmount:F2}");

            // Specification Pattern
            Console.WriteLine("\n=== Specification Pattern ===");
            var userRepo = new InMemoryUserRepository();
            var orderRepo = new InMemoryOrderRepository();

            // Create test data
            await userRepo.CreateAsync(new User
            {
                Name = "Active User",
                Email = "user@company.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            });

            await userRepo.CreateAsync(new User
            {
                Name = "Old User",
                Email = "olduser@company.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            });

            var analytics = new CustomerAnalyticsService(userRepo, orderRepo);
            var eligibleUsers = await analytics.GetEligibleUsersForPromotionAsync();

            Console.WriteLine($"Found {eligibleUsers.Count()} users eligible for promotion:");
            foreach (var user in eligibleUsers)
            {
                Console.WriteLine($"  - {user.Name} ({user.Email})");
            }

            Console.WriteLine("\n✅ Design Patterns Summary:");
            Console.WriteLine("  - Creational: Object creation with flexibility and reuse");
            Console.WriteLine("  - Structural: Composing objects and managing relationships");
            Console.WriteLine("  - Behavioral: Communication between objects and algorithms");
            Console.WriteLine("  - Enterprise: Complex business logic and data management");
        }
    }
}
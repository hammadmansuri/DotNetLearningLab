namespace AbstractionLearning
{
    // =============================================================================
    // 1. INTERFACE ABSTRACTION - Highest level of abstraction
    // =============================================================================

    /// <summary>
    /// Interface defines WHAT can be done, not HOW
    /// Pure abstraction - no implementation details
    /// </summary>
    public interface INotificationService
    {
        Task SendAsync(string recipient, string message);
        bool IsAvailable { get; }
    }

    public interface IPaymentProcessor
    {
        Task<PaymentResult> ProcessPaymentAsync(decimal amount, string currency);
        bool SupportsRefunds { get; }
    }

    // =============================================================================
    // 2. ABSTRACT CLASS ABSTRACTION - Partial abstraction with common behavior
    // =============================================================================

    /// <summary>
    /// Abstract class provides SOME implementation while forcing subclasses
    /// to implement specific behavior. Mix of concrete and abstract.
    /// </summary>
    public abstract class DatabaseConnection
    {
        protected string ConnectionString { get; }
        protected bool IsConnected { get; private set; }

        protected DatabaseConnection(string connectionString)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // Concrete method - common behavior for all database types
        public virtual async Task<bool> ConnectAsync()
        {
            Console.WriteLine($"Establishing connection to: {GetDatabaseType()}");

            // Simulate connection logic
            await Task.Delay(100);
            IsConnected = await EstablishConnectionAsync();

            if (IsConnected)
            {
                Console.WriteLine("Connection established successfully");
                await OnConnectionEstablished();
            }

            return IsConnected;
        }

        public virtual void Disconnect()
        {
            if (IsConnected)
            {
                Console.WriteLine($"Disconnecting from {GetDatabaseType()}");
                IsConnected = false;
                OnDisconnected();
            }
        }

        // Abstract methods - MUST be implemented by concrete classes
        protected abstract Task<bool> EstablishConnectionAsync();
        protected abstract string GetDatabaseType();
        public abstract Task<T> ExecuteQueryAsync<T>(string query);

        // Virtual methods - CAN be overridden by concrete classes
        protected virtual Task OnConnectionEstablished()
        {
            Console.WriteLine("Base connection setup completed");
            return Task.CompletedTask;
        }

        protected virtual void OnDisconnected()
        {
            Console.WriteLine("Base cleanup completed");
        }
    }

    // =============================================================================
    // 3. CONCRETE IMPLEMENTATIONS - Hide specific implementation details
    // =============================================================================

    public class SqlServerConnection : DatabaseConnection
    {
        public SqlServerConnection(string connectionString) : base(connectionString) { }

        protected override async Task<bool> EstablishConnectionAsync()
        {
            // SQL Server specific connection logic
            Console.WriteLine("Initializing SQL Server connection pool...");
            await Task.Delay(50);
            return true;
        }

        protected override string GetDatabaseType() => "SQL Server";

        public override async Task<T> ExecuteQueryAsync<T>(string query)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected to database");

            Console.WriteLine($"Executing SQL Server query: {query}");
            await Task.Delay(10);

            // Simulate returning data
            return default(T);
        }

        protected override async Task OnConnectionEstablished()
        {
            await base.OnConnectionEstablished();
            Console.WriteLine("SQL Server specific initialization completed");
        }
    }

    public class PostgreSQLConnection : DatabaseConnection
    {
        public PostgreSQLConnection(string connectionString) : base(connectionString) { }

        protected override async Task<bool> EstablishConnectionAsync()
        {
            // PostgreSQL specific connection logic
            Console.WriteLine("Setting up PostgreSQL connection parameters...");
            await Task.Delay(75);
            return true;
        }

        protected override string GetDatabaseType() => "PostgreSQL";

        public override async Task<T> ExecuteQueryAsync<T>(string query)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected to database");

            Console.WriteLine($"Executing PostgreSQL query: {query}");
            await Task.Delay(15);

            return default(T);
        }

        protected override async Task OnConnectionEstablished()
        {
            await base.OnConnectionEstablished();
            Console.WriteLine("PostgreSQL extensions loaded");
        }
    }

    // =============================================================================
    // 4. INTERFACE IMPLEMENTATIONS - Complete abstraction
    // =============================================================================

    public class EmailNotificationService : INotificationService
    {
        private readonly string _smtpServer;

        public EmailNotificationService(string smtpServer)
        {
            _smtpServer = smtpServer;
        }

        public bool IsAvailable => !string.IsNullOrEmpty(_smtpServer);

        public async Task SendAsync(string recipient, string message)
        {
            Console.WriteLine($"Sending email to {recipient} via {_smtpServer}");
            Console.WriteLine($"Message: {message}");

            // Simulate email sending
            await Task.Delay(100);
            Console.WriteLine("Email sent successfully");
        }
    }

    public class SmsNotificationService : INotificationService
    {
        private readonly string _apiKey;

        public SmsNotificationService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public bool IsAvailable => !string.IsNullOrEmpty(_apiKey);

        public async Task SendAsync(string recipient, string message)
        {
            Console.WriteLine($"Sending SMS to {recipient}");
            Console.WriteLine($"Message: {message}");

            // Simulate SMS API call
            await Task.Delay(50);
            Console.WriteLine("SMS sent successfully");
        }
    }

    public class PayPalProcessor : IPaymentProcessor
    {
        private readonly string _apiKey;

        public PayPalProcessor(string apiKey)
        {
            _apiKey = apiKey;
        }

        public bool SupportsRefunds => true;

        public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, string currency)
        {
            Console.WriteLine($"Processing PayPal payment: {amount} {currency}");

            // Simulate PayPal API call
            await Task.Delay(200);

            return new PaymentResult
            {
                IsSuccessful = true,
                TransactionId = $"PP_{Guid.NewGuid():N}",
                ProcessedAmount = amount,
                Currency = currency
            };
        }
    }

    public class StripeProcessor : IPaymentProcessor
    {
        private readonly string _secretKey;

        public StripeProcessor(string secretKey)
        {
            _secretKey = secretKey;
        }

        public bool SupportsRefunds => true;

        public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, string currency)
        {
            Console.WriteLine($"Processing Stripe payment: {amount} {currency}");

            // Simulate Stripe API call
            await Task.Delay(150);

            return new PaymentResult
            {
                IsSuccessful = true,
                TransactionId = $"ST_{Guid.NewGuid():N}",
                ProcessedAmount = amount,
                Currency = currency
            };
        }
    }

    // =============================================================================
    // 5. ABSTRACTION THROUGH FACADE PATTERN
    // =============================================================================

    /// <summary>
    /// OrderService demonstrates abstraction by hiding complex business logic
    /// behind a simple, clean interface. Client doesn't need to know about
    /// payment processing, notifications, or database operations.
    /// </summary>
    public class OrderService
    {
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly INotificationService _notificationService;
        private readonly DatabaseConnection _database;

        public OrderService(
            IPaymentProcessor paymentProcessor,
            INotificationService notificationService,
            DatabaseConnection database)
        {
            _paymentProcessor = paymentProcessor;
            _notificationService = notificationService;
            _database = database;
        }

        /// <summary>
        /// Simple interface hides complex orchestration
        /// This is abstraction at the service level
        /// </summary>
        public async Task<OrderResult> ProcessOrderAsync(Order order)
        {
            try
            {
                // Step 1: Validate order (abstracted)
                if (!ValidateOrder(order))
                {
                    return OrderResult.Failed("Invalid order data");
                }

                // Step 2: Connect to database (implementation hidden)
                if (!await _database.ConnectAsync())
                {
                    return OrderResult.Failed("Database connection failed");
                }

                // Step 3: Process payment (implementation abstracted)
                var paymentResult = await _paymentProcessor.ProcessPaymentAsync(
                    order.TotalAmount, order.Currency);

                if (!paymentResult.IsSuccessful)
                {
                    return OrderResult.Failed("Payment processing failed");
                }

                // Step 4: Save to database (implementation hidden)
                await SaveOrderToDatabase(order, paymentResult.TransactionId);

                // Step 5: Send confirmation (implementation abstracted)
                await SendOrderConfirmation(order, paymentResult.TransactionId);

                return OrderResult.Success(paymentResult.TransactionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Order processing failed: {ex.Message}");
                return OrderResult.Failed($"Unexpected error: {ex.Message}");
            }
            finally
            {
                _database.Disconnect();
            }
        }

        // Private methods abstract away implementation details
        private bool ValidateOrder(Order order)
        {
            return order != null &&
                   !string.IsNullOrEmpty(order.CustomerEmail) &&
                   order.TotalAmount > 0 &&
                   !string.IsNullOrEmpty(order.Currency);
        }

        private async Task SaveOrderToDatabase(Order order, string transactionId)
        {
            var query = $"INSERT INTO Orders (CustomerEmail, Amount, Currency, TransactionId) " +
                       $"VALUES ('{order.CustomerEmail}', {order.TotalAmount}, '{order.Currency}', '{transactionId}')";

            await _database.ExecuteQueryAsync<object>(query);
            Console.WriteLine("Order saved to database");
        }

        private async Task SendOrderConfirmation(Order order, string transactionId)
        {
            if (_notificationService.IsAvailable)
            {
                var message = $"Your order has been confirmed. Transaction ID: {transactionId}. " +
                             $"Amount: {order.TotalAmount} {order.Currency}";

                await _notificationService.SendAsync(order.CustomerEmail, message);
            }
        }
    }

    // =============================================================================
    // 6. ABSTRACTION THROUGH GENERIC TYPES
    // =============================================================================

    /// <summary>
    /// Generic repository abstracts data access patterns
    /// Client doesn't need to know about specific storage implementation
    /// </summary>
    public abstract class Repository<T> where T : class
    {
        protected abstract Task<T> GetByIdAsync(int id);
        protected abstract Task<IEnumerable<T>> GetAllAsync();
        protected abstract Task<T> AddAsync(T entity);
        protected abstract Task<T> UpdateAsync(T entity);
        protected abstract Task<bool> DeleteAsync(int id);

        // Template method pattern - defines the algorithm structure
        public async Task<T> SaveAsync(T entity)
        {
            Console.WriteLine($"Preparing to save {typeof(T).Name}");

            ValidateEntity(entity);

            var savedEntity = await AddAsync(entity);

            await OnEntitySaved(savedEntity);

            Console.WriteLine($"{typeof(T).Name} saved successfully");
            return savedEntity;
        }

        protected virtual void ValidateEntity(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
        }

        protected virtual Task OnEntitySaved(T entity)
        {
            // Override in derived classes for specific post-save logic
            return Task.CompletedTask;
        }
    }

    // =============================================================================
    // 7. SUPPORTING CLASSES AND MODELS
    // =============================================================================

    public class Order
    {
        public string CustomerEmail { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }

    public class OrderItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class PaymentResult
    {
        public bool IsSuccessful { get; set; }
        public string TransactionId { get; set; }
        public decimal ProcessedAmount { get; set; }
        public string Currency { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class OrderResult
    {
        public bool IsSuccessful { get; private set; }
        public string TransactionId { get; private set; }
        public string ErrorMessage { get; private set; }

        private OrderResult() { }

        public static OrderResult Success(string transactionId)
        {
            return new OrderResult
            {
                IsSuccessful = true,
                TransactionId = transactionId
            };
        }

        public static OrderResult Failed(string errorMessage)
        {
            return new OrderResult
            {
                IsSuccessful = false,
                ErrorMessage = errorMessage
            };
        }
    }

    // =============================================================================
    // 8. DEMONSTRATION PROGRAM
    // =============================================================================

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== ABSTRACTION DEMONSTRATION ===\n");

            // 1. Demonstrate Abstract Class Abstraction
            await DemonstrateAbstractClasses();

            Console.WriteLine("\n" + new string('=', 60) + "\n");

            // 2. Demonstrate Interface Abstraction
            await DemonstrateInterfaceAbstraction();

            Console.WriteLine("\n" + new string('=', 60) + "\n");

            // 3. Demonstrate Service-Level Abstraction
            await DemonstrateServiceAbstraction();
        }

        private static async Task DemonstrateAbstractClasses()
        {
            Console.WriteLine("1. ABSTRACT CLASS ABSTRACTION");
            Console.WriteLine("Hiding database-specific implementation details\n");

            var databases = new List<DatabaseConnection>
            {
                new SqlServerConnection("Server=localhost;Database=TestDB"),
                new PostgreSQLConnection("Host=localhost;Database=testdb;Username=user")
            };

            foreach (var db in databases)
            {
                // Client code doesn't know which specific database type it's using
                // The abstraction hides implementation details
                await db.ConnectAsync();
                await db.ExecuteQueryAsync<string>("SELECT * FROM Users");
                db.Disconnect();
                Console.WriteLine();
            }
        }

        private static async Task DemonstrateInterfaceAbstraction()
        {
            Console.WriteLine("2. INTERFACE ABSTRACTION");
            Console.WriteLine("Hiding notification implementation details\n");

            var notifications = new List<INotificationService>
            {
                new EmailNotificationService("smtp.company.com"),
                new SmsNotificationService("api-key-12345")
            };

            foreach (var service in notifications)
            {
                // Client doesn't know if it's email, SMS, or push notification
                // The interface abstracts away the implementation
                if (service.IsAvailable)
                {
                    await service.SendAsync("user@example.com", "Your order is ready!");
                }
                Console.WriteLine();
            }
        }

        private static async Task DemonstrateServiceAbstraction()
        {
            Console.WriteLine("3. SERVICE-LEVEL ABSTRACTION");
            Console.WriteLine("Hiding complex business process behind simple interface\n");

            // Setup dependencies
            var paymentProcessor = new PayPalProcessor("paypal-api-key");
            var notificationService = new EmailNotificationService("smtp.company.com");
            var database = new SqlServerConnection("Server=localhost;Database=Orders");

            var orderService = new OrderService(paymentProcessor, notificationService, database);

            // Create sample order
            var order = new Order
            {
                CustomerEmail = "customer@example.com",
                TotalAmount = 99.99m,
                Currency = "USD",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Laptop", Quantity = 1, Price = 99.99m }
                }
            };

            // Client calls one simple method
            // All complexity is hidden behind the abstraction
            var result = await orderService.ProcessOrderAsync(order);

            if (result.IsSuccessful)
            {
                Console.WriteLine($"\n✅ Order processed successfully!");
                Console.WriteLine($"Transaction ID: {result.TransactionId}");
            }
            else
            {
                Console.WriteLine($"\n❌ Order failed: {result.ErrorMessage}");
            }
        }
    }
}
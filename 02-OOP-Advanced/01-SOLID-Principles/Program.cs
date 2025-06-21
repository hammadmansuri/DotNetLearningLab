//The SOLID Principles Summary
//S - Single Responsibility Principle (SRP)

//One class, one job - UserValidator only validates, EmailService only sends emails
//Easy to test - Mock one responsibility at a time
//Easy to change - Modify validation logic without affecting email sending

//O - Open/Closed Principle (OCP)

//Extend without modifying - Add new shapes without changing AreaCalculator
//Strategy pattern - Add new discount strategies without touching existing code
//Polymorphism is key - Abstract base classes and interfaces enable extension

//L - Liskov Substitution Principle (LSP)

//Subtypes behave correctly - Square2D and Rectangle2D both work as Shape2D
//No surprising side effects - Methods behave as expected in all implementations
//Consistent contracts - Base class promises are kept by derived classes

//I - Interface Segregation Principle (ISP)

//Small, focused interfaces - IPrinter, IScanner, ICopier instead of one huge interface
//Implement only what you need - BasicPrinter doesn't fake scanner functionality
//Composition over fat interfaces - Combine small interfaces as needed

//D - Dependency Inversion Principle (DIP)

//Depend on abstractions - OrderService uses IPaymentProcessor, not StripeProcessor
//Injection over construction - Dependencies passed in, not created internally
//Easy to swap implementations - Change from Stripe to PayPal without code changes

//Real - World Impact for Senior Engineers
//Maintainability: Changes are isolated and predictable
//Testability: Each component can be tested independently with mocks
//Flexibility: Swap implementations based on configuration or environment
//Team Collaboration: Clear contracts between different parts of the system
//Code Reviews: SOLID violations are easy to spot and fix

//Common SOLID Violations in Enterprise Code
//SRP Violations: "God classes" that handle everything
//OCP Violations: Giant if/else or switch statements for types
//LSP Violations: Derived classes that break base class assumptions
//ISP Violations: Forcing classes to implement methods they don't need
//DIP Violations: new operators scattered throughout high-level code

namespace SOLIDPrinciples
{
    // =============================================================================
    // S - SINGLE RESPONSIBILITY PRINCIPLE (SRP)
    // =============================================================================
    // "A class should have only one reason to change"
    // Each class should have only one job or responsibility

    /// <summary>
    /// ❌ BAD EXAMPLE: Violates SRP - Multiple responsibilities
    /// This class handles user data, validation, email sending, and database operations
    /// </summary>
    public class BadUserManager
    {
        public bool CreateUser(string name, string email, string password)
        {
            // Responsibility 1: Validation
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                return false;

            // Responsibility 2: Password hashing
            var hashedPassword = HashPassword(password);

            // Responsibility 3: Database operations
            var sql = $"INSERT INTO Users (Name, Email, Password) VALUES ('{name}', '{email}', '{hashedPassword}')";
            ExecuteSQL(sql);

            // Responsibility 4: Email sending
            SendWelcomeEmail(email, name);

            // Responsibility 5: Logging
            LogUserCreation(name, email);

            return true;
        }

        private string HashPassword(string password) => $"hashed_{password}";
        private void ExecuteSQL(string sql) => Console.WriteLine($"Executing: {sql}");
        private void SendWelcomeEmail(string email, string name) => Console.WriteLine($"Sending welcome email to {email}");
        private void LogUserCreation(string name, string email) => Console.WriteLine($"User created: {name} ({email})");
    }

    /// <summary>
    /// ✅ GOOD EXAMPLE: Follows SRP - Each class has one responsibility
    /// </summary>

    // Single responsibility: User data representation
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Single responsibility: User validation
    public class UserValidator
    {
        public ValidationResult Validate(string name, string email, string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(name))
                errors.Add("Name is required");

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                errors.Add("Valid email is required");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                errors.Add("Password must be at least 6 characters");

            return new ValidationResult { IsValid = !errors.Any(), Errors = errors };
        }
    }

    // Single responsibility: Password operations
    public class PasswordService
    {
        public string HashPassword(string password)
        {
            // In real world, use BCrypt or similar
            return $"hashed_{password}_{DateTime.Now.Ticks}";
        }

        public bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }

    // Single responsibility: Database operations for users
    public class UserRepository
    {
        public async Task<User> CreateAsync(User user)
        {
            Console.WriteLine($"Saving user to database: {user.Name}");
            user.Id = new Random().Next(1000, 9999);
            user.CreatedAt = DateTime.UtcNow;
            await Task.Delay(50); // Simulate database operation
            return user;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            Console.WriteLine($"Finding user by email: {email}");
            await Task.Delay(30);
            return new User { Id = 1, Name = "John Doe", Email = email };
        }
    }

    // Single responsibility: Email operations
    public class EmailService
    {
        public async Task SendWelcomeEmailAsync(string email, string name)
        {
            Console.WriteLine($"Sending welcome email to {name} at {email}");
            await Task.Delay(100); // Simulate email sending
        }
    }

    // Single responsibility: Logging operations
    public class Logger
    {
        public void LogUserCreated(User user)
        {
            Console.WriteLine($"[LOG] User created: {user.Name} (ID: {user.Id}) at {DateTime.Now}");
        }

        public void LogError(string message, Exception ex = null)
        {
            Console.WriteLine($"[ERROR] {message} {ex?.Message}");
        }
    }

    // Single responsibility: Orchestrating user creation process
    public class UserService
    {
        private readonly UserValidator _validator;
        private readonly PasswordService _passwordService;
        private readonly UserRepository _userRepository;
        private readonly EmailService _emailService;
        private readonly Logger _logger;

        public UserService(UserValidator validator, PasswordService passwordService,
                          UserRepository userRepository, EmailService emailService, Logger logger)
        {
            _validator = validator;
            _passwordService = passwordService;
            _userRepository = userRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<UserCreationResult> CreateUserAsync(string name, string email, string password)
        {
            try
            {
                // Validate input
                var validationResult = _validator.Validate(name, email, password);
                if (!validationResult.IsValid)
                {
                    return UserCreationResult.Failed(validationResult.Errors);
                }

                // Create user
                var user = new User
                {
                    Name = name,
                    Email = email,
                    PasswordHash = _passwordService.HashPassword(password)
                };

                // Save to database
                var savedUser = await _userRepository.CreateAsync(user);

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(savedUser.Email, savedUser.Name);

                // Log success
                _logger.LogUserCreated(savedUser);

                return UserCreationResult.Success(savedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create user", ex);
                return UserCreationResult.Failed(new[] { "An unexpected error occurred" });
            }
        }
    }

    // =============================================================================
    // O - OPEN/CLOSED PRINCIPLE (OCP)
    // =============================================================================
    // "Classes should be open for extension but closed for modification"
    // You should be able to add new functionality without changing existing code

    /// <summary>
    /// ❌ BAD EXAMPLE: Violates OCP - Must modify class to add new shapes
    /// </summary>
    public class BadAreaCalculator
    {
        public double CalculateArea(object shape)
        {
            // Every time we add a new shape, we need to modify this method
            if (shape is Rectangle rectangle)
            {
                return rectangle.Width * rectangle.Height;
            }
            else if (shape is Circle circle)
            {
                return Math.PI * circle.Radius * circle.Radius;
            }
            // Adding Triangle requires modifying this method ❌
            else if (shape is Triangle triangle)
            {
                return 0.5 * triangle.Base * triangle.Height;
            }

            throw new ArgumentException("Unknown shape type");
        }
    }

    /// <summary>
    /// ✅ GOOD EXAMPLE: Follows OCP - Open for extension, closed for modification
    /// </summary>

    // Abstract base that defines the contract
    public abstract class Shape
    {
        public abstract double CalculateArea();
        public abstract string GetShapeInfo();
    }

    // Concrete implementations
    public class Rectangle : Shape
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public Rectangle(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public override double CalculateArea()
        {
            return Width * Height;
        }

        public override string GetShapeInfo()
        {
            return $"Rectangle: {Width} x {Height}";
        }
    }

    public class Circle : Shape
    {
        public double Radius { get; set; }

        public Circle(double radius)
        {
            Radius = radius;
        }

        public override double CalculateArea()
        {
            return Math.PI * Radius * Radius;
        }

        public override string GetShapeInfo()
        {
            return $"Circle: radius {Radius}";
        }
    }

    // New shape can be added WITHOUT modifying existing code ✅
    public class Triangle : Shape
    {
        public double Base { get; set; }
        public double Height { get; set; }

        public Triangle(double baseLength, double height)
        {
            Base = baseLength;
            Height = height;
        }

        public override double CalculateArea()
        {
            return 0.5 * Base * Height;
        }

        public override string GetShapeInfo()
        {
            return $"Triangle: base {Base}, height {Height}";
        }
    }

    // Another new shape - still no modification needed ✅
    public class Hexagon : Shape
    {
        public double SideLength { get; set; }

        public Hexagon(double sideLength)
        {
            SideLength = sideLength;
        }

        public override double CalculateArea()
        {
            return (3 * Math.Sqrt(3) / 2) * SideLength * SideLength;
        }

        public override string GetShapeInfo()
        {
            return $"Hexagon: side length {SideLength}";
        }
    }

    // Calculator never needs to change when new shapes are added
    public class AreaCalculator
    {
        public double CalculateArea(Shape shape)
        {
            return shape.CalculateArea();
        }

        public void PrintShapeInfo(Shape shape)
        {
            Console.WriteLine($"{shape.GetShapeInfo()} - Area: {shape.CalculateArea():F2}");
        }
    }

    // Real-world example: Discount system following OCP
    public abstract class DiscountStrategy
    {
        public abstract decimal CalculateDiscount(decimal amount, Customer customer);
        public abstract string GetDescription();
    }

    public class RegularCustomerDiscount : DiscountStrategy
    {
        public override decimal CalculateDiscount(decimal amount, Customer customer)
        {
            return amount * 0.05m; // 5% discount
        }

        public override string GetDescription() => "Regular Customer 5% Discount";
    }

    public class VipCustomerDiscount : DiscountStrategy
    {
        public override decimal CalculateDiscount(decimal amount, Customer customer)
        {
            return amount * 0.15m; // 15% discount
        }

        public override string GetDescription() => "VIP Customer 15% Discount";
    }

    // New discount can be added without modifying existing code
    public class SeasonalDiscount : DiscountStrategy
    {
        private readonly decimal _discountPercentage;

        public SeasonalDiscount(decimal discountPercentage)
        {
            _discountPercentage = discountPercentage;
        }

        public override decimal CalculateDiscount(decimal amount, Customer customer)
        {
            return amount * (_discountPercentage / 100);
        }

        public override string GetDescription() => $"Seasonal {_discountPercentage}% Discount";
    }

    // =============================================================================
    // L - LISKOV SUBSTITUTION PRINCIPLE (LSP)
    // =============================================================================
    // "Objects of a superclass should be replaceable with objects of its subclasses
    // without breaking the application"

    /// <summary>
    /// ❌ BAD EXAMPLE: Violates LSP - Square changes Rectangle behavior unexpectedly
    /// </summary>
    public class BadRectangle
    {
        public virtual double Width { get; set; }
        public virtual double Height { get; set; }

        public double GetArea() => Width * Height;
    }

    public class BadSquare : BadRectangle
    {
        public override double Width
        {
            get => base.Width;
            set
            {
                base.Width = value;
                base.Height = value; // ❌ Violates LSP - unexpected side effect
            }
        }

        public override double Height
        {
            get => base.Height;
            set
            {
                base.Width = value; // ❌ Violates LSP - unexpected side effect
                base.Height = value;
            }
        }


        // This code works with Rectangle but breaks with Square
        public static void DemonstrateLSPViolation()
        {
            BadRectangle rect = new BadSquare(); // Should work according to LSP
            rect.Width = 5;
            rect.Height = 10;
            // Expected area: 50, but Square will give 100 because setting Height changed Width too
            Console.WriteLine($"Area: {rect.GetArea()}"); // Unexpected behavior!
        }
    }

    /// <summary>
    /// ✅ GOOD EXAMPLE: Follows LSP - Proper abstraction and inheritance
    /// </summary>

    // Abstract base class that defines common behavior
    public abstract class Shape2D
    {
        public abstract double GetArea();
        public abstract double GetPerimeter();
        public abstract string GetDescription();
    }

    public class Rectangle2D : Shape2D
    {
        public double Width { get; }
        public double Height { get; }

        public Rectangle2D(double width, double height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Width and height must be positive");

            Width = width;
            Height = height;
        }

        public override double GetArea() => Width * Height;
        public override double GetPerimeter() => 2 * (Width + Height);
        public override string GetDescription() => $"Rectangle: {Width} x {Height}";
    }

    public class Square2D : Shape2D
    {
        public double SideLength { get; }

        public Square2D(double sideLength)
        {
            if (sideLength <= 0)
                throw new ArgumentException("Side length must be positive");

            SideLength = sideLength;
        }

        public override double GetArea() => SideLength * SideLength;
        public override double GetPerimeter() => 4 * SideLength;
        public override string GetDescription() => $"Square: {SideLength} x {SideLength}";
    }

    // Now any Shape2D can be substituted without issues
    public class ShapeProcessor
    {
        public void ProcessShapes(List<Shape2D> shapes)
        {
            foreach (var shape in shapes)
            {
                // This works correctly for ALL Shape2D implementations
                Console.WriteLine($"{shape.GetDescription()}");
                Console.WriteLine($"  Area: {shape.GetArea():F2}");
                Console.WriteLine($"  Perimeter: {shape.GetPerimeter():F2}");
            }
        }
    }

    // Real-world example: File processors following LSP
    public abstract class FileProcessor
    {
        public abstract Task<ProcessingResult> ProcessAsync(string filePath);
        public abstract bool CanProcess(string fileExtension);
        public abstract string GetProcessorName();

        // Template method that works for all implementations
        public async Task<ProcessingResult> ProcessFileAsync(string filePath)
        {
            var extension = Path.GetExtension(filePath);

            if (!CanProcess(extension))
            {
                return ProcessingResult.Failed($"Cannot process {extension} files");
            }

            Console.WriteLine($"Processing {filePath} with {GetProcessorName()}");
            return await ProcessAsync(filePath);
        }
    }

    public class PdfProcessor : FileProcessor
    {
        public override async Task<ProcessingResult> ProcessAsync(string filePath)
        {
            await Task.Delay(100); // Simulate PDF processing
            return ProcessingResult.Success($"PDF processed: {Path.GetFileName(filePath)}");
        }

        public override bool CanProcess(string fileExtension)
        {
            return fileExtension.ToLower() == ".pdf";
        }

        public override string GetProcessorName() => "PDF Processor";
    }

    public class ImageProcessor : FileProcessor
    {
        public override async Task<ProcessingResult> ProcessAsync(string filePath)
        {
            await Task.Delay(150); // Simulate image processing
            return ProcessingResult.Success($"Image processed: {Path.GetFileName(filePath)}");
        }

        public override bool CanProcess(string fileExtension)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            return imageExtensions.Contains(fileExtension.ToLower());
        }

        public override string GetProcessorName() => "Image Processor";
    }

    // =============================================================================
    // I - INTERFACE SEGREGATION PRINCIPLE (ISP)
    // =============================================================================
    // "A client should never be forced to implement an interface that it doesn't use"
    // Many small, specific interfaces are better than one large interface

    /// <summary>
    /// ❌ BAD EXAMPLE: Violates ISP - Fat interface forces unnecessary implementations
    /// </summary>
    public interface IBadPrinter
    {
        void Print(string document);
        void Scan(string document);
        void Fax(string document);
        void Copy(string document);
        void PrintInColor(string document);
        void PrintDoubleSided(string document);
    }

    // Simple printer is forced to implement features it doesn't have ❌
    public class SimplePrinter : IBadPrinter
    {
        public void Print(string document)
        {
            Console.WriteLine($"Printing: {document}");
        }

        public void Scan(string document)
        {
            throw new NotSupportedException("This printer cannot scan"); // ❌ Forced to implement
        }

        public void Fax(string document)
        {
            throw new NotSupportedException("This printer cannot fax"); // ❌ Forced to implement
        }

        public void Copy(string document)
        {
            throw new NotSupportedException("This printer cannot copy"); // ❌ Forced to implement
        }

        public void PrintInColor(string document)
        {
            throw new NotSupportedException("This printer cannot print in color"); // ❌ Forced to implement
        }

        public void PrintDoubleSided(string document)
        {
            throw new NotSupportedException("This printer cannot print double-sided"); // ❌ Forced to implement
        }
    }

    /// <summary>
    /// ✅ GOOD EXAMPLE: Follows ISP - Small, focused interfaces
    /// </summary>

    // Basic printing capability
    public interface IPrinter
    {
        void Print(string document);
    }

    // Optional scanning capability
    public interface IScanner
    {
        string Scan();
    }

    // Optional faxing capability
    public interface IFax
    {
        void SendFax(string document, string phoneNumber);
    }

    // Optional copying capability
    public interface ICopier
    {
        void Copy(string document, int copies);
    }

    // Optional color printing capability
    public interface IColorPrinter
    {
        void PrintInColor(string document);
    }

    // Optional double-sided printing capability
    public interface IDuplexPrinter
    {
        void PrintDoubleSided(string document);
    }

    // Now each device implements only what it can do ✅
    public class BasicPrinter : IPrinter
    {
        public void Print(string document)
        {
            Console.WriteLine($"Basic printer: Printing {document}");
        }
    }

    public class ColorPrinter : IPrinter, IColorPrinter
    {
        public void Print(string document)
        {
            Console.WriteLine($"Color printer: Printing {document} in black and white");
        }

        public void PrintInColor(string document)
        {
            Console.WriteLine($"Color printer: Printing {document} in color");
        }
    }

    public class MultiFunctionPrinter : IPrinter, IScanner, ICopier, IColorPrinter, IDuplexPrinter
    {
        public void Print(string document)
        {
            Console.WriteLine($"MFP: Printing {document}");
        }

        public string Scan()
        {
            Console.WriteLine("MFP: Scanning document");
            return "scanned_document.pdf";
        }

        public void Copy(string document, int copies)
        {
            Console.WriteLine($"MFP: Making {copies} copies of {document}");
        }

        public void PrintInColor(string document)
        {
            Console.WriteLine($"MFP: Printing {document} in color");
        }

        public void PrintDoubleSided(string document)
        {
            Console.WriteLine($"MFP: Printing {document} double-sided");
        }
    }

    // Clients can depend only on the interfaces they need
    public class PrintingService
    {
        public void PrintDocument(IPrinter printer, string document)
        {
            printer.Print(document);

            // Optional features - only used if available
            if (printer is IColorPrinter colorPrinter)
            {
                colorPrinter.PrintInColor(document);
            }

            if (printer is IDuplexPrinter duplexPrinter)
            {
                duplexPrinter.PrintDoubleSided(document);
            }
        }
    }

    // =============================================================================
    // D - DEPENDENCY INVERSION PRINCIPLE (DIP)
    // =============================================================================
    // "High-level modules should not depend on low-level modules. Both should depend on abstractions."
    // "Abstractions should not depend on details. Details should depend on abstractions."

    /// <summary>
    /// ❌ BAD EXAMPLE: Violates DIP - High-level class depends on concrete implementations
    /// </summary>
    public class EmailNotification
    {
        public void SendEmail(string message)
        {
            Console.WriteLine($"Sending email: {message}");
        }
    }

    public class SmsNotification
    {
        public void SendSms(string message)
        {
            Console.WriteLine($"Sending SMS: {message}");
        }
    }

    // High-level class depends on concrete implementations ❌
    public class BadOrderService
    {
        private readonly EmailNotification _emailService; // ❌ Concrete dependency
        private readonly SmsNotification _smsService;     // ❌ Concrete dependency

        public BadOrderService()
        {
            _emailService = new EmailNotification(); // ❌ Hard dependency
            _smsService = new SmsNotification();     // ❌ Hard dependency
        }

        public void ProcessOrder(Order order)
        {
            Console.WriteLine($"Processing order {order.Id}");

            // Tightly coupled to specific notification types
            _emailService.SendEmail($"Order {order.Id} processed");
            _smsService.SendSms($"Order {order.Id} processed");
        }
    }

    /// <summary>
    /// ✅ GOOD EXAMPLE: Follows DIP - Depends on abstractions, not concretions
    /// </summary>

    // Abstraction that both high-level and low-level modules depend on
    public interface INotificationService
    {
        Task SendNotificationAsync(string message, string recipient);
        string GetServiceName();
    }

    // Low-level modules depend on abstraction ✅
    public class EmailNotificationService : INotificationService
    {
        public async Task SendNotificationAsync(string message, string recipient)
        {
            Console.WriteLine($"Sending email to {recipient}: {message}");
            await Task.Delay(100); // Simulate email sending
        }

        public string GetServiceName() => "Email Service";
    }

    public class SmsNotificationService : INotificationService
    {
        public async Task SendNotificationAsync(string message, string recipient)
        {
            Console.WriteLine($"Sending SMS to {recipient}: {message}");
            await Task.Delay(50); // Simulate SMS sending
        }

        public string GetServiceName() => "SMS Service";
    }

    public class PushNotificationService : INotificationService
    {
        public async Task SendNotificationAsync(string message, string recipient)
        {
            Console.WriteLine($"Sending push notification to {recipient}: {message}");
            await Task.Delay(25); // Simulate push notification
        }

        public string GetServiceName() => "Push Notification Service";
    }

    // Repository abstraction
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(int orderId);
        Task<Order> SaveAsync(Order order);
        Task<List<Order>> GetOrdersByCustomerAsync(int customerId);
    }

    // Payment processing abstraction
    public interface IPaymentProcessor
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        string GetProcessorName();
    }

    // High-level module depends on abstractions ✅
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IEnumerable<INotificationService> _notificationServices;
        private readonly Logger _logger;

        // Dependencies injected through constructor ✅
        public OrderService(
            IOrderRepository orderRepository,
            IPaymentProcessor paymentProcessor,
            IEnumerable<INotificationService> notificationServices,
            Logger logger)
        {
            _orderRepository = orderRepository;
            _paymentProcessor = paymentProcessor;
            _notificationServices = notificationServices;
            _logger = logger;
        }

        public async Task<OrderResult> ProcessOrderAsync(int orderId)
        {
            try
            {
                // Get order
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return OrderResult.Failed("Order not found");
                }

                // Process payment
                var paymentRequest = new PaymentRequest
                {
                    Amount = order.TotalAmount,
                    Currency = "USD",
                    PaymentMethod = order.PaymentMethod
                };

                var paymentResult = await _paymentProcessor.ProcessPaymentAsync(paymentRequest);
                if (!paymentResult.IsSuccessful)
                {
                    return OrderResult.Failed($"Payment failed: {paymentResult.ErrorMessage}");
                }

                // Update order
                order.Status = OrderStatus.Processed;
                order.TransactionId = paymentResult.TransactionId;
                await _orderRepository.SaveAsync(order);

                // Send notifications through all configured services
                var notificationTasks = _notificationServices.Select(service =>
                    SendNotificationSafely(service, $"Order {order.Id} has been processed successfully!", order.CustomerEmail)
                );

                await Task.WhenAll(notificationTasks);

                _logger.LogUserCreated(new User { Id = order.CustomerId, Name = "Customer", Email = order.CustomerEmail });

                return OrderResult.Success(order.Id, paymentResult.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to process order {orderId}", ex);
                return OrderResult.Failed("An unexpected error occurred");
            }
        }

        private async Task SendNotificationSafely(INotificationService service, string message, string recipient)
        {
            try
            {
                await service.SendNotificationAsync(message, recipient);
                Console.WriteLine($"✅ Notification sent via {service.GetServiceName()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send notification via {service.GetServiceName()}: {ex.Message}");
            }
        }
    }

    // Concrete implementations can be easily swapped
    public class StripePaymentProcessor : IPaymentProcessor
    {
        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            Console.WriteLine($"Processing payment via Stripe: ${request.Amount}");
            await Task.Delay(200);
            return PaymentResult.Success($"stripe_{Guid.NewGuid():N}", request.Amount, request.Currency);
        }

        public string GetProcessorName() => "Stripe";
    }

    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders = new();
        private int _nextId = 1;

        public Task<Order> GetByIdAsync(int orderId)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId);
            return Task.FromResult(order);
        }

        public Task<Order> SaveAsync(Order order)
        {
            if (order.Id == 0)
            {
                order.Id = _nextId++;
                _orders.Add(order);
            }
            else
            {
                var existingIndex = _orders.FindIndex(o => o.Id == order.Id);
                if (existingIndex >= 0)
                {
                    _orders[existingIndex] = order;
                }
            }

            return Task.FromResult(order);
        }

        public Task<List<Order>> GetOrdersByCustomerAsync(int customerId)
        {
            var orders = _orders.Where(o => o.CustomerId == customerId).ToList();
            return Task.FromResult(orders);
        }
    }

    // =============================================================================
    // SUPPORTING TYPES AND MODELS
    // =============================================================================

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class UserCreationResult
    {
        public bool IsSuccessful { get; private set; }
        public User User { get; private set; }
        public List<string> Errors { get; private set; }

        public static UserCreationResult Success(User user)
        {
            return new UserCreationResult { IsSuccessful = true, User = user };
        }

        public static UserCreationResult Failed(IEnumerable<string> errors)
        {
            return new UserCreationResult { IsSuccessful = false, Errors = errors.ToList() };
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public CustomerType Type { get; set; }
    }

    public enum CustomerType
    {
        Regular,
        Vip,
        Enterprise
    }

    public class ProcessingResult
    {
        public bool IsSuccessful { get; private set; }
        public string Message { get; private set; }
        public string ErrorMessage { get; private set; }

        public static ProcessingResult Success(string message)
        {
            return new ProcessingResult { IsSuccessful = true, Message = message };
        }

        public static ProcessingResult Failed(string errorMessage)
        {
            return new ProcessingResult { IsSuccessful = false, ErrorMessage = errorMessage };
        }
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class PaymentResult
    {
        public bool IsSuccessful { get; private set; }
        public string TransactionId { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        public string ErrorMessage { get; private set; }

        public static PaymentResult Success(string transactionId, decimal amount, string currency)
        {
            return new PaymentResult
            {
                IsSuccessful = true,
                TransactionId = transactionId,
                Amount = amount,
                Currency = currency
            };
        }

        public static PaymentResult Failed(string errorMessage)
        {
            return new PaymentResult { IsSuccessful = false, ErrorMessage = errorMessage };
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerEmail { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public OrderStatus Status { get; set; }
        public string TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum OrderStatus
    {
        Pending,
        Processed,
        Shipped,
        Delivered,
        Cancelled
    }

    public class OrderResult
    {
        public bool IsSuccessful { get; private set; }
        public int OrderId { get; private set; }
        public string TransactionId { get; private set; }
        public string ErrorMessage { get; private set; }

        public static OrderResult Success(int orderId, string transactionId)
        {
            return new OrderResult
            {
                IsSuccessful = true,
                OrderId = orderId,
                TransactionId = transactionId
            };
        }

        public static OrderResult Failed(string errorMessage)
        {
            return new OrderResult { IsSuccessful = false, ErrorMessage = errorMessage };
        }
    }

    // =============================================================================
    // DEMONSTRATION PROGRAM
    // =============================================================================

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== SOLID PRINCIPLES DEMONSTRATION ===\n");

            DemonstrateSingleResponsibility();
            Console.WriteLine("\n" + new string('=', 80) + "\n");

            DemonstrateOpenClosed();
            Console.WriteLine("\n" + new string('=', 80) + "\n");

            DemonstrateLiskovSubstitution();
            Console.WriteLine("\n" + new string('=', 80) + "\n");

            DemonstrateInterfaceSegregation();
            Console.WriteLine("\n" + new string('=', 80) + "\n");

            await DemonstrateDependencyInversion();
        }

        private static void DemonstrateSingleResponsibility()
        {
            Console.WriteLine("1. SINGLE RESPONSIBILITY PRINCIPLE (SRP)");
            Console.WriteLine("Each class has one reason to change\n");

            // Setup dependencies (in real app, use DI container)
            var validator = new UserValidator();
            var passwordService = new PasswordService();
            var userRepository = new UserRepository();
            var emailService = new EmailService();
            var logger = new Logger();

            var userService = new UserService(validator, passwordService, userRepository, emailService, logger);

            Console.WriteLine("=== Creating a valid user ===");
            var result = userService.CreateUserAsync("John Doe", "john@example.com", "securepassword123").Result;

            if (result.IsSuccessful)
            {
                Console.WriteLine($"✅ User created successfully: {result.User.Name} (ID: {result.User.Id})");
            }
            else
            {
                Console.WriteLine($"❌ User creation failed: {string.Join(", ", result.Errors)}");
            }

            Console.WriteLine("\n=== Attempting to create invalid user ===");
            var invalidResult = userService.CreateUserAsync("", "invalid-email", "123").Result;

            if (!invalidResult.IsSuccessful)
            {
                Console.WriteLine("❌ Validation errors:");
                foreach (var error in invalidResult.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            Console.WriteLine("\n✅ Benefits of SRP:");
            Console.WriteLine("  - Easy to test each component independently");
            Console.WriteLine("  - Changes to validation don't affect email sending");
            Console.WriteLine("  - Can replace any component without affecting others");
        }

        private static void DemonstrateOpenClosed()
        {
            Console.WriteLine("2. OPEN/CLOSED PRINCIPLE (OCP)");
            Console.WriteLine("Open for extension, closed for modification\n");

            var calculator = new AreaCalculator();
            var shapes = new List<Shape>
            {
                new Rectangle(5, 10),
                new Circle(7),
                new Triangle(6, 8),
                new Hexagon(4) // New shape added without modifying existing code!
            };

            Console.WriteLine("=== Calculating areas for different shapes ===");
            foreach (var shape in shapes)
            {
                calculator.PrintShapeInfo(shape);
            }

            Console.WriteLine("\n=== Demonstrating discount strategies ===");
            var customer = new Customer { Id = 1, Name = "John Doe", Type = CustomerType.Vip };
            var discountStrategies = new List<DiscountStrategy>
            {
                new RegularCustomerDiscount(),
                new VipCustomerDiscount(),
                new SeasonalDiscount(20) // New discount strategy added!
            };

            decimal orderAmount = 100m;
            foreach (var strategy in discountStrategies)
            {
                var discount = strategy.CalculateDiscount(orderAmount, customer);
                var finalAmount = orderAmount - discount;
                Console.WriteLine($"{strategy.GetDescription()}: ${discount:F2} off, Final: ${finalAmount:F2}");
            }

            Console.WriteLine("\n✅ Benefits of OCP:");
            Console.WriteLine("  - Add new shapes without modifying AreaCalculator");
            Console.WriteLine("  - Add new discount strategies without changing existing code");
            Console.WriteLine("  - Existing code remains stable and tested");
        }

        private static void DemonstrateLiskovSubstitution()
        {
            Console.WriteLine("3. LISKOV SUBSTITUTION PRINCIPLE (LSP)");
            Console.WriteLine("Subtypes must be substitutable for their base types\n");

            var shapes = new List<Shape2D>
            {
                new Rectangle2D(5, 10),
                new Square2D(7) // Square can be substituted for Shape2D without issues
            };

            var processor = new ShapeProcessor();

            Console.WriteLine("=== Processing shapes (LSP compliant) ===");
            processor.ProcessShapes(shapes);

            Console.WriteLine("\n=== Demonstrating file processors ===");
            var fileProcessors = new List<FileProcessor>
            {
                new PdfProcessor(),
                new ImageProcessor()
            };

            var testFiles = new[] { "document.pdf", "image.jpg", "data.txt" };

            foreach (var processor1 in fileProcessors)
            {
                Console.WriteLine($"\n--- {processor1.GetProcessorName()} ---");
                foreach (var file in testFiles)
                {
                    var result = processor1.ProcessFileAsync(file).Result;
                    if (result.IsSuccessful)
                    {
                        Console.WriteLine($"✅ {result.Message}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ {result.ErrorMessage}");
                    }
                }
            }

            Console.WriteLine("\n✅ Benefits of LSP:");
            Console.WriteLine("  - Any Shape2D can be processed the same way");
            Console.WriteLine("  - FileProcessor subtypes work consistently");
            Console.WriteLine("  - No unexpected behavior when substituting types");
        }

        private static void DemonstrateInterfaceSegregation()
        {
            Console.WriteLine("4. INTERFACE SEGREGATION PRINCIPLE (ISP)");
            Console.WriteLine("Clients shouldn't depend on interfaces they don't use\n");

            var printers = new List<IPrinter>
            {
                new BasicPrinter(),
                new ColorPrinter(),
                new MultiFunctionPrinter()
            };

            var printingService = new PrintingService();
            var testDocument = "important_document.pdf";

            Console.WriteLine("=== Testing different printers ===");
            foreach (var printer in printers)
            {
                Console.WriteLine($"\n--- {printer.GetType().Name} ---");
                printingService.PrintDocument(printer, testDocument);

                // Test optional capabilities
                if (printer is IScanner scanner)
                {
                    var scannedDoc = scanner.Scan();
                    Console.WriteLine($"Scanned: {scannedDoc}");
                }

                if (printer is ICopier copier)
                {
                    copier.Copy(testDocument, 3);
                }
            }

            Console.WriteLine("\n✅ Benefits of ISP:");
            Console.WriteLine("  - BasicPrinter only implements IPrinter (what it can do)");
            Console.WriteLine("  - MultiFunctionPrinter implements all relevant interfaces");
            Console.WriteLine("  - No forced implementation of unsupported features");
            Console.WriteLine("  - Clients depend only on what they need");
        }

        private static async Task DemonstrateDependencyInversion()
        {
            Console.WriteLine("5. DEPENDENCY INVERSION PRINCIPLE (DIP)");
            Console.WriteLine("Depend on abstractions, not concretions\n");

            // Setup dependencies (simulating DI container)
            IOrderRepository orderRepository = new InMemoryOrderRepository();
            IPaymentProcessor paymentProcessor = new StripePaymentProcessor();
            var notificationServices = new List<INotificationService>
            {
                new EmailNotificationService(),
                new SmsNotificationService(),
                new PushNotificationService()
            };
            var logger = new Logger();

            // High-level service depends on abstractions
            var orderService = new OrderService(orderRepository, paymentProcessor, notificationServices, logger);

            // Create a test order
            var order = new Order
            {
                CustomerId = 123,
                CustomerEmail = "customer@example.com",
                TotalAmount = 99.99m,
                PaymentMethod = "credit_card",
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var savedOrder = await orderRepository.SaveAsync(order);
            Console.WriteLine($"Created test order with ID: {savedOrder.Id}");

            Console.WriteLine("\n=== Processing order with dependency injection ===");
            var result = await orderService.ProcessOrderAsync(savedOrder.Id);

            if (result.IsSuccessful)
            {
                Console.WriteLine($"✅ Order {result.OrderId} processed successfully!");
                Console.WriteLine($"Transaction ID: {result.TransactionId}");
            }
            else
            {
                Console.WriteLine($"❌ Order processing failed: {result.ErrorMessage}");
            }

            Console.WriteLine("\n✅ Benefits of DIP:");
            Console.WriteLine("  - OrderService doesn't know about specific implementations");
            Console.WriteLine("  - Can easily swap payment processors (Stripe → PayPal)");
            Console.WriteLine("  - Can add/remove notification services without code changes");
            Console.WriteLine("  - Easy to test with mock implementations");
            Console.WriteLine("  - Configuration determines which implementations to use");
        }
    }
}
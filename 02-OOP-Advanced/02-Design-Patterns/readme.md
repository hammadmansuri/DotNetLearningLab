# Design Patterns - Complete Implementation Guide

> **Enterprise .NET Design Patterns with SOLID Principles Integration**

## 📋 Table of Contents

- [Overview](#overview)
- [Creational Patterns](#creational-patterns)
- [Structural Patterns](#structural-patterns)
- [Behavioral Patterns](#behavioral-patterns)
- [Enterprise Patterns](#enterprise-patterns)
- [SOLID Integration](#solid-integration)
- [When NOT to Use](#when-not-to-use)
- [Real-World Benefits](#real-world-benefits)
- [Common Combinations](#common-combinations)
- [Next Steps](#next-steps)

## 🎯 Overview

This implementation covers the most essential design patterns for enterprise .NET development, showing how they support SOLID principles and solve common programming problems with proven, reusable solutions.

### Patterns Covered

| Category | Patterns | Purpose |
|----------|----------|---------|
| **Creational** | Singleton, Factory, Builder | Object creation with flexibility |
| **Structural** | Repository, Adapter, Decorator | Object composition and relationships |
| **Behavioral** | Strategy, Observer, Command | Object interaction and algorithms |
| **Enterprise** | Unit of Work, Specification | Business complexity management |

## 🏗️ Creational Patterns

### Singleton Pattern
**Purpose**: Ensure a class has only one instance and provide global access to it

```csharp
// ✅ Thread-safe lazy singleton
public sealed class ConfigurationManager
{
    private static readonly Lazy<ConfigurationManager> _instance = 
        new Lazy<ConfigurationManager>(() => new ConfigurationManager());

    public static ConfigurationManager Instance => _instance.Value;
}
```

**Key Benefits:**
- Thread-safe implementation using `Lazy<T>`
- Global access point for shared resources
- Controlled instantiation

**⚠️ Prefer Dependency Injection over Singleton for better testability**

### Factory Pattern
**Purpose**: Create objects without specifying their concrete classes

```csharp
// Interface for products
public interface INotification
{
    Task SendAsync(string message, string recipient);
    string GetNotificationType();
}

// Factory creates appropriate implementation
public class NotificationFactory : INotificationFactory
{
    public INotification CreateNotification(string type) { /* ... */ }
}
```

**Key Benefits:**
- Supports Open/Closed Principle
- Easy to extend with new types
- Centralizes object creation logic

### Builder Pattern
**Purpose**: Construct complex objects step by step

```csharp
// Fluent interface for complex object construction
var email = EmailMessageBuilder
    .Create()
    .To("john@example.com")
    .Subject("Welcome!")
    .Body("<h1>Welcome!</h1>", isHtml: true)
    .HighPriority()
    .Build();
```

**Key Benefits:**
- Readable, fluent API
- Handles complex construction logic
- Validates required parameters

## 🏛️ Structural Patterns

### Repository Pattern
**Purpose**: Encapsulate data access logic and provide a uniform interface

```csharp
// Clean separation of data access from business logic
public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    Task<User> CreateAsync(User user);
    // ... other methods
}

// Business service depends on abstraction
public class UserService
{
    private readonly IUserRepository _userRepository;
    
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
}
```

**Key Benefits:**
- Supports Dependency Inversion Principle
- Easy to test with mock implementations
- Centralizes data access logic

### Adapter Pattern
**Purpose**: Allow incompatible interfaces to work together

```csharp
// Makes legacy system work with modern interface
public class LegacyPaymentAdapter : IPaymentProcessor
{
    private readonly LegacyPaymentSystem _legacySystem;

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        // Adapt modern request to legacy format
        var success = _legacySystem.ProcessCreditCard(
            request.CardNumber, request.ExpiryDate, (double)request.Amount);
        // ...
    }
}
```

**Key Benefits:**
- Integrates legacy systems
- No modification to existing code
- Maintains interface consistency

### Decorator Pattern
**Purpose**: Add new functionality to objects without altering their structure

```csharp
// Flexible composition of features
ICoffee coffee = new SimpleCoffee();
coffee = new MilkDecorator(coffee);
coffee = new SugarDecorator(coffee);
coffee = new WhipDecorator(coffee);

Console.WriteLine($"{coffee.GetDescription()} - ${coffee.GetCost():F2}");
// Output: "Simple Coffee, Milk, Sugar, Whipped Cream - $3.50"
```

**Key Benefits:**
- Supports Open/Closed Principle
- Flexible feature composition
- Runtime behavior modification

## 🎭 Behavioral Patterns

### Strategy Pattern
**Purpose**: Define family of algorithms and make them interchangeable

```csharp
// Different shipping calculation strategies
public interface IShippingStrategy
{
    decimal CalculateShipping(decimal weight, decimal distance);
    string GetMethodName();
}

// Easy to add new strategies without modifying existing code
public class OvernightShippingStrategy : IShippingStrategy
{
    public decimal CalculateShipping(decimal weight, decimal distance)
    {
        return (weight * 5.0m) + (distance * 0.5m) + 10.0m;
    }
}
```

**Key Benefits:**
- Supports Open/Closed Principle
- Runtime algorithm selection
- Eliminates conditional logic

### Observer Pattern
**Purpose**: Define one-to-many dependency between objects

```csharp
// Order notifies multiple observers when status changes
public class Order
{
    private readonly List<IOrderObserver> _observers = new();
    
    public OrderStatus Status
    {
        get => _status;
        set
        {
            var oldStatus = _status;
            _status = value;
            NotifyStatusChanged(oldStatus);
        }
    }
}
```

**Key Benefits:**
- Loose coupling between subjects and observers
- Dynamic subscription/unsubscription
- Supports event-driven architecture

### Command Pattern
**Purpose**: Encapsulate requests as objects, enabling parameterization and queuing

```csharp
// Encapsulate operations for undo/redo and queuing
public class DepositCommand : ICommand
{
    public Task ExecuteAsync() => _account.Deposit(_amount);
    public Task UndoAsync() => _account.Withdraw(_amount);
}

// Transaction manager handles execution and history
var manager = new BankTransactionManager();
await manager.ExecuteCommandAsync(new DepositCommand(account, 100m));
await manager.UndoLastCommandAsync(); // Supports undo!
```

**Key Benefits:**
- Supports undo/redo operations
- Command queuing and logging
- Decouples invoker from receiver

## 🏢 Enterprise Patterns

### Unit of Work Pattern
**Purpose**: Maintain a list of objects affected by a business transaction

```csharp
// Coordinates multiple repository operations
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IOrderRepository Orders { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
}

// Ensures atomicity across multiple operations
using var unitOfWork = new InMemoryUnitOfWork();
await unitOfWork.BeginTransactionAsync();
var user = await unitOfWork.Users.CreateAsync(newUser);
var order = await unitOfWork.Orders.CreateAsync(newOrder);
await unitOfWork.SaveChangesAsync();
await unitOfWork.CommitTransactionAsync();
```

**Key Benefits:**
- Ensures transaction consistency
- Coordinates multiple repositories
- Simplifies error handling

### Specification Pattern
**Purpose**: Encapsulate business rules as reusable and combinable specifications

```csharp
// Composable business rules
var eligibleUserSpec = new ActiveUserSpecification()
    .And(new EmailDomainSpecification("company.com"))
    .And(new RecentUserSpecification(TimeSpan.FromDays(30)));

var eligibleUsers = allUsers.Where(user => eligibleUserSpec.IsSatisfiedBy(user));
```

**Key Benefits:**
- Reusable business logic
- Combinable rules
- Testable specifications

## 🔗 SOLID Integration

### How Design Patterns Support SOLID Principles

| SOLID Principle | Supporting Patterns | How |
|----------------|-------------------|-----|
| **Single Responsibility** | All patterns | Each pattern class has one clear job |
| **Open/Closed** | Factory, Strategy, Decorator | Extend behavior without modification |
| **Liskov Substitution** | Repository, Strategy, Command | All implementations work as base types |
| **Interface Segregation** | All patterns | Small, focused interfaces throughout |
| **Dependency Inversion** | Repository, Factory, Strategy | Depend on abstractions, not concretions |

### Example: Repository + Strategy + Decorator
```csharp
// Multiple SOLID principles working together
IUserRepository repository = new InMemoryUserRepository();
repository = new LoggingRepositoryDecorator(repository, logger);  // Decorator
repository = new CachingRepositoryDecorator(repository, cache);   // Decorator

INotificationStrategy strategy = strategyFactory.Create("email"); // Factory + Strategy
var service = new UserService(repository, strategy);              // Dependency Inversion
```

## ❌ When NOT to Use

### Anti-Patterns to Avoid

| Pattern | Don't Use When | Use Instead |
|---------|---------------|-------------|
| **Singleton** | Almost always | Dependency Injection |
| **Factory** | Simple object creation | Direct instantiation |
| **Observer** | Simple notifications | Direct method calls |
| **Repository** | Simple CRUD operations | Direct data access |
| **Command** | Simple operations | Direct method calls |
| **Decorator** | Single responsibility | Simple inheritance |

### Over-Engineering Warning
```csharp
// ❌ DON'T: Over-engineered for simple scenarios
public interface IStringProcessor { string Process(string input); }
public class StringProcessorFactory { /* ... */ }
public class StringProcessorCommand { /* ... */ }

// ✅ DO: Simple and direct
public static class StringHelper
{
    public static string Process(string input) => input.Trim().ToUpper();
}
```

## 🎯 Real-World Benefits

### Testing Advantages
```csharp
// Easy to test with mocked dependencies
[Test]
public async Task ProcessOrder_ValidOrder_SendsNotification()
{
    // Arrange
    var mockRepo = new Mock<IOrderRepository>();
    var mockNotification = new Mock<INotificationStrategy>();
    var service = new OrderService(mockRepo.Object, mockNotification.Object);
    
    // Act & Assert - Test business logic without dependencies
}
```

### Maintainability Benefits
- **Localized Changes**: Modify one pattern implementation without affecting others
- **Clear Responsibilities**: Each class has a single, well-defined purpose
- **Predictable Structure**: Team members can quickly understand and extend code

### Flexibility Advantages
- **Runtime Configuration**: Switch implementations based on configuration
- **A/B Testing**: Easy to test different algorithms or strategies
- **Feature Toggles**: Enable/disable features without code changes

## 🔧 Common Combinations

### Enterprise Application Stack
```csharp
// Typical enterprise pattern combination
public class OrderManagementService
{
    public OrderManagementService(
        IUnitOfWork unitOfWork,                    // Unit of Work
        INotificationFactory notificationFactory,  // Factory
        IShippingStrategy shippingStrategy,        // Strategy
        ILogger logger)                            // Dependency Injection
    {
        // Repository pattern through Unit of Work
        // Observer pattern for order events
        // Command pattern for order operations
    }
}
```

### CQRS with Patterns
```csharp
// Command Query Responsibility Segregation
public class CreateOrderCommandHandler
{
    public CreateOrderCommandHandler(
        IOrderRepository writeRepository,          // Repository
        ICommandBus commandBus,                    // Command
        IEventPublisher eventPublisher)            // Observer
    {
        // Specification pattern for validation
        // Unit of Work for transaction management
    }
}
```

## 📈 Next Steps

### Recommended Learning Path
1. **Practice Implementation**: Build each pattern from scratch
2. **Combine Patterns**: Create mini-applications using multiple patterns
3. **Study Frameworks**: See how patterns are used in ASP.NET Core, Entity Framework
4. **Architectural Patterns**: Move to Clean Architecture, CQRS, Event Sourcing

### Advanced Topics
- **Dependency Injection Containers**: How patterns work with DI frameworks
- **Async Patterns**: Implementing patterns with async/await
- **Performance Considerations**: When patterns help or hurt performance
- **Testing Strategies**: Unit testing each pattern effectively

### Related Learning Topics
- **03-Composition-vs-Inheritance**: Understanding when to use composition (many patterns use this)
- **04-Advanced-Inheritance**: Deep dive into inheritance hierarchies
- **06-DotNet-Core-Features/02-Dependency-Injection**: How DI containers work with patterns

---

## 📚 Key Takeaways

✅ **Design patterns solve recurring problems with proven solutions**  
✅ **They support SOLID principles and improve code quality**  
✅ **Don't over-engineer - use patterns when they add real value**  
✅ **Combine patterns for powerful, flexible architectures**  
✅ **Focus on testability and maintainability over cleverness**

> **Remember**: Patterns are tools, not goals. Use them to solve real problems, not to show off knowledge.

---

*This guide provides complete, working examples of each pattern with enterprise-focused implementations. Practice with real scenarios to master when and how to apply each pattern effectively.*
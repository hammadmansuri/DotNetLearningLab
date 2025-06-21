//The Key Principles for Effective Abstractions
//1. Hide Implementation Details, Not Just Move Them

//❌ Bad: Exposing SQL queries, connection strings, or SMTP configuration through interfaces
//✅ Good: Using domain-focused methods like GetUserByEmailAsync() or SendWelcomeMessageAsync()

//2. Use Result Types Instead of Exception Propagation

//❌ Bad: Letting FileNotFoundException, SqlException, SmtpException bubble up
//✅ Good: Converting all errors to consistent, abstraction-level error types

//3. Encapsulate Configuration Complexity

//❌ Bad: Making clients pass database connection strings or SMTP settings to every method
//✅ Good: Configure once at construction time, hide all complexity internally

//4. Design Domain-Focused Interfaces

//❌ Bad: ExecuteSqlAsync(string sql, params) - exposes technical implementation
//✅ Good: GetActiveUsersAsync() - speaks in business terms

namespace EffectiveAbstractions
{
    // =============================================================================
    // ❌ BAD EXAMPLES - LEAKY ABSTRACTIONS
    // =============================================================================

    /// <summary>
    /// LEAKY ABSTRACTION #1: Exposing implementation details through interface
    /// Problem: Client needs to know about SQL-specific concepts
    /// </summary>
    public interface ILeakyDataAccess
    {
        // ❌ Exposes SQL implementation details
        Task<object> ExecuteSqlAsync(string sql, Dictionary<string, object> parameters);

        // ❌ Exposes transaction management complexity
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // ❌ Exposes connection management
        bool IsConnectionOpen { get; }
        Task OpenConnectionAsync();
        Task CloseConnectionAsync();
    }

    /// <summary>
    /// LEAKY ABSTRACTION #2: Exception details leak through abstraction
    /// Problem: Client needs to handle different types of exceptions based on implementation
    /// </summary>
    public class LeakyFileStorage
    {
        public async Task SaveFileAsync(string path, byte[] content)
        {
            // ❌ Different implementations throw different exceptions
            // Client needs to know about FileNotFoundException, UnauthorizedAccessException, etc.
            await File.WriteAllBytesAsync(path, content);
        }

        public async Task<byte[]> LoadFileAsync(string path)
        {
            // ❌ Implementation details leak through exceptions
            return await File.ReadAllBytesAsync(path);
        }
    }

    /// <summary>
    /// LEAKY ABSTRACTION #3: Configuration requirements leak through
    /// Problem: Client needs to know about implementation-specific configuration
    /// </summary>
    public interface ILeakyEmailService
    {
        // ❌ Exposes SMTP-specific configuration
        Task SendAsync(string to, string subject, string body,
                      string smtpServer, int port, bool useSsl, string username, string password);
    }

    // =============================================================================
    // ✅ GOOD EXAMPLES - EFFECTIVE ABSTRACTIONS
    // =============================================================================

    /// <summary>
    /// EFFECTIVE ABSTRACTION #1: Clean data access without implementation leakage
    /// Benefits: Client works with domain concepts, not technical details
    /// </summary>
    public interface IUserRepository
    {
        // ✅ Domain-focused methods
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);

        // ✅ High-level query methods hide complexity
        Task<IEnumerable<User>> SearchAsync(UserSearchCriteria criteria);
        Task<bool> ExistsAsync(string email);
    }

    /// <summary>
    /// EFFECTIVE ABSTRACTION #2: Proper exception abstraction
    /// Benefits: Consistent error handling regardless of implementation
    /// </summary>
    public interface IDocumentStorage
    {
        Task<DocumentResult> SaveAsync(string documentId, Stream content, DocumentMetadata metadata);
        Task<DocumentResult> GetAsync(string documentId);
        Task<DocumentResult> DeleteAsync(string documentId);
        Task<IEnumerable<DocumentInfo>> ListAsync(string folderPath = null);
    }

    /// <summary>
    /// Custom result type prevents exception leakage
    /// Client gets consistent error information regardless of underlying implementation
    /// </summary>
    public class DocumentResult
    {
        public bool IsSuccess { get; private set; }
        public Stream Content { get; private set; }
        public DocumentMetadata Metadata { get; private set; }
        public string ErrorMessage { get; private set; }
        public DocumentErrorType ErrorType { get; private set; }

        public static DocumentResult Success(Stream content, DocumentMetadata metadata)
        {
            return new DocumentResult
            {
                IsSuccess = true,
                Content = content,
                Metadata = metadata
            };
        }

        public static DocumentResult NotFound(string documentId)
        {
            return new DocumentResult
            {
                IsSuccess = false,
                ErrorType = DocumentErrorType.NotFound,
                ErrorMessage = $"Document {documentId} not found"
            };
        }

        public static DocumentResult AccessDenied(string documentId)
        {
            return new DocumentResult
            {
                IsSuccess = false,
                ErrorType = DocumentErrorType.AccessDenied,
                ErrorMessage = $"Access denied to document {documentId}"
            };
        }

        public static DocumentResult StorageError(string message)
        {
            return new DocumentResult
            {
                IsSuccess = false,
                ErrorType = DocumentErrorType.StorageError,
                ErrorMessage = message
            };
        }
    }

    /// <summary>
    /// EFFECTIVE ABSTRACTION #3: Self-contained service with proper encapsulation
    /// Benefits: All configuration and complexity hidden from client
    /// </summary>
    public interface INotificationService
    {
        Task<NotificationResult> SendAsync(NotificationRequest request);
        Task<NotificationResult> SendBulkAsync(IEnumerable<NotificationRequest> requests);
        Task<DeliveryStatus> GetDeliveryStatusAsync(string notificationId);
    }

    // =============================================================================
    // IMPLEMENTATION TECHNIQUES FOR EFFECTIVE ABSTRACTIONS
    // =============================================================================

    /// <summary>
    /// TECHNIQUE #1: Use Result Types Instead of Exceptions
    /// Prevents implementation details from leaking through exception types
    /// </summary>
    public class FileDocumentStorage : IDocumentStorage
    {
        private readonly string _basePath;

        public FileDocumentStorage(string basePath)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            Directory.CreateDirectory(_basePath);
        }

        public async Task<DocumentResult> SaveAsync(string documentId, Stream content, DocumentMetadata metadata)
        {
            try
            {
                var filePath = GetFilePath(documentId);
                var directory = Path.GetDirectoryName(filePath);
                Directory.CreateDirectory(directory);

                using var fileStream = File.Create(filePath);
                await content.CopyToAsync(fileStream);

                // Save metadata separately
                await SaveMetadataAsync(documentId, metadata);

                return DocumentResult.Success(null, metadata);
            }
            catch (UnauthorizedAccessException)
            {
                // ✅ Convert implementation-specific exception to abstraction-level error
                return DocumentResult.AccessDenied(documentId);
            }
            catch (DirectoryNotFoundException)
            {
                return DocumentResult.StorageError("Storage location not accessible");
            }
            catch (IOException ex)
            {
                return DocumentResult.StorageError($"Storage error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return DocumentResult.StorageError($"Unexpected error: {ex.Message}");
            }
        }

        public async Task<DocumentResult> GetAsync(string documentId)
        {
            try
            {
                var filePath = GetFilePath(documentId);

                if (!File.Exists(filePath))
                {
                    return DocumentResult.NotFound(documentId);
                }

                var content = new MemoryStream(await File.ReadAllBytesAsync(filePath));
                var metadata = await LoadMetadataAsync(documentId);

                return DocumentResult.Success(content, metadata);
            }
            catch (UnauthorizedAccessException)
            {
                return DocumentResult.AccessDenied(documentId);
            }
            catch (Exception ex)
            {
                return DocumentResult.StorageError($"Error reading document: {ex.Message}");
            }
        }

        public async Task<DocumentResult> DeleteAsync(string documentId)
        {
            try
            {
                var filePath = GetFilePath(documentId);

                if (!File.Exists(filePath))
                {
                    return DocumentResult.NotFound(documentId);
                }

                File.Delete(filePath);
                await DeleteMetadataAsync(documentId);

                return DocumentResult.Success(null, null);
            }
            catch (UnauthorizedAccessException)
            {
                return DocumentResult.AccessDenied(documentId);
            }
            catch (Exception ex)
            {
                return DocumentResult.StorageError($"Error deleting document: {ex.Message}");
            }
        }

        public async Task<IEnumerable<DocumentInfo>> ListAsync(string folderPath = null)
        {
            try
            {
                var searchPath = string.IsNullOrEmpty(folderPath)
                    ? _basePath
                    : Path.Combine(_basePath, folderPath);

                if (!Directory.Exists(searchPath))
                {
                    return new List<DocumentInfo>();
                }

                var files = Directory.GetFiles(searchPath, "*", SearchOption.TopDirectoryOnly);
                var documents = new List<DocumentInfo>();

                foreach (var file in files)
                {
                    if (Path.GetExtension(file) == ".meta") continue; // Skip metadata files

                    var documentId = Path.GetFileNameWithoutExtension(file);
                    var metadata = await LoadMetadataAsync(documentId);

                    documents.Add(new DocumentInfo
                    {
                        DocumentId = documentId,
                        Metadata = metadata,
                        Size = new FileInfo(file).Length,
                        LastModified = File.GetLastWriteTimeUtc(file)
                    });
                }

                return documents;
            }
            catch (Exception ex)
            {
                // ✅ Log error but return empty result instead of throwing
                Console.WriteLine($"Error listing documents: {ex.Message}");
                return new List<DocumentInfo>();
            }
        }

        // ✅ Private methods hide implementation complexity
        private string GetFilePath(string documentId)
        {
            return Path.Combine(_basePath, $"{documentId}.dat");
        }

        private async Task SaveMetadataAsync(string documentId, DocumentMetadata metadata)
        {
            var metadataPath = Path.Combine(_basePath, $"{documentId}.meta");
            var json = System.Text.Json.JsonSerializer.Serialize(metadata);
            await File.WriteAllTextAsync(metadataPath, json);
        }

        private async Task<DocumentMetadata> LoadMetadataAsync(string documentId)
        {
            var metadataPath = Path.Combine(_basePath, $"{documentId}.meta");

            if (!File.Exists(metadataPath))
            {
                return new DocumentMetadata(); // Return default metadata
            }

            var json = await File.ReadAllTextAsync(metadataPath);
            return System.Text.Json.JsonSerializer.Deserialize<DocumentMetadata>(json);
        }

        private async Task DeleteMetadataAsync(string documentId)
        {
            var metadataPath = Path.Combine(_basePath, $"{documentId}.meta");
            if (File.Exists(metadataPath))
            {
                File.Delete(metadataPath);
            }
        }
    }

    /// <summary>
    /// TECHNIQUE #2: Configuration Encapsulation
    /// Hide all configuration complexity inside the implementation
    /// </summary>
    public class EmailNotificationService : INotificationService
    {
        private readonly EmailConfiguration _config;

        public EmailNotificationService(EmailConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<NotificationResult> SendAsync(NotificationRequest request)
        {
            try
            {
                // ✅ All SMTP complexity hidden from client
                Console.WriteLine($"Sending email via {_config.SmtpServer}:{_config.Port}");
                Console.WriteLine($"To: {request.Recipient}");
                Console.WriteLine($"Subject: {request.Subject}");
                Console.WriteLine($"Body: {request.Body}");

                // Simulate email sending
                await Task.Delay(100);

                return NotificationResult.Success(Guid.NewGuid().ToString());
            }
            catch (Exception ex)
            {
                // ✅ Convert implementation exceptions to abstraction-level errors
                return NotificationResult.Failed($"Email delivery failed: {ex.Message}");
            }
        }

        public async Task<NotificationResult> SendBulkAsync(IEnumerable<NotificationRequest> requests)
        {
            var results = new List<string>();

            foreach (var request in requests)
            {
                var result = await SendAsync(request);
                if (result.IsSuccess)
                {
                    results.Add(result.NotificationId);
                }
                else
                {
                    return NotificationResult.Failed($"Bulk send failed: {result.ErrorMessage}");
                }
            }

            return NotificationResult.BulkSuccess(results);
        }

        public async Task<DeliveryStatus> GetDeliveryStatusAsync(string notificationId)
        {
            // ✅ Abstract away implementation-specific status checking
            await Task.Delay(50);
            return new DeliveryStatus
            {
                NotificationId = notificationId,
                Status = DeliveryStatusType.Delivered,
                DeliveredAt = DateTime.UtcNow.AddMinutes(-5)
            };
        }
    }

    /// <summary>
    /// TECHNIQUE #3: Proper Abstraction Layering
    /// Each layer only knows about the layer directly below it
    /// </summary>
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public UserService(IUserRepository userRepository, INotificationService notificationService)
        {
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        /// <summary>
        /// ✅ High-level business operation that orchestrates lower-level abstractions
        /// Client doesn't need to know about repositories or notifications
        /// </summary>
        public async Task<UserRegistrationResult> RegisterUserAsync(UserRegistrationRequest request)
        {
            try
            {
                // Validate request
                if (!IsValidRegistrationRequest(request))
                {
                    return UserRegistrationResult.Invalid("Invalid registration data");
                }

                // Check if user already exists
                if (await _userRepository.ExistsAsync(request.Email))
                {
                    return UserRegistrationResult.Invalid("User already exists");
                }

                // Create user
                var user = new User
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var createdUser = await _userRepository.CreateAsync(user);

                // Send welcome notification
                var notification = new NotificationRequest
                {
                    Recipient = user.Email,
                    Subject = "Welcome to our platform!",
                    Body = $"Hello {user.FirstName}, welcome to our platform!"
                };

                var notificationResult = await _notificationService.SendAsync(notification);

                return UserRegistrationResult.Success(createdUser.Id, notificationResult.NotificationId);
            }
            catch (Exception ex)
            {
                // ✅ Handle any unexpected errors gracefully
                return UserRegistrationResult.Failed($"Registration failed: {ex.Message}");
            }
        }

        private bool IsValidRegistrationRequest(UserRegistrationRequest request)
        {
            return request != null &&
                   !string.IsNullOrWhiteSpace(request.Email) &&
                   !string.IsNullOrWhiteSpace(request.FirstName) &&
                   !string.IsNullOrWhiteSpace(request.LastName) &&
                   request.Email.Contains("@");
        }
    }

    // =============================================================================
    // SUPPORTING TYPES AND MODELS
    // =============================================================================

    public enum DocumentErrorType
    {
        NotFound,
        AccessDenied,
        StorageError,
        InvalidFormat
    }

    public class DocumentMetadata
    {
        public string ContentType { get; set; } = "application/octet-stream";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public Dictionary<string, string> Tags { get; set; } = new();
    }

    public class DocumentInfo
    {
        public string DocumentId { get; set; }
        public DocumentMetadata Metadata { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class NotificationRequest
    {
        public string Recipient { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new();
    }

    public class NotificationResult
    {
        public bool IsSuccess { get; private set; }
        public string NotificationId { get; private set; }
        public List<string> NotificationIds { get; private set; }
        public string ErrorMessage { get; private set; }

        public static NotificationResult Success(string notificationId)
        {
            return new NotificationResult { IsSuccess = true, NotificationId = notificationId };
        }

        public static NotificationResult BulkSuccess(List<string> notificationIds)
        {
            return new NotificationResult { IsSuccess = true, NotificationIds = notificationIds };
        }

        public static NotificationResult Failed(string errorMessage)
        {
            return new NotificationResult { IsSuccess = false, ErrorMessage = errorMessage };
        }
    }

    public class DeliveryStatus
    {
        public string NotificationId { get; set; }
        public DeliveryStatusType Status { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum DeliveryStatusType
    {
        Pending,
        Delivered,
        Failed,
        Bounced
    }

    public class EmailConfiguration
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserSearchCriteria
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
    }

    public class UserRegistrationRequest
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class UserRegistrationResult
    {
        public bool IsSuccess { get; private set; }
        public int UserId { get; private set; }
        public string NotificationId { get; private set; }
        public string ErrorMessage { get; private set; }

        public static UserRegistrationResult Success(int userId, string notificationId)
        {
            return new UserRegistrationResult
            {
                IsSuccess = true,
                UserId = userId,
                NotificationId = notificationId
            };
        }

        public static UserRegistrationResult Invalid(string errorMessage)
        {
            return new UserRegistrationResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }

        public static UserRegistrationResult Failed(string errorMessage)
        {
            return new UserRegistrationResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }

    // =============================================================================
    // DEMONSTRATION PROGRAM
    // =============================================================================

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== EFFECTIVE ABSTRACTIONS DEMONSTRATION ===\n");

            await DemonstrateEffectiveDocumentStorage();
            Console.WriteLine("\n" + new string('=', 60) + "\n");

            await DemonstrateEffectiveNotifications();
            Console.WriteLine("\n" + new string('=', 60) + "\n");

            await DemonstrateEffectiveServiceLayer();
        }

        private static async Task DemonstrateEffectiveDocumentStorage()
        {
            Console.WriteLine("1. EFFECTIVE DOCUMENT STORAGE ABSTRACTION");
            Console.WriteLine("No implementation details leak to client\n");

            IDocumentStorage storage = new FileDocumentStorage("./documents");

            // ✅ Client doesn't need to handle different exception types
            // ✅ Client doesn't need to know about file paths, directories, etc.
            var metadata = new DocumentMetadata
            {
                ContentType = "text/plain",
                CreatedBy = "demo-user"
            };

            using var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Hello World!"));

            var saveResult = await storage.SaveAsync("test-doc", content, metadata);
            if (saveResult.IsSuccess)
            {
                Console.WriteLine("✅ Document saved successfully");
            }
            else
            {
                Console.WriteLine($"❌ Save failed: {saveResult.ErrorMessage}");
            }

            var getResult = await storage.GetAsync("test-doc");
            if (getResult.IsSuccess)
            {
                Console.WriteLine("✅ Document retrieved successfully");
                using var reader = new StreamReader(getResult.Content);
                var text = await reader.ReadToEndAsync();
                Console.WriteLine($"Content: {text}");
            }
        }

        private static async Task DemonstrateEffectiveNotifications()
        {
            Console.WriteLine("2. EFFECTIVE NOTIFICATION ABSTRACTION");
            Console.WriteLine("All configuration complexity hidden\n");

            var config = new EmailConfiguration
            {
                SmtpServer = "smtp.company.com",
                Port = 587,
                UseSsl = true,
                Username = "app@company.com",
                Password = "password",
                FromAddress = "noreply@company.com",
                FromName = "Company App"
            };

            INotificationService notifications = new EmailNotificationService(config);

            // ✅ Client doesn't need to know about SMTP, ports, authentication, etc.
            var request = new NotificationRequest
            {
                Recipient = "user@example.com",
                Subject = "Test Notification",
                Body = "This is a test message"
            };

            var result = await notifications.SendAsync(request);
            if (result.IsSuccess)
            {
                Console.WriteLine($"✅ Notification sent: {result.NotificationId}");

                var status = await notifications.GetDeliveryStatusAsync(result.NotificationId);
                Console.WriteLine($"Delivery Status: {status.Status}");
            }
        }

        private static async Task DemonstrateEffectiveServiceLayer()
        {
            Console.WriteLine("3. EFFECTIVE SERVICE LAYER ABSTRACTION");
            Console.WriteLine("High-level business operations hide complex orchestration\n");

            // Setup (in real app, this would come from DI container)
            IUserRepository userRepo = new InMemoryUserRepository();
            var emailConfig = new EmailConfiguration
            {
                SmtpServer = "smtp.company.com",
                Port = 587,
                FromAddress = "welcome@company.com"
            };
            INotificationService notifications = new EmailNotificationService(emailConfig);

            var userService = new UserService(userRepo, notifications);

            // ✅ Client calls one simple method
            // ✅ All complexity (validation, database, notifications) is hidden
            var registrationRequest = new UserRegistrationRequest
            {
                Email = "newuser@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            var result = await userService.RegisterUserAsync(registrationRequest);
            if (result.IsSuccess)
            {
                Console.WriteLine($"✅ User registered successfully!");
                Console.WriteLine($"User ID: {result.UserId}");
                Console.WriteLine($"Welcome email sent: {result.NotificationId}");
            }
            else
            {
                Console.WriteLine($"❌ Registration failed: {result.ErrorMessage}");
            }
        }
    }

    // Simple in-memory implementation for demo
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();
        private int _nextId = 1;

        public Task<User> GetByIdAsync(int id)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        }

        public Task<User> GetByEmailAsync(string email)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Email == email));
        }

        public Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return Task.FromResult(_users.Where(u => u.IsActive));
        }

        public Task<User> CreateAsync(User user)
        {
            user.Id = _nextId++;
            _users.Add(user);
            return Task.FromResult(user);
        }

        public Task<User> UpdateAsync(User user)
        {
            var existing = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existing != null)
            {
                existing.Email = user.Email;
                existing.FirstName = user.FirstName;
                existing.LastName = user.LastName;
                existing.IsActive = user.IsActive;
                return Task.FromResult(existing);
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

        public Task<IEnumerable<User>> SearchAsync(UserSearchCriteria criteria)
        {
            var query = _users.AsEnumerable();

            if (!string.IsNullOrEmpty(criteria.Email))
                query = query.Where(u => u.Email.Contains(criteria.Email));

            if (!string.IsNullOrEmpty(criteria.FirstName))
                query = query.Where(u => u.FirstName.Contains(criteria.FirstName));

            if (!string.IsNullOrEmpty(criteria.LastName))
                query = query.Where(u => u.LastName.Contains(criteria.LastName));

            if (criteria.IsActive.HasValue)
                query = query.Where(u => u.IsActive == criteria.IsActive.Value);

            return Task.FromResult(query);
        }

        public Task<bool> ExistsAsync(string email)
        {
            return Task.FromResult(_users.Any(u => u.Email == email));
        }
    }
}
namespace _02_Inheritance
{
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
        public virtual string GetContactInfo() => $"Email: {Email}, Phone: {PhoneNumber ?? "Not provided"}";

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
}

using System;

namespace PublicMethodsInheritanceDemo
{
    // ==========================================
    // BASE CLASS WITH PUBLIC METHODS
    // ==========================================

    public class Vehicle
    {
        protected string _brand;
        protected string _model;
        protected int _year;

        public Vehicle(string brand, string model, int year)
        {
            _brand = brand;
            _model = model;
            _year = year;
        }

        // PUBLIC METHODS - Can be called from derived classes
        public void StartEngine()
        {
            Console.WriteLine($"{_brand} {_model} engine started!");
        }

        public void StopEngine()
        {
            Console.WriteLine($"{_brand} {_model} engine stopped!");
        }

        public virtual void DisplayInfo()
        {
            Console.WriteLine($"Vehicle: {_year} {_brand} {_model}");
        }

        public void Honk()
        {
            Console.WriteLine("Beep beep!");
        }

        // This method calls other public methods from within the same class
        public void StartTrip()
        {
            Console.WriteLine("=== Starting Trip ===");
            StartEngine();
            Honk();
            Console.WriteLine("Ready to go!");
        }
    }

    // ==========================================
    // DERIVED CLASS - CAN ACCESS ALL PUBLIC METHODS
    // ==========================================

    public class Car : Vehicle
    {
        private int _doors;

        public Car(string brand, string model, int year, int doors)
            : base(brand, model, year)
        {
            _doors = doors;
        }

        // Override virtual method from base class
        public override void DisplayInfo()
        {
            base.DisplayInfo(); // Calling base class public method
            Console.WriteLine($"Doors: {_doors}");
        }

        // NEW METHOD that calls inherited public methods
        public void GoToWork()
        {
            Console.WriteLine("=== Going to Work ===");

            // Calling inherited public methods directly
            StartEngine();

            Console.WriteLine("Driving to work...");

            // Calling another inherited public method
            StopEngine();

            Console.WriteLine("Arrived at work!");
        }

        // Method that demonstrates calling multiple inherited public methods
        public void WeekendTrip()
        {
            Console.WriteLine("=== Weekend Trip ===");

            // Call inherited public method that itself calls other public methods
            StartTrip();

            Console.WriteLine("Enjoying the weekend drive...");

            // Call inherited public methods individually
            Honk(); // Say hello to other drivers
            StopEngine();

            Console.WriteLine("Weekend trip completed!");
        }

        // Method that shows we can call public methods in any order
        public void ParkingLotManeuver()
        {
            StartEngine();
            Console.WriteLine("Maneuvering in parking lot...");
            Honk(); // Alert pedestrians
            Honk(); // Alert again
            StopEngine();
        }
    }

    // ==========================================
    // ANOTHER DERIVED CLASS
    // ==========================================

    public class Motorcycle : Vehicle
    {
        private bool _hasSidecar;

        public Motorcycle(string brand, string model, int year, bool hasSidecar)
            : base(brand, model, year)
        {
            _hasSidecar = hasSidecar;
        }

        public override void DisplayInfo()
        {
            base.DisplayInfo(); // Calling base public method
            Console.WriteLine($"Sidecar: {(_hasSidecar ? "Yes" : "No")}");
        }

        // Method specific to motorcycle that uses inherited public methods
        public void RideToBeach()
        {
            Console.WriteLine("=== Motorcycle Beach Ride ===");

            StartEngine(); // Inherited public method
            Console.WriteLine("Riding along the coast...");

            // Motorcycles have different horn sound, but we can still use base method
            Console.WriteLine("Custom motorcycle horn:");
            Honk(); // Inherited public method

            StopEngine(); // Inherited public method
            Console.WriteLine("Enjoying the beach!");
        }
    }

    // ==========================================
    // MULTI-LEVEL INHERITANCE EXAMPLE
    // ==========================================

    public class SportsCar : Car
    {
        private int _horsepower;

        public SportsCar(string brand, string model, int year, int doors, int horsepower)
            : base(brand, model, year, doors)
        {
            _horsepower = horsepower;
        }

        public override void DisplayInfo()
        {
            base.DisplayInfo(); // Calls Car's DisplayInfo, which calls Vehicle's DisplayInfo
            Console.WriteLine($"Horsepower: {_horsepower}");
        }

        // Method that shows calling public methods through inheritance chain
        public void RaceDay()
        {
            Console.WriteLine("=== Race Day ===");

            // These are all inherited from Vehicle (grandparent class)
            StartEngine();
            Honk(); // Signal ready to race

            Console.WriteLine($"Racing with {_horsepower} horsepower!");

            StopEngine();

            Console.WriteLine("Race completed!");
        }

        // Method that calls methods from different levels of inheritance
        public void ShowOff()
        {
            DisplayInfo(); // Calls overridden method (goes through inheritance chain)

            // Call inherited public methods from Vehicle
            StartEngine();
            Honk();

            // Call inherited method from Car
            GoToWork(); // This method itself calls more inherited methods!

            StopEngine();
        }
    }

    // ==========================================
    // DEMONSTRATION OF PUBLIC METHOD ACCESS
    // ==========================================

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== PUBLIC METHODS IN DERIVED CLASSES ===\n");

            // 1. Basic derived class calling public methods
            Console.WriteLine("1. CAR CALLING INHERITED PUBLIC METHODS:");
            var car = new Car("Toyota", "Camry", 2023, 4);
            car.DisplayInfo(); // Overridden method that calls base public method
            car.GoToWork(); // New method that calls inherited public methods
            Console.WriteLine();

            // 2. Different derived class using same public methods
            Console.WriteLine("2. MOTORCYCLE CALLING INHERITED PUBLIC METHODS:");
            var motorcycle = new Motorcycle("Harley", "Street 750", 2023, false);
            motorcycle.DisplayInfo();
            motorcycle.RideToBeach();
            Console.WriteLine();

            // 3. Multi-level inheritance
            Console.WriteLine("3. SPORTS CAR (MULTI-LEVEL INHERITANCE):");
            var sportsCar = new SportsCar("Ferrari", "F8", 2023, 2, 710);
            sportsCar.DisplayInfo(); // Goes through inheritance chain
            sportsCar.RaceDay(); // Calls grandparent public methods
            Console.WriteLine();

            // 4. Show that we can call public methods directly on derived objects
            Console.WriteLine("4. DIRECT CALLS TO INHERITED PUBLIC METHODS:");
            Console.WriteLine("Car object calling Vehicle methods directly:");
            car.StartEngine(); // Direct call to inherited public method
            car.Honk(); // Direct call to inherited public method
            car.StopEngine(); // Direct call to inherited public method
            Console.WriteLine();

            // 5. Show polymorphism with public methods
            Console.WriteLine("5. POLYMORPHISM WITH PUBLIC METHODS:");
            Vehicle[] vehicles = { car, motorcycle, sportsCar };

            foreach (Vehicle vehicle in vehicles)
            {
                vehicle.StartEngine(); // Public method called on base reference
                vehicle.DisplayInfo(); // Virtual method - calls overridden versions
                vehicle.StopEngine(); // Public method called on base reference
                Console.WriteLine("---");
            }

            // 6. Show that derived classes can call public methods in any combination
            Console.WriteLine("6. FLEXIBLE METHOD CALLING:");
            car.WeekendTrip(); // Method that calls multiple inherited public methods
            Console.WriteLine();

            sportsCar.ShowOff(); // Method that calls methods from different inheritance levels
        }
    }
}
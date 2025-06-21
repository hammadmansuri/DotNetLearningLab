using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _02_Inheritance
{
    public class InheritanceDemo1
    {
        public static void RunDemo()
        {
            // Create objects of different types

            var people = new List<Person>
            {
                new Person("John", "Doe", new DateTime(1985, 5, 15), "john.doe@email.com"),
                new Employee("Jane", "Smith", new DateTime(1990, 3, 12), "jane.smith@company.com",
                                "HR", 65000m, "HR Specialist"),
                new Manager1("Bob", "Johnson", new DateTime(1975, 8, 10), "bob.j@company.com",
                          "Engineering", 95000m, "Engineering Manager")
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

            //Employee test = new Employee("TestFirstName", "TestLastName", new DateTime(1990, 5, 12), "test@email.com",
            //                                "Engineering", 120000m, "Senior Software Engineer");

            //test.DisplayBasicInfo();

            //test.SetEmployeeSSN("abc");

            //Console.WriteLine("\n3. TEST:");

            //Manager1 manager = new Manager1("Bob", "Johnson", new DateTime(1975, 8, 10), "bob.j@company.com",
            //              "Engineering", 95000m, "Engineering Manager");

            //manager.SetEmployeeSSN("123-45-678");
            //manager.ApproveBudget(3500m);
        }
    }
}

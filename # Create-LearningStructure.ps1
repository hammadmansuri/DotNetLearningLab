# Create-LearningStructure.ps1

# Define the main categories and subcategories
$structure = @{
    "01-OOP-Fundamentals" = @(
        "01-Classes-Objects",
        "02-Inheritance", 
        "03-Polymorphism",
        "04-Encapsulation",
        "05-Abstraction",
        "06-Interfaces-vs-Abstract"
    )
    "02-OOP-Advanced" = @(
        "01-SOLID-Principles",
        "02-Design-Patterns",
        "03-Composition-vs-Inheritance",
        "04-Advanced-Inheritance"
    )
    "03-CSharp-Fundamentals" = @(
        "01-Value-vs-Reference-Types",
        "02-Memory-Management",
        "03-Exception-Handling",
        "04-Collections-Generics",
        "05-Operators-Indexers"
    )
    "04-CSharp-Advanced" = @(
        "01-Delegates-Events",
        "02-Lambda-LINQ",
        "03-Async-Await",
        "04-Reflection-Attributes",
        "05-Extension-Methods",
        "06-Nullable-Types",
        "07-Pattern-Matching",
        "08-Records-Init",
        "09-Span-Memory"
    )
    "05-Performance-Memory" = @(
        "01-Benchmarking",
        "02-Memory-Optimization",
        "03-ArrayPool-ObjectPool",
        "04-Unsafe-Code",
        "05-Performance-Patterns"
    )
    "06-DotNet-Core-Features" = @(
        "01-Configuration",
        "02-Dependency-Injection",
        "03-Logging",
        "04-Options-Pattern",
        "05-Background-Services",
        "06-Health-Checks"
    )
    "07-Data-Access" = @(
        "01-EF-Core-Basics",
        "02-Relationships",
        "03-Migrations",
        "04-Query-Optimization",
        "05-Raw-SQL",
        "06-Advanced-EF"
    )
    "08-Testing" = @(
        "01-Unit-Testing",
        "02-Mocking",
        "03-Integration-Testing",
        "04-Test-Data-Builders",
        "05-BDD-Testing"
    )
    "09-Design-Patterns" = @(
        "01-Creational",
        "02-Structural",
        "03-Behavioral",
        "04-Enterprise-Patterns"
    )
    "10-Architecture-Patterns" = @(
        "01-Repository-UnitOfWork",
        "02-CQRS",
        "03-Event-Sourcing",
        "04-Clean-Architecture",
        "05-Microservices-Patterns"
    )
    "11-Azure-Experiments" = @(
        "01-Storage-Blobs",
        "02-Service-Bus",
        "03-Functions",
        "04-Key-Vault",
        "05-Cognitive-Services"
    )
}

# Create directory structure
foreach ($category in $structure.Keys) {
    Write-Host "Creating category: $category" -ForegroundColor Green
    New-Item -ItemType Directory -Path $category -Force | Out-Null
    
    foreach ($subcategory in $structure[$category]) {
        $fullPath = Join-Path $category $subcategory
        Write-Host "  Creating: $subcategory" -ForegroundColor Yellow
        New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
        
        # Create console app in each subcategory
        Push-Location $fullPath
        dotnet new console --framework net8.0
        Pop-Location
    }
}

Write-Host "Structure created successfully!" -ForegroundColor Green
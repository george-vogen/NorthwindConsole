using NLog;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using NorthwindConsole.Model;
using System.ComponentModel.DataAnnotations;
string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");

do
{
    Console.WriteLine("1) Display All Products");
    Console.WriteLine("2) Display Specific Product");
    Console.WriteLine("3) Add a Product");
    Console.WriteLine("4) Edit a Product");
    Console.WriteLine("5) Display categories");
    Console.WriteLine("6) Add category");
    Console.WriteLine("7) Display Category and related products");
    Console.WriteLine("8) Display all Categories and their related products");
    Console.WriteLine("Enter to quit");
    string? choice = Console.ReadLine();
    Console.Clear();
    logger.Info("Option {choice} selected", choice);

    if (choice == "1")
    {
        // Display All Products
        var db = new DataContext();

        Console.WriteLine("Show products:");
        Console.WriteLine("1) All");
        Console.WriteLine("2) Active only");
        Console.WriteLine("3) Discontinued only");
        string? filterChoice = Console.ReadLine();
        Console.Clear();
        logger.Info("Option {filterChoice} selected", filterChoice);

        var query = db.Products.OrderBy(p => p.ProductName);

        switch (filterChoice)
        {
            case "2":
                query = query.Where(p => p.Discontinued == false).OrderBy(p => p.ProductName);
                break;
            case "3":
                query = query.Where(p => p.Discontinued == true).OrderBy(p => p.ProductName);
                break;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{query.Count()} records returned");
        Console.ForegroundColor = ConsoleColor.Magenta;
        
        foreach (var item in query)
        {
            Console.WriteLine($"{item.ProductName} - {(item.Discontinued ? "Discontinued" : "Active")}");
        }

        Console.ForegroundColor = ConsoleColor.White;
    }
    else if (choice == "2")
    {
        // Display Specific Product
        var db = new DataContext();
        var query = db.Products.OrderBy(p => p.ProductId);

        Console.WriteLine("Select the product that you would like to see:");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        foreach (var item in query)
        {
            Console.WriteLine($"{item.ProductId}) {item.ProductName}");
        }
        Console.ForegroundColor = ConsoleColor.White;
        int id = int.Parse(Console.ReadLine()!);
        Console.Clear();
        logger.Info($"ProductId {id} selected");

        Product? product = null;

        product = db.Products.Include("Category").Include("Supplier").FirstOrDefault(p => p.ProductId == id);
        
        if (product != null)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Product ID: {product.ProductId}");
            Console.WriteLine($"Product Name: {product.ProductName}");
            Console.WriteLine($"Category: {product.Category?.CategoryName ?? "N/A"}");
            Console.WriteLine($"Supplier: {product.Supplier?.CompanyName ?? "N/A"}");
            Console.WriteLine($"Quantity Per Unit: {product.QuantityPerUnit ?? "N/A"}");
            Console.WriteLine($"Unit Price: ${product.UnitPrice:F2}");
            Console.WriteLine($"Units In Stock: {product.UnitsInStock}");
            Console.WriteLine($"Units On Order: {product.UnitsOnOrder}");
            Console.WriteLine($"Reorder Level: {product.ReorderLevel}");
            Console.WriteLine($"Discontinued: {(product.Discontinued ? "YES" : "NO")}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Product not found.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
    else if (choice == "3")
    {
        // Add a Product
        var db = new DataContext();

        var product = new Product();

        string productName;
        while (true)
        {
            Console.WriteLine("Enter product name:");
            productName = Console.ReadLine()!;

            if (string.IsNullOrWhiteSpace(productName))
            {
                Console.WriteLine("Product name is required.");
                continue;
            }

            if (db.Products.Any(p => p.ProductName == productName))
            {
                Console.WriteLine("A product with that name already exists. Enter a different product name.");
                continue;
            }

            break;
        }
        product.ProductName = productName;

        var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
        Console.WriteLine("Available categories:");
        foreach (var category in categories)
        {
            Console.WriteLine($"{category.CategoryId}) {category.CategoryName}");
        }

        int categoryId;
        while (true)
        {
            Console.WriteLine("Enter Category ID:");
            if (int.TryParse(Console.ReadLine(), out categoryId) && categories.Any(c => c.CategoryId == categoryId))
            {
                product.CategoryId = categoryId;
                break;
            }
            Console.WriteLine("Invalid Category ID. Please enter one of the IDs shown above.");
        }

        var suppliers = db.Suppliers.OrderBy(s => s.SupplierId).ToList();
        Console.WriteLine("Available suppliers:");
        foreach (var supplier in suppliers)
        {
            Console.WriteLine($"{supplier.SupplierId}) {supplier.CompanyName}");
        }

        int supplierId;
        while (true)
        {
            Console.WriteLine("Enter Supplier ID:");
            if (int.TryParse(Console.ReadLine(), out supplierId) && suppliers.Any(s => s.SupplierId == supplierId))
            {
                product.SupplierId = supplierId;
                break;
            }
            Console.WriteLine("Invalid Supplier ID. Please enter one of the IDs shown above.");
        }

        string quantityPerUnit;
        do
        {
            Console.WriteLine("Enter quantity per unit:");
            quantityPerUnit = Console.ReadLine()!;
        } while (string.IsNullOrWhiteSpace(quantityPerUnit));
        product.QuantityPerUnit = quantityPerUnit;

        decimal unitPrice;
        while (true)
        {
            Console.WriteLine("Enter unit price:");
            if (decimal.TryParse(Console.ReadLine(), out unitPrice))
            {
                product.UnitPrice = unitPrice;
                break;
            }
            Console.WriteLine("Invalid unit price. Please enter a decimal number.");
        }

        short unitsInStock;
        while (true)
        {
            Console.WriteLine("Enter units in stock:");
            if (short.TryParse(Console.ReadLine(), out unitsInStock))
            {
                product.UnitsInStock = unitsInStock;
                break;
            }
            Console.WriteLine("Invalid units in stock. Please enter a number.");
        }

        product.Discontinued = false;

        db.AddProduct(product);

        Console.WriteLine("Product added with ID: " + product.ProductId);
    }
    else if (choice == "4")
    {
        // Edit a Product
        
    }
    else if (choice == "5")
    {
        // display categories
        var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");

        var config = configuration.Build();

        var db = new DataContext();
        var query = db.Categories.OrderBy(p => p.CategoryName);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{query.Count()} records returned");
        Console.ForegroundColor = ConsoleColor.Magenta;
        foreach (var item in query)
        {
            Console.WriteLine($"{item.CategoryName} - {item.Description}");
        }
        Console.ForegroundColor = ConsoleColor.White;
    }
    else if (choice == "6")
    {
        // Add category
        Category category = new();
        Console.WriteLine("Enter Category Name:");
        category.CategoryName = Console.ReadLine()!;
        Console.WriteLine("Enter the Category Description:");
        category.Description = Console.ReadLine();
        ValidationContext context = new ValidationContext(category, null, null);
        List<ValidationResult> results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(category, context, results, true);
        if (isValid)
        {
            var db = new DataContext();
            // check for unique name
            if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
            {
                // generate validation error
                isValid = false;
                results.Add(new ValidationResult("Name exists", ["CategoryName"]));
            }
            else
            {
                logger.Info("Validation passed");
                // TODO: save category to db
            }
        }
        if (!isValid)
        {
            foreach (var result in results)
            {
                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
            }
        }
    }
    else if (choice == "7")
    {
        // Display Category and related products
        var db = new DataContext();
        var query = db.Categories.OrderBy(p => p.CategoryId);

        Console.WriteLine("Select the category whose products you want to display:");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        foreach (var item in query)
        {
            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
        }
        Console.ForegroundColor = ConsoleColor.White;
        int id = int.Parse(Console.ReadLine()!);
        Console.Clear();
        logger.Info($"CategoryId {id} selected");
        Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id)!;
        Console.WriteLine($"{category.CategoryName} - {category.Description}");
        foreach (Product p in category.Products)
        {
            Console.WriteLine($"\t{p.ProductName}");
        }
    }
    else if (choice == "8")
    {
        // Display all Categories and their related products
        var db = new DataContext();
        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
        foreach (var item in query)
        {
            Console.WriteLine($"{item.CategoryName}");
            foreach (Product p in item.Products)
            {
                Console.WriteLine($"\t{p.ProductName}");
            }
        }
    }
    else if (String.IsNullOrEmpty(choice))
    {
        break;
    }
    Console.WriteLine();
} while (true);

logger.Info("Program ended");
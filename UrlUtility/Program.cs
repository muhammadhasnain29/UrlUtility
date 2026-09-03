using UrlUtility.Services;

while (true)
{
    Console.Clear();

    Console.WriteLine("========================================");
    Console.WriteLine("             URL UTILITY");
    Console.WriteLine("========================================");
    Console.WriteLine();

    Console.WriteLine("Do you want to go through Initialize?");
    Console.WriteLine();
    Console.WriteLine("1. Initialize");
    Console.WriteLine("2. Somewhere Else");
    Console.WriteLine();

    Console.Write("Enter your choice (1 or 2): ");

    var choice = Console.ReadLine();

    Console.WriteLine();

    if (choice == "1")
    {
        var urlRunner = new UrlRunnerService();

        await urlRunner.RunInitializeAsync();
    }
    else if (choice == "2")
    {
        Console.WriteLine("Thank you!");
        Console.WriteLine("No file exists for Somewhere Else.");
    }
    else
    {
        Console.WriteLine("Invalid choice.");
        Console.WriteLine("Please enter 1 or 2.");

        Console.WriteLine();
        Console.Write("Press any key to continue...");
        Console.ReadKey();

        continue;
    }

    Console.WriteLine();
    Console.WriteLine("========================================");
    Console.WriteLine("Do you want to proceed more?");
    Console.WriteLine();
    Console.WriteLine("1. Proceed");
    Console.WriteLine("0. Leave");
    Console.WriteLine();

    Console.Write("Enter your choice (1 or 0): ");

    var proceedChoice = Console.ReadLine();

    if (proceedChoice == "0")
    {
        Console.WriteLine();
        Console.WriteLine("Thank you for using URL Utility.");
        Console.WriteLine("Goodbye!");

        break;
    }

    if (proceedChoice != "1")
    {
        Console.WriteLine();
        Console.WriteLine("Invalid choice. Exiting...");

        break;
    }
}
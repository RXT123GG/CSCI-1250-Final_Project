namespace biblioteca;

internal class LibraryItem
{
    // Below are object properties
    public int Id { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public double DailyLateFee { get; set; }
    public int Stock { get; set; }


    public LibraryItem(int id, string title, string type, double dailyLateFee, int stock = 1)
    {
        Id = id;
        Title = title;
        Type = type;
        DailyLateFee = dailyLateFee;
        Stock = stock;
    }
    // ^^ Constructor ^^

    // Below is overriding the ToString functionality for this class
    // That way it returns a fomatted version every time
    public override string ToString()
    {
        return $"{Id} - {Title} ({Type}) | Daily Late Fee: ${DailyLateFee:F2} | Stock: {Stock}";
    }



}



internal class CheckoutItem
{
    // CheckoutItem properties and Constructor below
    public LibraryItem Item { get; set; }
    public DateTime CheckoutDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedDate { get; set; }

    public CheckoutItem(LibraryItem item, DateTime checkoutDate)
    {
        Item = item;
        CheckoutDate = checkoutDate;
        ReturnedDate = null;
        int loanDays;
        if (item.Type == "DVD")
            loanDays = 3;
        else
            loanDays = 7;
        DueDate = checkoutDate.AddDays(loanDays);
    }
    // ^ Constructor ^
    public double CalculateLateFee(int daysLate)
    {
        if (daysLate <= 0) return 0;
        return daysLate * Item.DailyLateFee;
    }

    public int GetDaysLate()
    {
        // Use ReturnedDate if it has a value; otherwise use DateTime.Now
        DateTime effectiveReturnDate = ReturnedDate.HasValue ? ReturnedDate.Value : DateTime.Now;
        int daysLate = (effectiveReturnDate - DueDate).Days;
        return Math.Max(daysLate, 0); // never negative
    }

    public string GetReceiptLine()
    {
        int daysLate = GetDaysLate();
        double fee = CalculateLateFee(daysLate);
        return $"{Item.Id} - {Item.Title} ({Item.Type}) | Days Late: {daysLate} | Late Fee: ${fee:F2}";
    }

    public void MarkAsReturned(DateTime returnDate)
    {
        ReturnedDate = returnDate;
    }
}



internal class LibrarySystem
{
    // Class of functions mostly handling the Library System itself and navigation
    static List<LibraryItem> catalog = new List<LibraryItem>();

    static void Main()
    {
        // Load catalog at startup
        catalog = CatalogManagement.LoadCatalogFromFile();

        // Start the menu loop
        DisplayMenu();

    }

    static void DisplayMenu()
    {
        bool exitRequested = false;
        List<CheckoutItem> checkoutList = new List<CheckoutItem>();

        while (!exitRequested)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n===== Library Checkout System =====");
            Console.ResetColor();
            Console.WriteLine("1. View Catalog");
            Console.WriteLine("2. Add Library Item");
            Console.WriteLine("3. Check Out Item");
            Console.WriteLine("4. Return Item");
            Console.WriteLine("5. View My Checkout Receipt");
            Console.WriteLine("6. Save My Checkout List");
            Console.WriteLine("7. Load My Previous Checkout List");
            Console.WriteLine("8. Set Catalog Location");
            Console.WriteLine("9. Set Checkout List Location");
            Console.WriteLine("10. Exit");

            int choice = Utility.GetValidMenuChoice(1, 10);
            // The switch of doom (Menu Navigation switch that calls the appropriate function based on user-input)
            switch (choice)
            {
                case 1:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    CatalogManagement.DisplayCatalog(catalog);
                    Console.ResetColor();
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                    break;

                case 2:
                    CatalogManagement.AddLibraryItem(catalog);
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                    break;

                case 3:
                    CheckoutLogic.CheckOutItem(checkoutList, catalog);
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                    break;

                case 4:
                    CheckoutLogic.ReturnItem(checkoutList);
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                    break;

                case 5:
                    CheckoutLogic.DisplayCheckoutList(checkoutList);
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                    break;

                case 6:
                    Utility.SaveCheckOutListToFile(checkoutList);
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                    break;

                case 7:
                    checkoutList = Utility.LoadCheckOutListToFile(catalog);
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                    break;

                case 8:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Utility.SaveCatalogPath();

                    // ⭐ Reloads the catalog from the new file * <-- If this is gone the old catalog will load with the new catalog
                    catalog = CatalogManagement.LoadCatalogFromFile();

                    Console.ResetColor();
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                    break;

                case 9:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Utility.SaveCheckoutPath();

                    // * Reloads the Checkout List from the new file * 
                    checkoutList = Utility.LoadCheckOutListToFile(catalog);

                    Console.ResetColor();
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                    break;

                case 10:
                    exitRequested = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Exiting the Library System. Goodbye!");
                    Console.ResetColor();
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid choice. Please select again.");
                    Console.ResetColor();
                    Console.WriteLine("\nPress any key to shut down...");
                    Console.ReadKey();
                    Console.Clear();
                    break;
            }
        }

    }
}

    internal class CatalogManagement
    {
        // Class that handles most of the management of the catalog

        internal static bool ContainsId(List<LibraryItem> catalog, int id)
        {
            return catalog.Any(item => item.Id == id);
        }


    public static List<LibraryItem> LoadCatalogFromFile()
    {
        // Loads the catalog from a file. If the file doesn't exist it makes one, if it has duplicate IDs it will purge them automatically.
        List<LibraryItem> catalogList = new List<LibraryItem>();

        if (!File.Exists(Utility.CatalogFilePath))
        {
            Console.WriteLine($"Catalog file not found at {Utility.CatalogFilePath}. Starting with empty catalog.");
            return catalogList;
        }

        try
        {
            string[] lines = File.ReadAllLines(Utility.CatalogFilePath);

            foreach (string line in lines)
            {
                LibraryItem item = Utility.ParseLibraryItemFromLine(line);
                if (item != null)
                    catalogList.Add(item);
            }

            // Remove duplicate IDs
            var unique = catalogList
                .GroupBy(i => i.Id)
                .Select(g => g.First())
                .ToList();

            if (unique.Count != catalogList.Count)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning: Duplicate IDs found in catalog file. Duplicates removed.");
                Console.ResetColor();

                SaveCatalogToFile(unique); // rewrite cleaned catalog
            }

            Console.WriteLine("Catalog loaded successfully!");
            return unique;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading catalog: {ex.Message}");
            return catalogList;
        }
    }



    public static void SaveCatalogToFile(List<LibraryItem> catalog)
    {
        // Saves the current catalog to a file. Uses the file location set by the user in the menu
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Utility.CatalogFilePath)!);

            using (StreamWriter writer = new StreamWriter(Utility.CatalogFilePath, false))
            {
                foreach (var item in catalog)
                {
                    writer.WriteLine($"{item.Id},{item.Title},{item.Type},{item.DailyLateFee:F2},{item.Stock}");
                }
            }

            Console.WriteLine("Catalog saved successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving catalog: {ex.Message}");
        }
    }




    internal static void AddLibraryItem(List<LibraryItem> catalog)
    {
        // Adds a new item to the catalog. Will prevent duplicate Ids
        int id = Utility.AskUserForChoice("Enter the new item ID:");

        if (ContainsId(catalog, id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: An item with this ID already exists in the catalog.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("Enter the title:");
        string title = Console.ReadLine().Trim();

        // Validation for the types of items. Does Books and DVDs only right now, but could be changed to do others with the same setup.
        string type;
        while (true)
        {
            Console.WriteLine("Enter the type (Book/DVD):");
            type = Console.ReadLine().Trim();

            if (type.Equals("Book", StringComparison.OrdinalIgnoreCase))
            {
                type = "Book";  // Force proper casing for saving later, accepts any case from the user
                break;
            }

            if (type.Equals("DVD", StringComparison.OrdinalIgnoreCase))
            {
                type = "DVD";   // Force proper casing for saving later, accepts any case from the user
                break;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid type! You must enter either 'Book' or 'DVD'.");
            Console.ResetColor();
        }
        // --- End validation ---

        Console.WriteLine("Enter the daily late fee (decimal):");
        double fee = double.Parse(Console.ReadLine().Trim());

        Console.WriteLine("Enter the initial stock (number of copies):");
        int stock;
        while (!int.TryParse(Console.ReadLine()?.Trim(), out stock) || stock < 1)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid input. Stock must be a positive integer.");
            Console.ResetColor();
        }

        LibraryItem newItem = new LibraryItem(id, title, type, fee, stock);
        catalog.Add(newItem);
        SaveNewItemToCatalogFile(newItem);   // File will always save Book/DVD with proper capitalization so my eyes don't bleed reading the catalog
    }





    static void SaveNewItemToCatalogFile(LibraryItem item)
        {
            // Saves the item from AddLibraryItem to the catalog file specified by the user in the menu
            try
            {
                // Ensure target directory exists
                string? dir = Path.GetDirectoryName(Utility.CatalogFilePath);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                Console.WriteLine($"Saving new catalog item to: {Utility.CatalogFilePath}");

                using (StreamWriter writer = new StreamWriter(Utility.CatalogFilePath, append: true))
                {
                    writer.WriteLine($"{item.Id},{item.Title},{item.Type},{item.DailyLateFee:F2},{item.Stock}");
                }

            Console.WriteLine("Item saved to catalog successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving item to catalog: {ex.Message}");
            }
        }

        public static void DisplayCatalog(List<LibraryItem> catalog)
        {
            Console.WriteLine("===== Library Catalog =====");
            foreach (var item in catalog)
            {
                Console.WriteLine(item.ToString());
            }
            Console.WriteLine("===========================");
        }


        internal static LibraryItem FindItemById(int id, List<LibraryItem> catalog)
        {
            return catalog.FirstOrDefault(i => i.Id == id);
        }

    }

    internal class CheckoutLogic
    {
        // Class with functions handling checkout logic
        public static void CheckOutItem(List<CheckoutItem> checkoutList, List<LibraryItem> catalog)
        {
            Console.WriteLine("Enter the ID of the item you want to check out:");
            int id = Utility.AskUserForChoice("");

            LibraryItem item = CatalogManagement.FindItemById(id, catalog);
            if (item == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Item not found in catalog.");
                Console.ResetColor();
                return;
            }

            if (IsItemCheckedOut(id, checkoutList))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Item is already checked out.");
                Console.ResetColor();
                return;
            }

        if (item.Stock <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("This item is out of stock and cannot be checked out.");
            Console.ResetColor();
            return;
        }

        CheckoutItem checkoutItem = new CheckoutItem(item, DateTime.Now);
            checkoutList.Add(checkoutItem);
            item.Stock--;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Successfully checked out: {item.Title}");
            Console.ResetColor();
        }

        public static void ReturnItem(List<CheckoutItem> checkoutList)
        {
            Console.WriteLine("Enter the ID of the item you want to return:");
            int id = Utility.AskUserForChoice("");

            CheckoutItem itemToReturn = checkoutList.FirstOrDefault(ci => ci.Item.Id == id && !ci.ReturnedDate.HasValue);

            if (itemToReturn == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Item not found in your current checkouts or already returned.");
                Console.ResetColor();
                return;
            }

            itemToReturn.MarkAsReturned(DateTime.Now);
            itemToReturn.Item.Stock++;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Item returned: {itemToReturn.Item.Title}");
            Console.WriteLine($"Late Fee: ${itemToReturn.CalculateLateFee(itemToReturn.GetDaysLate()):F2}");
            Console.ResetColor();
        }

        public static void DisplayCheckoutList(List<CheckoutItem> checkoutList)
        {
            if (checkoutList.Count == 0)
            {
                Console.WriteLine("You have no items checked out.");
                return;
            }

            Console.WriteLine("===== My Checkout List =====");
            foreach (var ci in checkoutList)
            {
                string status = ci.ReturnedDate.HasValue ? "Returned" : "Checked Out";
                Console.WriteLine($"{ci.GetReceiptLine()} | Status: {status}");
            }
            Console.WriteLine("============================");
        }

        public static bool IsItemCheckedOut(int id, List<CheckoutItem> checkoutList)
        {
            return checkoutList.Any(ci => ci.Item.Id == id && !ci.ReturnedDate.HasValue);
        }

    }

    internal class Utility
    {
        // Class with assorted utility functions. Mostly functions that are used frequently or don't fit elsewhere.
        public static string CatalogFilePath { get; private set; } = "catalog.txt";
        public static string CheckoutFilePath { get; private set; } = "myCheckouts.txt";

        public static void SaveCatalogPath()
        {
            Console.WriteLine("Enter full path for catalog file (or just a folder). Press Enter for default:");
            string input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                // keep default
                Console.WriteLine($"Using default catalog path: {CatalogFilePath}");
                return;
            }

            // If user gave a folder path (exists or ends with separator), create or use that folder + default filename
            if (Directory.Exists(input) || input.EndsWith(Path.DirectorySeparatorChar) || input.EndsWith(Path.AltDirectorySeparatorChar))
            {
                CatalogFilePath = Path.Combine(input, "catalog.txt");
            }
            else
            {
                // If input has no extension, treat as folder
                string fileName = Path.GetFileName(input);
                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                {
                    // probably a folder name — combine with current or parent
                    CatalogFilePath = Path.Combine(input, "catalog.txt");
                }
                else
                {
                    // looks like a file path
                    CatalogFilePath = input;
                }
            }

            // Ensure directory exists
            string? dir = Path.GetDirectoryName(CatalogFilePath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            Console.WriteLine($"Catalog path set to: {CatalogFilePath}");
        }

        public static void SaveCheckoutPath()
        {
            Console.WriteLine("Enter full path for checkout file (or just a folder). Press Enter for default:");
            string input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine($"Using default checkout path: {CheckoutFilePath}");
                return;
            }

            if (Directory.Exists(input) || input.EndsWith(Path.DirectorySeparatorChar) || input.EndsWith(Path.AltDirectorySeparatorChar))
            {
                CheckoutFilePath = Path.Combine(input, "myCheckouts.txt");
            }
            else
            {
                string fileName = Path.GetFileName(input);
                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                    CheckoutFilePath = Path.Combine(input, "myCheckouts.txt");
                else
                    CheckoutFilePath = input;
            }

            string? dir = Path.GetDirectoryName(CheckoutFilePath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            Console.WriteLine($"Checkout path set to: {CheckoutFilePath}");

        }
        public static bool AskYesNo(string prompt)
        {
            // Reusable yes/no question returning tool
            bool formattedUserInput = false;
            while (true)
            {
                Console.WriteLine(prompt);
                string yesNo = Console.ReadLine().ToUpper().Trim();
                if (yesNo == "YES" || yesNo == "Y") return true;
                else if (yesNo == "NO" || yesNo == "N") return false;
                else Console.WriteLine("Please enter a valid Yes or No input");
            }
        }

        public static int AskUserForChoice(string prompt)
        {
            // Reusable prompt for returning an integer, mostly used for menu navigation
            bool gettingInput = true;
            int formattedUserInput = 0;
            while (gettingInput == true)
            {
                try
                {
                    Console.WriteLine(prompt);
                    var userInput = Console.ReadLine()?.Trim();
                    formattedUserInput = int.Parse(userInput);
                    gettingInput = false;
                }
                catch
                {
                    Console.WriteLine("Please enter a valid numeric input");
                }
            }
            return formattedUserInput;

        }

        public static void SaveCheckOutListToFile(List<CheckoutItem> checkoutList)
        {
            try
            {
                string? dir = Path.GetDirectoryName(Utility.CheckoutFilePath);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                Console.WriteLine($"Saving checkout list to: {Utility.CheckoutFilePath}");

                using (StreamWriter writer = new StreamWriter(Utility.CheckoutFilePath, append: false))
                {
                    foreach (var item in checkoutList)
                    {
                        string returnedDateStr = item.ReturnedDate.HasValue
                                                 ? item.ReturnedDate.Value.ToString("yyyy-MM-dd")
                                                 : "";

                        string line = $"{item.Item.Id},{item.Item.Title},{item.Item.Type}," +
                                      $"{item.CheckoutDate:yyyy-MM-dd},{item.DueDate:yyyy-MM-dd},{returnedDateStr}";

                        writer.WriteLine(line);
                    }
                }
                Console.WriteLine("Checkout list saved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving checkout list: {ex.Message}");
            }
        }

        public static List<CheckoutItem> LoadCheckOutListToFile(List<LibraryItem> catalog)
        {
            List<CheckoutItem> checkoutList = new List<CheckoutItem>();

            if (!File.Exists(Utility.CheckoutFilePath))
            {
                Console.WriteLine("No previous checkout file found.");
                return checkoutList;
            }

            try
            {
                string[] lines = File.ReadAllLines(Utility.CheckoutFilePath);

                foreach (string line in lines)
                {
                    CheckoutItem item = ParseCheckOutItemFromLine(line, catalog);
                    if (item != null)
                    {
                        checkoutList.Add(item);
                    }
                }

                Console.WriteLine("Checkout list loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading checkout list: {ex.Message}");
            }

            return checkoutList;
        }

        public static int GetValidMenuChoice(int min, int max)
        {
            int choice;
            while (true)
            {
                Console.Write($"Enter your choice ({min}-{max}): ");
                string input = Console.ReadLine()?.Trim();

                if (int.TryParse(input, out choice))
                {
                    if (choice >= min && choice <= max)
                        return choice;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid choice. Please try again.");
                Console.ResetColor();
            }
        }

    internal static LibraryItem ParseLibraryItemFromLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;

        string[] parts = line.Split(',');
        if (parts.Length != 5) return null;

        try
        {
            int id = int.Parse(parts[0].Trim());
            string title = parts[1].Trim();
            string type = parts[2].Trim();
            double dailyLateFee = double.Parse(parts[3].Trim());
            int stock = int.Parse(parts[4].Trim());

            return new LibraryItem(id, title, type, dailyLateFee, stock);
        }
        catch
        {
            return null;
        }
    }


    internal static CheckoutItem ParseCheckOutItemFromLine(string line, List<LibraryItem> catalog)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            string[] parts = line.Split(',');
            if (parts.Length < 5) return null;

            try
            {
                int id = int.Parse(parts[0].Trim());
                string title = parts[1].Trim();
                string type = parts[2].Trim();
                DateTime checkoutDate = DateTime.Parse(parts[3].Trim());
                DateTime dueDate = DateTime.Parse(parts[4].Trim());

                // Look up the item in the catalog to get the correct DailyLateFee
                LibraryItem catalogItem = catalog.FirstOrDefault(i => i.Id == id);

                if (catalogItem == null)
                {
                    // If not found, fall back to creating a new LibraryItem
                    catalogItem = new LibraryItem(id, title, type, 0.25); // default fee
                }

                CheckoutItem checkoutItem = new CheckoutItem(catalogItem, checkoutDate);
                checkoutItem.DueDate = dueDate;

                // Optional ReturnedDate
                if (parts.Length >= 6 && !string.IsNullOrWhiteSpace(parts[5]))
                {
                    checkoutItem.ReturnedDate = DateTime.Parse(parts[5].Trim());
                }

                return checkoutItem;
            }
            catch
            {
                return null;
            }
        }
    }

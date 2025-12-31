using System.Xml.Serialization;

namespace biblioteca // Left off on line 
{
    /*
    ==== Important Notes ====
    This is a revamped version of the original Final Project Submission
    The removal potentially AI-derived code and practices was heavily prioritized, as such, every line was hand-typed.
    The old project has been used for reference for certain elements of this version, and functionality has been tailored to be as similar as possible
    
    */

    internal class LibraryItem
    {
        // Class-Object Properties. Can be retrieved and changed publicly
        public int Id { get; set; } // ID for each item
        public string Title { get; set; } // Title for each item
        public string Type { get; set; } // Type of item (i.e. Book, DVD, etc.)
        public double DailyLateFee { get; set; } // Daily late fee of each item
        public int Stock {  get; set; } // Stock of each item. Defaults to 1 in the constructor

        // Constructor. Must pass all Object Properties through to make the object
        public LibraryItem(int id, string title, string type, double dailyLateFee, int stock = 1)
        {
            Id = id;
            Title = title;
            Type = type;
            DailyLateFee = dailyLateFee;
            Stock = stock;
        }

        // Overriding ToString Function to return a formatted string for this class
        // If we don't do this, its going to return the namespace and class of the object instead
        public override string ToString()
        {
            return $"{Id} - {Title} - {Type} | Daily Late Fee: ${DailyLateFee:F2} | {Stock}";
        }
    }

    internal class CheckoutItem
    {
        // Class-Object Properties. Can be retrieved and changed publicly
        public LibraryItem Item { get; set; } // Establishes "Has A" relationship. Each checkout item must have a corresponding LibraryItem
        public DateTime CheckoutDate { get; set; } // The date an item was checked out from the system
        public DateTime DueDate { get; set; } // The date an item has to be returned before incurring a late fee
        public DateTime? DateReturned { get; set; } // The date an item was returned on. Can be null

        // Constructor. Must pass the LibraryItem and the Checkout Date through. Everything else is determined internally
        public CheckoutItem(LibraryItem item, DateTime checkoutDate)
        {
            Item = item;
            CheckoutDate = checkoutDate;
            DateReturned = null;
            // Internal logic for DueDate Property

            int loanPeriod;
            switch (Item.Type) // Determine loan period based on Item Type
            {
                case "DVD":
                    loanPeriod = 3;
                        break;
                case "Book":
                    loanPeriod = 7;
                    break;
                case "Audiobook":
                    loanPeriod = 5;
                    break;
                default: // Slight redundancy incase the item does not fit the predetermined types. This should never happen
                    loanPeriod = 7;
                    break;
            }
            DueDate = CheckoutDate.AddDays(loanPeriod); // Adds the loan perido to the Checkout Date to determine Due date
        }
        
        public int GetDaysLate()
        {
            // Ternary function to determine if there is a return date to use, or default to current time
            DateTime timeCheckedout = DateReturned.HasValue ? DateReturned.Value : DateTime.Now;
            int daysLate = (timeCheckedout -  DueDate).Days;
            return Math.Max(daysLate, 0); // Returns the days late or 0, whichever is higher
        }

        public double CalculateLateFee(int daysLate)
        {
            // Calculates the late fee. If it isn't late it returns 0, if it is late it multiplies the daily fee by the days late.
            if (daysLate <= 0) return 0;
            return daysLate * Item.DailyLateFee;
        }

        public string GetReceiptLine()
        {
            // Returns a formatted string of text for each line of the receipt 
            int dayslate = GetDaysLate();
            double fee = CalculateLateFee(dayslate);
            return $"{Item.Id} - {Item.Title} - {Item.Type} | Days Late: {dayslate} | Late Fee: ${fee:F2}";
        }

        public void MarkAsReturned(DateTime returnDate)
        {
            DateReturned = returnDate;
        }
    }

    internal class LibrarySystem // Main is located in this class
    {
        // Class of functions for handling the Library System and Menu Navigation
        static List<LibraryItem> catalog = new List<LibraryItem>(); // Establishes list of library items in our catalog

        static void DisplayMenu()
        {
            bool exitMenu = false;
            List<CheckoutItem> checkoutList = new List<CheckoutItem>(); // Establishes list of checkout items in our checkout list

            while (!exitMenu) // Keeps menu loop running until the variable is set true
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n ====== Welcome to the Library ======");
                Console.ResetColor();
                Console.WriteLine("1. View Catalog");
                Console.WriteLine("2. Add Library Item");
                Console.WriteLine("3. Check Out Item");
                Console.WriteLine("4. Return Item");
                Console.WriteLine("5. View my Checkout Receipt");
                Console.WriteLine("6. Save my Checkout List");
                Console.WriteLine("7. Load my Previous Checkout List");
                Console.WriteLine("8. Set Catalog Location");
                Console.WriteLine("9. Set Checkout List Location");
                Console.WriteLine("10. Exit");

                int choice = Utility.AskUserForChoice("Please Pick a Menu Option");
                if (1 > choice || choice > 10) // If int is out of range
                {
                    Console.WriteLine("\nInvalid Menu Option, Press any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                    continue; // Resets the loop to the beginning
                }

                switch (choice)
                {
                    case 1: // View Catalog
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        CatalogManagement.DisplayCatalog(catalog);
                        Console.ResetColor();
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    case 2: // Add Library Item
                        CatalogManagement.AddLibraryItem(catalog);
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    case 3: // Check Out Item
                        CheckoutLogic.CheckoutItem(checkoutList, catalog);
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    case 4: // Return Item
                        CheckoutLogic.ReturnItem(checkoutList);
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    case 5: // View Checkout Receipt
                        CheckoutLogic.DisplayCheckoutList(checkoutList);
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    case 6: // Save Checkout List
                        Utility.SaveCheckoutListToFile(checkoutList);
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    case 7: // Load Previous Checkout List
                        checkoutList = Utility.LoadCheckoutListFromFile(catalog);
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    case 8: // Set Catalog Location
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Utility.SaveCatalogPath();
                        // Reloads the catalog from the new file
                        catalog = CatalogManagement.LoadCatalogFromFile();

                        Console.ResetColor();
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    case 9: // Set Checkout List Location
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Utility.SaveCheckoutPath();
                        // Reloads the Checkout List from the new file
                        checkoutList = Utility.LoadCheckoutListFromFile(catalog);

                        Console.ResetColor();
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    case 10: // Exit
                        exitMenu = true;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Exiting the Library System. Goodbye!");
                        Console.ResetColor();
                        Console.WriteLine("\nPress any key to shut down...");
                        Console.ReadKey();
                        Console.Clear();
                        break;

                    default: // Backup switch incase of bad input.
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid choice. Please re-select.");
                        Console.ResetColor();
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        Console.Clear();
                        break;

                }

                

            }
        }

        static void Main()
        {
            catalog = CatalogManagement.LoadCatalogFromFile();
            // Loads the catalog at startup

            DisplayMenu();
            // Starts the menu loop
        }

    }

    internal class CatalogManagement
    {

        internal static bool ContainsId(List<LibraryItem> catalog, int id)
        {
            foreach (LibraryItem item in catalog) // Iterates over every item in the catalog
            {
                if (item.Id == id) // If it finds a matching Id it returns true
                {
                    return true;
                }
                else continue; // If not it tries the next iteration
            }
            return false; // If it runs out of iterations and hasn't found anything it returns false
        }

        // Function that loads the catalog from the file
        public static List<LibraryItem> LoadCatalogFromFile()
        {
            int sequenceCheck = 0;
            List<LibraryItem> catalogList = new List<LibraryItem>(); // Initializes new list to store the loaded items
        
            // Checks to make sure the file actually exists just incase
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
                    if (item == null)
                    {
                        continue;
                    }
                    if (item.Id <= sequenceCheck)
                    { // Very basic checker to validate that the id isnt less than or equal to the last one.
                        // If it is, it skips it and keeps going, and tells the user.
                        // If the catalog file is disorderly this would likely cause issues.
                        Console.WriteLine($"Duplicate ID found for {item.Id}. Skipping Duplicate.");
                        continue;
                    }
                    if (item != null)
                    {
                        catalogList.Add(item); // Adds item to the list 
                        sequenceCheck = item.Id; // Sets the id value to check against the next item in the sequence
                    }
                }

                Console.WriteLine("Catalog loaded successfully!");
                return catalogList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading catalog; {ex.Message}");
                return catalogList;
            }

        }

        public static void SaveCatalogToFile(List<LibraryItem> catalog)
        {

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Utility.CatalogFilePath)!);
                // Creates the directory if it doesnt already exist

                using (StreamWriter write = new StreamWriter(Utility.CatalogFilePath, append: false))
                { // Starts streamwriter instance for writing the goods to a file
                    foreach (var item in catalog)
                    {
                        write.WriteLine($"{item.Id},{item.Title},{item.Type},{item.DailyLateFee:F2},{item.Stock}");
                        // Writes the information
                    }
                }
                Console.WriteLine("Catalog saved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saaving catalog: {ex.Message}");
                // If something happens, tell the user the error
            }


        }
   
        internal static void AddLibraryItem(List<LibraryItem> catalog)
        {
            // Adds a new item to the catalog and prevents duplicate Ids
            int id = Utility.AskUserForChoice("Enter the new item ID: ");

            if (ContainsId(catalog, id)) // Adds functionality to check for duplicate ID and prevents it getting adding
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: An item with this ID already exists in the catalog.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("Enter the title: ");
            string title = Console.ReadLine().Trim();

            // Type-validation for items
            string type; // Establish Variable
            while (true)
            {
                Console.WriteLine("Enter item type (Book/DVD/Audiobook)");
                type = Console.ReadLine().Trim();

                if (type.Equals("Book", StringComparison.OrdinalIgnoreCase))
                {
                    type = "Book"; // Forces proper casing while saving, but should accept any case from user.
                    break;
                }

                if (type.Equals("DVD", StringComparison.OrdinalIgnoreCase))
                {
                    type = "DVD"; // Forces proper casing while saving, but should accept any case from user.
                    break;
                }

                if (type.Equals("Audiobook", StringComparison.OrdinalIgnoreCase))
                {
                    type = "Audiobook"; // Forces proper casing while saving, but should accept any case from user.
                    break;
                }

                // If no preset option is picked, default to invalid type and loop back.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid type! You must enter either 'Book', 'DVD', or 'Audiobook'.");
                Console.ResetColor();
            }

            Console.WriteLine("Enter the daily late fee (decimal):");
            double fee = double.Parse(Console.ReadLine().Trim());

            Console.WriteLine("Enter the initial item stock");
            int stock;
            while (!int.TryParse(Console.ReadLine()?.Trim(), out stock) || stock < 1)
                // ^ Keeps looping until the user puts in an integer of at least 1
                // Gives the parsed value to stock via the out feature if it can be parsed
                // Will loop if parsing fails or the integer is incorrect
            { 
                Console.ForegroundColor= ConsoleColor.Red;
                Console.WriteLine("Invalid input. Stock must be a positive integer.");
                Console.ResetColor();
            }

            LibraryItem newItem = new LibraryItem(id, title, type, fee, stock); // Makes new object based on provided data
            catalog.Add(newItem); // Adds to current viewed catalog so user doesnt need to reload
            SaveNewItemToCatalogFile(newItem); // Passes the data to saving function

        }

        static void SaveNewItemToCatalogFile(LibraryItem item)
        {
            // Saves the new item to the current catalog's file

            try
            { // Makes sure the target directory exists. 'dir' can be null so compiler doesn't read possible null error from using readlines
                string? dir = Path.GetDirectoryName(Utility.CatalogFilePath);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);  
                }

                Console.WriteLine($"Saving new catalog item to {Utility.CatalogFilePath}");

                using (StreamWriter writer = new StreamWriter(Utility.CatalogFilePath, append: true))
                { // New streamwriter instance set to append to the file
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
            Console.WriteLine("=-=-=-= Library Catalog =-=-=-=");
            foreach (LibraryItem item in catalog)
            {
                Console.WriteLine(item.ToString());
            }
            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
        }

        internal static LibraryItem FindItemById(int id, List<LibraryItem> catalog)
        {
            foreach (LibraryItem item in catalog) // Searching loop to find the matching Id
            {
                if (item.Id == id)
                {
                    return item;
                    // Semds the item back when found
                }
                else continue;
            }
            Console.WriteLine("No matching item found. ");
            return null; // Returns null incase no matching item is found. Shouldn't happen but just incase
        }

    }

    internal class CheckoutLogic
    {
        // Class that holds most functions handling checkout logic

        public static void CheckoutItem(List<CheckoutItem> checkoutList, List<LibraryItem> catalog)
        {
            Console.WriteLine("Enter the ID of the item you want to check out: ");
            int id = Utility.AskUserForChoice(""); // Calling the user choice function with a blank prompt

            LibraryItem item = CatalogManagement.FindItemById(id, catalog);
            if (item == null) // Gets item by its ID, if null tells user and ends
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Item not found in catalog.");
                Console.ResetColor();
                return;
            }

            if ((IsItemCheckedOut(id, checkoutList)) == true) // Checks for item already being checked out
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Item has already been checked out.");
                Console.ResetColor();
                return;
            }

            if (item.Stock <= 0) // Prevents checkouts on out of stock items
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("This item is out of stock.");
                Console.ResetColor();
                return;
            }

            CheckoutItem checkoutItem = new CheckoutItem(item, DateTime.Now); // Update Checkout Date
            checkoutList.Add(checkoutItem); // Adds to the checkout list
            item.Stock--; // Subtract Stock
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Successfully checked out : {item.Title}");
            Console.ResetColor();
        }

        public static void ReturnItem(List<CheckoutItem> checkoutList)
        {
            Console.WriteLine("Enter the ID of the item you want to return: ");
            int id = Utility.AskUserForChoice("");

            CheckoutItem itemToReturn = FindItemById(id, checkoutList);

            if (itemToReturn == null) // If function couldnt find the relevant item
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Item not found in current checkout list");
                Console.ResetColor();
                return;
            }

            if (itemToReturn.DateReturned.HasValue) // If the item has already been returned, stop it from being returned a second time
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("This item has already been returned");
                Console.ResetColor();
                return;
            }

            itemToReturn.MarkAsReturned(DateTime.Now); // Set return time to current time
            itemToReturn.Item.Stock++; // Increment stock

            Console.ForegroundColor= ConsoleColor.Green;
            Console.WriteLine($"Item returned: {itemToReturn.Item.Title}");
            Console.WriteLine($"Late Fee: ${itemToReturn.CalculateLateFee(itemToReturn.GetDaysLate())}");
            // Above prints the formatted line for the late fee
            Console.ResetColor();

        }

        public static void DisplayCheckoutList(List<CheckoutItem> checkoutList)
        {
            if (checkoutList.Count == 0)
            {
                Console.WriteLine("You have no items checked out");
                return;
            }

            Console.WriteLine("=-=-=-= My Checkout List =-=-=-=");
            foreach (var item in checkoutList)
            {
                string status = item.DateReturned.HasValue ? "Returned" : "Checked Out";
                Console.WriteLine($"{item.GetReceiptLine()} | Status: {status}");
            }
            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");


        }





        public static bool IsItemCheckedOut(int id, List<CheckoutItem> checkoutList)
        {
            CheckoutItem item = FindItemById(id, checkoutList); // Uses other function to get the item
            if (item == null) return false;
            if (item.DateReturned.HasValue == true) return true; // Checks for a return date, if it has one return true
            return false; // If there isnt a return date return false
        }

        private static CheckoutItem FindItemById(int id, List<CheckoutItem> catalog) 
        {// This is a tweaked instance of the function by the same name in the CatalogManagement class
            // It was reworked to allow for different class objects SPECIFICALLY for this class' functions
            foreach (CheckoutItem item in catalog) // Searching loop to find the matching Id
            {
                if (item.Item.Id == id)
                {
                    return item;
                    // Semds the item back when found
                }
                else continue;
            }
            return null; // Returns null incase no matching item is found. Shouldn't happen but just incase
        }


    }

    internal class Utility
    {
        // Object properties for the class to help handle retrieving necessary file paths.
        // Can only be set inside this class, but can be retrieved globally
        public static string CatalogFilePath { get; private set; } = "default_catalog.txt";

        public static string CheckoutFilePath { get; private set; } = "default_checkout.txt";


        public static void SaveCatalogPath()
        {
            Console.WriteLine("Please enter the full path for the catalog file or type DEFAULT");
            string userInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(userInput)) // Checks to prevent null/whitespace file paths, falls back on last usable path
            {
                Console.WriteLine($"Using last defined catalog path: {CatalogFilePath}");
                return;
            }

            if (Directory.Exists(userInput) || userInput.EndsWith(Path.DirectorySeparatorChar) || userInput.EndsWith(Path.AltDirectorySeparatorChar))
            {
                // If the directory exists, or the user input ends with a directory seperator, use that as the directory.
                CatalogFilePath = Path.Combine(userInput, "catalog.txt");
                // If only a folder path/directory is specified, the program will use it + a default file name
            }

            else
            {
                string fileName = Path.GetFileName(userInput);
                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                {
                    // If input has no extension, treat as a folder
                    CatalogFilePath = Path.Combine(userInput, "catalog.txt");
                }
                else
                {
                    // This should be a proper file path
                    CatalogFilePath = userInput;
                }
            }

            string? directory = Path.GetDirectoryName(CatalogFilePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory)) // Makes sure the directory isn't null or whitespace AND does not exist
            {
                Directory.CreateDirectory(directory); // Creates new directory if prior conditions are met
            }

            Console.WriteLine($"Catalog path set to: {CatalogFilePath}");
        }// Saves the file path used for the catalog
        public static void SaveCheckoutPath() // Functions exactly like the SaveCatalogPath function, just refitted for Checkout paths
        {
            Console.WriteLine("Please enter the full path for the checkout file or type DEFAULT");
            string userInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(userInput)) // Checks to prevent null/whitespace file paths, falls back on last usable path
            {
                Console.WriteLine($"Using last defined checkout path: {CheckoutFilePath}");
                return;
            }

            if (Directory.Exists(userInput) || userInput.EndsWith(Path.DirectorySeparatorChar) || userInput.EndsWith(Path.AltDirectorySeparatorChar))
            {
                // If the directory exists, or the user input ends with a directory seperator, use that as the directory.
                CheckoutFilePath = Path.Combine(userInput, "checkout.txt");
                // If only a folder path/directory is specified, the program will use it + a default file name
            }

            else
            {
                string fileName = Path.GetFileName(userInput);
                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                {
                    // If input has no extension, treat as a folder
                    CheckoutFilePath = Path.Combine(userInput, "checkout.txt");
                }
                else
                {
                    // This should be a proper file path
                    CheckoutFilePath = userInput;
                }
            }

            string? directory = Path.GetDirectoryName(CheckoutFilePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory)) // Makes sure the directory isn't null or whitespace AND does not exist
            {
                Directory.CreateDirectory(directory); // Creates new directory if prior conditions are met
            }

            Console.WriteLine($"Checkout path set to: {CheckoutFilePath}");
        }

        public static bool AskYesNo(string prompt)
        {
            // A function to ask a yes or no question and return a boolean based on user input
            bool userInput = false;
            while (true)
            {
                Console.WriteLine(prompt);
                string yesNo = Console.ReadLine().ToUpper().Trim(); // Helps format user input to something more consistent for checking
                if (yesNo == "YES" || yesNo == "Y") return true;
                else if (yesNo == "NO" || yesNo == "N") return false;
                else Console.WriteLine("Please enter a valid Yes or No input");
            }
        }

        public static int AskUserForChoice(string prompt)
        {
            // A function for returning an integer
            // Asks user to give int based on a prompt, and returns the int
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

        public static void SaveCheckoutListToFile(List<CheckoutItem> checkoutList)
        {
            try
            {
                string? directory = Path.GetDirectoryName(Utility.CheckoutFilePath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory); // Creates directory if it doesn't exist
                }

                Console.WriteLine($"Saving checkout list to: {Utility.CheckoutFilePath}");

                using (StreamWriter writer = new StreamWriter(Utility.CheckoutFilePath, append: false)) 
                { // Starts new streamwriter instance set to the checkout file path that overwrites instead of appending
                    foreach (var item in checkoutList)
                    {
                        string returnedDateString = item.DateReturned.HasValue ? item.DateReturned.Value.ToString("yyyy-MM-dd") : "";
                        // Ternary function to use the returned date if available, and if not leave it blank

                        string line = $"{item.Item.Id},{item.Item.Title},{item.Item.Type},{item.CheckoutDate:yyyy-MM-dd},{item.DueDate:yyyy-MM-dd},{returnedDateString}";
                        // Formats string input by comma-seperation before writing to file

                        writer.WriteLine(line); // Does the writing
                    }
                }
                Console.WriteLine("Checkout list saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving checkout list: {ex.Message}");
            }
        }


        public static List<CheckoutItem> LoadCheckoutListFromFile(List<LibraryItem> catalog)
        {
            List<CheckoutItem> checkoutList = new List<CheckoutItem>();
            // Initialized new list of checkout items

            if (!File.Exists(Utility.CheckoutFilePath))
            { // If the checkout file path doesnt exist, tell the user and return
                Console.WriteLine("No previous checkout file found.");
                return checkoutList;
            }

            try
            {
                string[] lines = File.ReadAllLines(Utility.CheckoutFilePath);
                // Takes all the lines in the file and makes a list out of them
                foreach (string line in lines) // Iterates over that list ^
                {
                    CheckoutItem item = ParseCheckoutItemFromLine(line, catalog);
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
        

        internal static LibraryItem ParseLibraryItemFromLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            string[] parts = line.Split(','); // Divides the lines by the comma seperator and stores those parts in a list
            if (parts.Length != 5) // If the information is missing or improperly formatted nothing is returned
            {
                return null;
            }

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

        internal static CheckoutItem ParseCheckoutItemFromLine(string line, List<LibraryItem> catalog)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            string[] parts = line.Split(",");
            if (parts.Length < 5)
            {
                return null;
            }

            try
            {
                int id = int.Parse(parts[0].Trim());
                string title = parts[1].Trim();
                string type = parts[2].Trim();
                DateTime checkoutDate = DateTime.Parse(parts[3].Trim());
                DateTime dueDate = DateTime.Parse(parts[4].Trim());


                LibraryItem catalogItem = null;

                foreach (LibraryItem item in catalog)
                {
                    if (item.Id == id)
                    {
                        catalogItem = item;
                        break;
                    }
                   if (catalogItem == null)
                    {
                        return null;
                        // If a checkout file references an item not in the catalog
                    }
                }

                CheckoutItem checkoutItem = new CheckoutItem(catalogItem, checkoutDate);
                checkoutItem.DueDate = dueDate;

                // Optionally adds the date returned if it is available.
                if (parts.Length >= 6 && !string.IsNullOrWhiteSpace(parts[5]))
                {
                    checkoutItem.DateReturned = DateTime.Parse(parts[5].Trim());
                }

                return checkoutItem;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Parsing Error: {ex.Message}");
                return null;
            }
        }

    }


}
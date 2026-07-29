using SmartFactorySimple;
using System;
using System.IO;

public static class AppFileNames
{
    public const string EmployeesFileName = "employees.txt";
    public const string OrdersFileName = "orders.txt";
    public const string MachinesFileName = "machines.txt";
    public const string ProductsFileName = "products.txt";
    public const string OperationsFileName = "operations.txt";

    public static string ResolvePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));
        }

        string currentDir = AppContext.BaseDirectory;
        string dataCandidate = Path.Combine(currentDir, "Data", fileName);
        if (File.Exists(dataCandidate))
        {
            return dataCandidate;
        }

        string candidate = Path.Combine(currentDir, fileName);
        if (File.Exists(candidate))
        {
            return candidate;
        }

        string projectRoot = FindProjectRoot(currentDir);
        if (!string.IsNullOrWhiteSpace(projectRoot))
        {
            string rootDataCandidate = Path.Combine(projectRoot, "Data", fileName);
            if (File.Exists(rootDataCandidate))
            {
                return rootDataCandidate;
            }

            string rootCandidate = Path.Combine(projectRoot, fileName);
            if (File.Exists(rootCandidate))
            {
                return rootCandidate;
            }

            Directory.CreateDirectory(Path.Combine(projectRoot, "Data"));
            return rootDataCandidate;
        }

        return candidate;
    }

    private static string FindProjectRoot(string startDirectory)
    {
        DirectoryInfo directory = new DirectoryInfo(startDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "SmartFactorySimple.csproj")) ||
                File.Exists(Path.Combine(directory.FullName, "SmartFactorySimple.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}

class Program
{
    static Factory fabrica = new Factory("TOYS R US");
    static Login.EmployeeCredential loggedInUser;
    static Login loginManager;

    static void Main()
    {
        // Authentication - Login required
        loginManager = new Login();
        loggedInUser = loginManager.LoginWithAttempts(3);

        if (loggedInUser == null)
        {
            return; // Exit if authentication fails
        }

        DateDemo();
        // Load persisted machines, products, and orders in a dependency-safe order.
        fabrica.LoadPersistentData();

        bool running = true;
        while (running)
        {
            Console.Clear();
            Console.WriteLine(Messages.FactoryHeader);
            Console.WriteLine(Messages.LoggedInAs(loggedInUser.Username, loggedInUser.Role));
            EmployeeRole rolCurent;
            if (!Enum.TryParse(loggedInUser.Role, out rolCurent))
            {
                Console.WriteLine(Messages.UnknownRoleInDatabase);
                return;
            }

            switch (rolCurent)
            {
                case EmployeeRole.Director:
                    running = MeniuDirector();
                    break;
                case EmployeeRole.ProductionManager:
                    running = MeniuProductionManager1();
                    break;
                case EmployeeRole.Engineer:
                    running = MeniuEngineer();
                    break;
                case EmployeeRole.Technician:
                    running = MeniuTechnician();
                    break;
                case EmployeeRole.MachineOperator:
                    running = MeniuMachineOperator();
                    break;
                case EmployeeRole.SalesAgent:
                    running = MeniuSalesAgent();
                    break;
                default:
                    Console.WriteLine(Messages.UnknownRole);
                    running = false;
                    break;
            }
        }

        Console.WriteLine(Messages.Goodbye);
        Console.WriteLine(Messages.SavingData);
        fabrica.SalveazaMasini();
        fabrica.SalveazaProduse();
    }



    static bool MeniuDirector()
    {
        Console.Clear();
        Console.WriteLine(Messages.LoggedInAs(loggedInUser.Username, loggedInUser.Role));
        fabrica.ShowMainMenuShareStatus();
        Console.WriteLine(Messages.MenuDirectorEmployees);
        Console.WriteLine(Messages.MenuDirectorMachines);
        Console.WriteLine(Messages.MenuDirectorProducts);
        Console.WriteLine(Messages.MenuDirectorSales);
        Console.WriteLine(Messages.MenuDirectorDashboard);
        Console.WriteLine(Messages.MenuDirectorLogs);
        Console.WriteLine(Messages.MenuDirectorListCompanyPublic);
        Console.WriteLine(Messages.MenuDirectorLogout);
        Console.WriteLine(Messages.MenuDirectorExit);
        Console.Write(Messages.MenuPromptChoose);
        string alegere = Console.ReadLine();

        switch (alegere)
        {
            case "1": MeniuAngajati(); break;
            case "2":
                fabrica.AfiseazaMasini();
                Console.WriteLine(Messages.MenuDirectorMachineView);
                PauseAndContinue();
                break;
            case "3":
                fabrica.AfiseazaProduse();
                Console.WriteLine(Messages.MenuDirectorProductView);
                PauseAndContinue();
                break;           
            case "4":
                Console.WriteLine(Messages.MenuDirectorSalesTitle);
                Console.WriteLine(Messages.MenuDirectorSalesReport);
                Console.WriteLine(Messages.MenuDirectorGeneralReport);
                string opt = Console.ReadLine();
                switch (opt)
                {
                    case "1": fabrica.AfiseazaRaportVanzari(); break;
                    case "2": fabrica.AfiseazaRaportGeneral(); break;
                    default: Console.WriteLine(Messages.MenuDirectorSalesOption); break;
                }
                PauseAndContinue();

                break;
            case "5":
                fabrica.AfiseazaDashboardGestionare();
                break;
            case "6": ShowOperationLogs(); PauseAndContinue(); break;
            case "7":
                fabrica.ListCompanyPubliclyFromConsole();
                PauseAndContinue();
                break;
            case "8": return Logout();
            case "0": return false;
            default: Console.WriteLine("Invalid option!"); break;
        }
        return true;
    }

    /*static bool MeniuProductionManager()
    {
        Console.WriteLine("1. Show all employees");
        Console.WriteLine("2. Machines");
        Console.WriteLine("3. Products");
        Console.WriteLine("4. Production");
        Console.WriteLine("5. General Report");
        Console.WriteLine("6. Log out");
        Console.WriteLine("0. Exit");
        Console.Write("Choose: ");
        string alegere = Console.ReadLine();

        switch (alegere)
        {
            case "1": fabrica.AfiseazaAngajati(); break;
            case "2": MeniuMasiniProductionManager(); break;
            case "3": MeniuProduse(); break;
            case "4": MeniuProductie(); break;
            case "5": fabrica.AfiseazaRaportGeneral(); break;
            case "6": return Logout();
            case "0": return false;
            default: Console.WriteLine("Invalid option!"); break;
        }
        return true;
    }
    */
    static bool MeniuProductionManager1()
    {
        Console.Clear();
        Console.WriteLine(Messages.LoggedInAs(loggedInUser.Username, loggedInUser.Role));
        fabrica.ShowMainMenuShareStatus();
        Console.WriteLine(Messages.MenuProductionEmployees);
        Console.WriteLine(Messages.MenuProductionMachines);
        Console.WriteLine(Messages.MenuProductionProducts);
        Console.WriteLine(Messages.MenuProductionProduction);
        Console.WriteLine(Messages.MenuProductionDashboard);
        Console.WriteLine(Messages.MenuProductionReport);
        Console.WriteLine(Messages.MenuProductionLogout);
        Console.WriteLine(Messages.MenuProductionExit);
        Console.Write(Messages.MenuPromptChoose);
        
        string alegere = Console.ReadLine();

        switch (alegere)
        {
            case "1": fabrica.AfiseazaAngajati(); PauseAndContinue(); break;
            case "2": MeniuMasiniProductionManager(); break;
            case "3": fabrica.AfiseazaProduse(); PauseAndContinue(); break;
            case "4": MeniuProductieManager(); break;
            case "5":
                fabrica.AfiseazaDashboardGestionare();
                break;
            case "6": fabrica.AfiseazaRaportGeneral(); PauseAndContinue(); break;
            case "7": return Logout();
            case "0": return false;
            default: Console.WriteLine("Invalid option!"); break;
        }
        return true;

    }
    static void MeniuProductieManager()
    {
        Console.Clear();
        Console.WriteLine(Messages.MenuProductionManagerTitle);
        Console.WriteLine(Messages.MenuProductionManagerCreateOrder);
        Console.WriteLine(Messages.MenuProductionManagerShowOrders);
        Console.WriteLine(Messages.MenuProductionManagerShowPriorityOrders);
        Console.Write(Messages.MenuPromptChoose);

        string alegere = Console.ReadLine();

        if (alegere == "1")
            CreazaComandaMan();
        else if (alegere == "2")
        {
            fabrica.AfiseazaComenzi();
            PauseAndContinue();
        }
        else if (alegere == "3")
        {
            fabrica.AfiseazaComenziSortedByPriority();
            PauseAndContinue();
        }

    }
    static void MeniuMasiniProductionManager()
    {
        Console.Clear();
        Console.WriteLine(Messages.MenuMachinesTitle);
        Console.WriteLine(Messages.MenuMachinesAdd);
        Console.WriteLine(Messages.MenuMachinesShow);
        //Console.WriteLine("3. Stop a machine");
        //Console.WriteLine("4. Start a machine");
        Console.Write(Messages.MenuPromptChoose);
        string alegere = Console.ReadLine();

        if (alegere == "1")
        {
            AdaugaMasina();
        }
        else if (alegere == "2")
        {
            fabrica.AfiseazaMasini();
        }
        /*else if (alegere == "3")                                                  
        {
            fabrica.AfiseazaMasini();
            Console.Write("Serial number for the machine you want to stop: ");
            string serial = Console.ReadLine();
            Machine m = fabrica.GasesteMasina(serial);
            if (m == null)
                Console.WriteLine("Machine doesn't exist!");
            else
                m.Stop();
        }
        else if (alegere == "4")
        {
            fabrica.AfiseazaMasini();
            Console.Write("Serial number for the machine you want to start: ");
            string serial = Console.ReadLine();
            Machine m = fabrica.GasesteMasina(serial);
            if (m == null)
                Console.WriteLine("Machine doesn't exist!");
            else
                m.Start();
        }
        */
    }
    static bool MeniuEngineer()
    {
        Console.Clear();
        Console.WriteLine(Messages.LoggedInAs(loggedInUser.Username, loggedInUser.Role));
        fabrica.ShowMainMenuShareStatus();
        Console.WriteLine(Messages.MenuEngineerTitle);
        Console.WriteLine(Messages.MenuEngineerMaintenance);
        Console.WriteLine(Messages.MenuEngineerHealth);
        Console.WriteLine(Messages.MenuEngineerLogout);
        Console.WriteLine(Messages.MenuEngineerExit);
        Console.Write(Messages.MenuPromptChoose);
        string alegere = Console.ReadLine();

        switch (alegere)
        {
            case "1": fabrica.AfiseazaMasini(); PauseAndContinue(); break;
            case "2": fabrica.AfiseazaMentenantaPredictiva(); PauseAndContinue(); break;
            case "3": fabrica.AfiseazaStareMasini(); PauseAndContinue(); break;
            case "4": return Logout();
            case "0": return false;
            default: Console.WriteLine("Invalid option!"); break;
        }
        return true;
    }
   
    static bool MeniuTechnician()
    {
        Console.Clear();
        Console.WriteLine(Messages.LoggedInAs(loggedInUser.Username, loggedInUser.Role));
        fabrica.ShowMainMenuShareStatus();
        Console.WriteLine(Messages.MenuTechnicianTitle);
        Console.WriteLine(Messages.MenuTechnicianShow);
        Console.WriteLine(Messages.MenuTechnicianRepair);
        Console.WriteLine(Messages.MenuTechnicianMaintenance);
        Console.WriteLine(Messages.MenuTechnicianHistory);
        Console.WriteLine(Messages.MenuTechnicianLogout);
        Console.WriteLine(Messages.MenuTechnicianExit);
        Console.Write(Messages.MenuPromptChoose);
        string alegere = Console.ReadLine();

        switch (alegere)
        {
            case "1":
                fabrica.AfiseazaMasini();
                PauseAndContinue();
                break;
            case "2":
                ReparaMasina();
                break;
            case "3":
                fabrica.AfiseazaMasiniInMentenanta();
                PauseAndContinue();
                break;
            case "4":
                AfiseazaIstoricReparatii();
                PauseAndContinue();
                break;
            case "5":
                return Logout();
            case "0":
                return false;
            default:
                Console.WriteLine(Messages.InvalidOption);
                break;
        }
        return true;
    }

    static bool MeniuMachineOperator()
    {
        Console.Clear();
        Console.WriteLine(Messages.LoggedInAs(loggedInUser.Username, loggedInUser.Role));
        fabrica.ShowMainMenuShareStatus();
        Console.WriteLine(Messages.MenuOperatorProduction);
        Console.WriteLine(Messages.MenuOperatorShowMachines);
        Console.WriteLine(Messages.MenuOperatorStop);
        Console.WriteLine(Messages.MenuOperatorStart);
        Console.WriteLine(Messages.MenuOperatorLogout);

        Console.WriteLine(Messages.MenuOperatorExit);
        Console.Write(Messages.MenuPromptChoose);
        string alegere = Console.ReadLine();

        switch (alegere)
        {
            case "1": MeniuProductie1(); break;
            case "2": fabrica.AfiseazaMasini(); PauseAndContinue(); break;
            case "3": StopMachine(); break;
            case "4": StartMachine(); break;
            case "5": return Logout();
            case "0": return false;
            default: Console.WriteLine(Messages.InvalidOption); break;
        }
        return true;
    }

    static void MeniuProductie1()
    {
        Console.Clear();
        Console.WriteLine(Messages.MenuProductionTitle);
        Console.WriteLine(Messages.MenuProductionExecuteOrder);
        Console.WriteLine(Messages.MenuProductionAutoOrder);
      
        Console.Write(Messages.MenuPromptProductionChoice);
        string alegere = Console.ReadLine();

         if (alegere == "1")
            ExecutaComanda();
        else if (alegere == "2")
            ExecutaComanaPrioritara();
    
    }

    static bool MeniuSalesAgent()
    {
        Console.Clear();
        Console.WriteLine(Messages.LoggedInAs(loggedInUser.Username, loggedInUser.Role));
        fabrica.ShowMainMenuShareStatus();
        Console.WriteLine(Messages.MenuSalesAgentTitle);
        Console.WriteLine(Messages.MenuSalesAgentSales);
        Console.WriteLine(Messages.MenuSalesAgentProducts);
        Console.WriteLine(Messages.MenuSalesAgentMaterials);
        Console.WriteLine(Messages.MenuSalesAgentLogout);
        Console.WriteLine(Messages.MenuSalesAgentExit);
        Console.Write(Messages.MenuPromptChoose);
        string alegere = Console.ReadLine();

        switch (alegere)
        {
            case "1": MeniuVanzari(); break;
            case "2": fabrica.AfiseazaProduse(); PauseAndContinue(); break;
            case "3": fabrica.AfiseazaStocMaterialePrime(); PauseAndContinue(); break; // Apelăm funcția din Factory
            case "4": return Logout();
            case "0": return false;
            default: Console.WriteLine("Invalid option!"); break;
        }
        return true;
    }
    static void StopMachine()
    {
        fabrica.AfiseazaMasini();
        Console.Write(Messages.PromptMachineSerialStop);
        string serial = Console.ReadLine();
        Machine m = fabrica.GasesteMasina(serial);
        if (m == null)
            Console.WriteLine(Messages.MachineDoesNotExist);
        else
            m.Stop();
    }
    static void StartMachine()
    {
        fabrica.AfiseazaMasini();
        Console.Write(Messages.PromptMachineSerialStart);
        string serial = Console.ReadLine();
        Machine m = fabrica.GasesteMasina(serial);
        if (m == null)
            Console.WriteLine(Messages.MachineDoesNotExist);
        else
            m.Start();
    }
    static void ShowOperationLogs()
    {
        Console.Clear();
        Console.WriteLine(Messages.OperationHistoryHeader);
        string[] entries = Logging.GetAllEntries();
        if (entries.Length == 0)
        {
            Console.WriteLine(Messages.NoOperationLogs);
        }
        else
        {
            foreach (string entry in entries)
            {
                Console.WriteLine(entry);
            }
        }
        Console.WriteLine(Messages.MenuOperationHistoryFooter);
    }

    static void PauseAndContinue()
    {
        Console.WriteLine();
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
    }

    // Logout and re-authenticate. Returns true to continue running, false to exit application.
    static bool Logout()
    {
        Console.WriteLine(Messages.LoggingOut);
        if (loggedInUser != null)
        {
            Logging.Log(loggedInUser.Username, Messages.UserLoggedOut);
        }

        loggedInUser = loginManager.LoginWithAttempts(3);
        if (loggedInUser == null)
        {
            Console.WriteLine(Messages.AuthenticationFailedExit);
            return false;
        }

        Console.WriteLine(Messages.SuccessfullyLoggedInAs(loggedInUser.Username, loggedInUser.Role));
        return true;
    }

    // ===== MENIU ANGAJATI =====

    static void MeniuAngajati()
    {
        Console.Clear();
        Console.WriteLine(Messages.MenuEmployeesTitle);
        Console.WriteLine(Messages.MenuEmployeesAdd);
        Console.WriteLine(Messages.MenuEmployeesShow);
        Console.WriteLine(Messages.MenuEmployeesDelete);
        Console.WriteLine(Messages.MenuEmployeesDuty);
        Console.Write(Messages.MenuPromptChoose);
        string alegere = Console.ReadLine();

        if (alegere == "1")
        {
            AdaugaAngajat();
        }
        else if (alegere == "2")
        {
            fabrica.AfiseazaAngajati();
        }
        else if (alegere == "3")
        {
            fabrica.AfiseazaAngajati();
            Console.Write(Messages.PromptEmployeeIdDelete);
            string id = Console.ReadLine();
            fabrica.StergeAngajat(id);
        }
        else if (alegere == "4")
        {
            fabrica.AfiseazaAngajati();
            Console.Write(Messages.PromptEmployeeIdInput);
            string id = Console.ReadLine();
            Employee ang = fabrica.GasesteAngajat(id);
            if (ang == null)
                Console.WriteLine(Messages.EmployeeDoesNotExist);
            else
                ang.PerformDuty();
        }
    }

    static void AdaugaAngajat()
    {
        Console.Write(Messages.PromptEmployeeId);
        string id = Console.ReadLine();
        if (fabrica.EmployeeIdExists(id))
        {
            Console.WriteLine(Messages.EmployeeAlreadyExists(id));
            return;
        }

        Console.Write(Messages.PromptEmployeeName);
        string nume = Console.ReadLine();
        Console.Write(Messages.PromptEmployeeSalary);
        decimal salariu = decimal.Parse(Console.ReadLine());

        Console.WriteLine(Messages.MenuEmployeeTypeTitle);
        Console.WriteLine(Messages.MenuEmployeeTypeDirector);
        Console.WriteLine(Messages.MenuEmployeeTypeProductionManager);
        Console.WriteLine(Messages.MenuEmployeeTypeEngineer);
        Console.WriteLine(Messages.MenuEmployeeTypeTechnician);
        Console.WriteLine(Messages.MenuEmployeeTypeMachineOperator);
        Console.WriteLine(Messages.MenuEmployeeTypeSalesAgent);
        Console.Write(Messages.MenuPromptChoose);
        string tip = Console.ReadLine();

        Employee angajat = null;
        string role = null;

        if (tip == "1")
        {
            angajat = new Director(id, nume, salariu, DateTime.Now);
            role = "Director";
        }
        else if (tip == "2")
        {
            angajat = new ProductionManager(id, nume, salariu, DateTime.Now);
            role = "ProductionManager";
        }
        else if (tip == "3")
        {
            angajat = new Engineer(id, nume, salariu, DateTime.Now);
            role = "Engineer";
        }
        else if (tip == "4")
        {
            angajat = new Technician(id, nume, salariu, DateTime.Now);
            role = "Technician";
        }
        else if (tip == "5")
        {
            angajat = new MachineOperator(id, nume, salariu, DateTime.Now);
            role = "MachineOperator";
        }
        else if (tip == "6")
        {
            angajat = new SalesAgent(id, nume, salariu, DateTime.Now);
            role = "SalesAgent";
        }
        else
        {
            Console.WriteLine("Invalid user!");
            return;
        }

        // Ask for login credentials
        Console.Write(Messages.PromptUsernameLogin);
        string username = Console.ReadLine();
        Console.Write(Messages.PromptPasswordLogin);
        string password = Console.ReadLine();

        if (fabrica.AdaugaAngajat(angajat))
        {
            // Save credentials to file
            if (loginManager.SaveEmployeeCredential(id, username, password, role))
            {
                Console.WriteLine(Messages.EmployeeAddedSuccessfully);
            }
            else
            {
                Console.WriteLine(Messages.EmployeeAddedCredentialsFailed);
            }
        }
    }
    static void AfiseazaIstoricReparatii()
    {
        Console.WriteLine(Messages.RepairHistoryHeader);
        Console.Write(Messages.MenuMachineRepairHistoryPrompt);
        string serialCautat = Console.ReadLine()?.Trim();

       
        string[] entries = Logging.GetAllEntries();
        bool found = false;

        if (entries != null && entries.Length > 0)
        {
            foreach (string entry in entries)
            {
               
                bool isRepairLog = entry.Contains("Repaired", StringComparison.OrdinalIgnoreCase);



                
                bool matchesSerial = string.IsNullOrEmpty(serialCautat) ||
                                     entry.Contains(serialCautat, StringComparison.OrdinalIgnoreCase);

                if (isRepairLog && matchesSerial)
                {
                    Console.WriteLine(entry);
                    found = true;
                }
            }
        }

        if (!found)
        {
            Console.WriteLine(Messages.NoRepairHistory);
        }
        Console.WriteLine(Messages.MenuRepairHistoryFooter);
    }

    // ===== MENIU MASINI =====

    static void MeniuMasini()
    {
        Console.Clear();
        Console.WriteLine(Messages.MenuMachinesTitle);
        Console.WriteLine(Messages.MenuMachinesAdd);
        Console.WriteLine(Messages.MenuMachinesShow);
        Console.WriteLine(Messages.MenuMachinesStop);
        Console.WriteLine(Messages.MenuMachinesRepair);
        Console.WriteLine(Messages.MenuMachinesStart);
        Console.WriteLine(Messages.MenuMachinesPredictiveMaintenance);
        Console.WriteLine(Messages.MenuMachinesDashboard);
        Console.WriteLine(Messages.MenuMachinesHealth);
        Console.Write(Messages.MenuPromptChoose);
        string alegere = Console.ReadLine();

        if (alegere == "1")
        {
            AdaugaMasina();
        }
        else if (alegere == "2")
        {
            fabrica.AfiseazaMasini();
        }
        else if (alegere == "3")
        {
            fabrica.AfiseazaMasini();
            Console.Write("Serial number for the machine you want to stop: ");
            string serial = Console.ReadLine();
            Machine m = fabrica.GasesteMasina(serial);
            if (m == null)
                Console.WriteLine("Machine doesn't exist!");
            else
                m.Stop();
        }
        else if (alegere == "4")
        {
            ReparaMasina();
        }
        else if (alegere == "5")
        {
            fabrica.AfiseazaMasini();
            Console.Write("Serial number for the machine you want to start: ");
            string serial = Console.ReadLine();
            Machine m = fabrica.GasesteMasina(serial);
            if (m == null)
                Console.WriteLine("Machine doesn't exist!");
            else
                m.Start();
        }
        else if (alegere == "6")
        {
            fabrica.AfiseazaMentenantaPredictiva();
        }
        else if (alegere == "7")
        {
            fabrica.AfiseazaDashboardEficienta();
        }
        else if (alegere == "8")
        {
            fabrica.AfiseazaStareMasini();
        }
    }

    static void AdaugaMasina()
    {
        Console.Write(Messages.PromptMachineSerial);
        string serial = Console.ReadLine();
        Console.Write(Messages.PromptMachineName);
        string nume = Console.ReadLine();

        Console.WriteLine(Messages.MenuMachineTypeTitle);
        Console.WriteLine(Messages.MenuMachineTypeSewing);
        Console.WriteLine(Messages.MenuMachineTypeCutting);
        Console.Write(Messages.MenuPromptChoose);
        string tip = Console.ReadLine();

        Machine masina = null;

        if (tip == "1")
            masina = new SewingMachine(serial, nume, DateTime.Now);
        else if (tip == "2")
            masina = new CuttingMachine(serial, nume, DateTime.Now);
        else
        {
            Console.WriteLine(Messages.InvalidUser);
            return;
        }

        Console.Write(Messages.PromptMachinePartAdd);
        string raspuns = Console.ReadLine();
        if (raspuns == Messages.ConfirmationYes)
        {
            Console.Write(Messages.PromptMachinePartName);
            string numePiesa = Console.ReadLine();
            Console.Write(Messages.PromptMachinePartType);
            string tipPiesa = Console.ReadLine();
            masina.AdaugaPiesa(new MachinePart(numePiesa, tipPiesa));
        }

        if (fabrica.AdaugaMasina(masina))
        {
            Console.WriteLine(Messages.EmployeeAddedSuccessfully);
            
            fabrica.SalveazaMasini();
        }
    }

    static void ReparaMasina()
    {
     
        string idTeh = loggedInUser.EmployeeId;

        fabrica.AfiseazaAngajati();

       
        Console.Write(Messages.PromptMachineSerialForEngineer);
        string idEng = Console.ReadLine();

        fabrica.AfiseazaMasini();
        Console.Write(Messages.PromptMachineSerialForRepair);
        string serial = Console.ReadLine();

       
        fabrica.ReparaMasina(idTeh, idEng, serial);
    }


    // ===== MENIU PRODUSE =====

    static void MeniuProduse()
    {
        Console.Clear();
        Console.WriteLine(Messages.MenuProductionProducts);
        Console.WriteLine(Messages.MenuProductsAdd);
        Console.WriteLine(Messages.MenuProductsShowAll);
        Console.WriteLine(Messages.MenuProductsAddStock);
        Console.WriteLine(Messages.MenuProductsSell);
        Console.WriteLine(Messages.MenuProductsDashboard);
        Console.WriteLine(Messages.MenuProductsInventoryAlerts);
        Console.Write(Messages.MenuPromptChoose);
        string alegere = Console.ReadLine();

        if (alegere == "1")
            AdaugaProdus();
        else if (alegere == "2")
        {
            fabrica.AfiseazaProduse();
            PauseAndContinue();
        }
        else if (alegere == "3")
            AdaugaStocProdus();
        else if (alegere == "4")
            VandeProdus();
        else if (alegere == "5")
            fabrica.AfiseazaDashboardEficienta();
        else if (alegere == "6")
            fabrica.AfiseazaAlerteInventar();
    }

    static void AdaugaStocProdus()
    {
        fabrica.AfiseazaProduse();
        Console.Write(Messages.PromptProductName);
        string nume = Console.ReadLine();

            Console.Write(Messages.PromptAmountToAdd);
            int cantitate = int.Parse(Console.ReadLine());

            fabrica.AdaugaStocProduse(nume, cantitate);
        }

        static void AdaugaProdus()
        {
            Console.Write(Messages.PromptEmployeeName);
            string nume = Console.ReadLine();
            Console.Write(Messages.PromptProductionCost);
            decimal productionCost = decimal.Parse(Console.ReadLine());
            Console.Write(Messages.PromptSellingPrice);
            decimal sellingPrice = decimal.Parse(Console.ReadLine());
            Console.Write(Messages.PromptInitialQuantity);
            int cantitate = int.Parse(Console.ReadLine());

            Console.WriteLine(Messages.MenuProductTypeTitle);
            Console.WriteLine(Messages.MenuProductTypeWoodenCubes);
            Console.WriteLine(Messages.MenuProductTypeTeddyBear);
            Console.WriteLine(Messages.MenuProductTypeFootball);
            Console.WriteLine(Messages.MenuProductTypeDoll);
            Console.WriteLine(Messages.MenuProductTypeFrisbee);

            Console.Write(Messages.PromptProductTypeChoice);
            string tip = Console.ReadLine();

            Product produs = null;

            if (tip == "1")
            {
                Console.Write("Size: ");
                string marime = Console.ReadLine();
                produs = new WoodenCubes(nume, productionCost, sellingPrice, cantitate, marime);
            }
            else if (tip == "2")
            {
                Console.Write("Size: ");
                string marime = Console.ReadLine();
                produs = new Doll(nume, productionCost, sellingPrice, cantitate, marime);
            }
            else if (tip == "3")
            {
                Console.Write("Size: ");
                string marime = Console.ReadLine();
                produs = new TedyBear(nume, productionCost, sellingPrice, cantitate, marime);
            }
            else if (tip == "4")
            {
                Console.Write("Size: ");
                string marime = Console.ReadLine();
                produs = new Ball(nume, productionCost, sellingPrice, cantitate, marime);
            }
            else if (tip == "5")
            {
                Console.Write("Size: ");
                string marime = Console.ReadLine();
                produs = new Frisbee(nume, productionCost, sellingPrice, cantitate, marime);
            }
            if (fabrica.AdaugaProdus(produs))
            {
                Console.WriteLine(Messages.ProductAddedSuccessfully);
                // Persist products immediately
                fabrica.SalveazaProduse();
            }
        }

        static void VandeProdus()
        {
            fabrica.AfiseazaAngajati();
            Console.Write(Messages.PromptSalesAgentId);
            string idAgent = Console.ReadLine();

            fabrica.AfiseazaProduse();
            Console.Write(Messages.PromptProductNameForSell);
            string numeProdus = Console.ReadLine();

            Console.Write(Messages.PromptSellingQuantity);
            int cantitate = int.Parse(Console.ReadLine());

            fabrica.VindeProdus(idAgent, numeProdus, cantitate);
        }

        // ===== MENIU PRODUCTIE =====

        static void MeniuProductie()
        {
            Console.Clear();
            Console.WriteLine(Messages.ProductionMenuTitle);
            Console.WriteLine(Messages.MenuProductionCreateOrder);
            Console.WriteLine(Messages.MenuProductionExecuteOrder);
            Console.WriteLine(Messages.MenuProductionAutoOrder);
            Console.WriteLine(Messages.MenuProductionShowOrders);
            Console.WriteLine(Messages.MenuProductionShowPriorityOrders);
            Console.Write(Messages.PromptProductTypeChoice);
            string alegere = Console.ReadLine();

            if (alegere == "1")
                CreazaComandaMan();
            else if (alegere == "2")
                ExecutaComanda();
            else if (alegere == "3")
                ExecutaComanaPrioritara();
            else if (alegere == "4")
            {
                fabrica.AfiseazaComenzi();
                PauseAndContinue();
            }
            else if (alegere == "5")
            {
                fabrica.AfiseazaComenziSortedByPriority();
                PauseAndContinue();
            }
        }

    static void CreazaComandaMan()
    {
       
        string idManager = loggedInUser.EmployeeId;

      
        fabrica.AfiseazaMasini();
        Console.Write(Messages.PromptMachineSerialOrder);
        string serial = Console.ReadLine();

        Console.WriteLine(Messages.ToysAvailableToManufacture);
        Console.Write(Messages.PromptProductNameToManufacture);
        string produs = Console.ReadLine();

        Console.Write(Messages.PromptTargetAmount);
        int cantitate = int.Parse(Console.ReadLine());

        Console.WriteLine(Messages.PromptPriority);
        string prio = Console.ReadLine();

        Priority prioritate;
        if (string.Equals(prio, "low", StringComparison.OrdinalIgnoreCase))
            prioritate = Priority.Low;
        else if (string.Equals(prio, "high", StringComparison.OrdinalIgnoreCase))
            prioritate = Priority.High;
        else
            prioritate = Priority.Medium;

        fabrica.CreazaComanda(idManager, serial, produs, cantitate, prioritate);
    }

    static void ExecutaComanda()
        {
            //fabrica.AfiseazaAngajati();
            //Console.Write("ID MachineOperator: ");
            string idOp = loggedInUser.EmployeeId;

            fabrica.AfiseazaComenzi();
            Console.Write(Messages.PromptOrderId);
            string idComanda = Console.ReadLine();

            Console.Write(Messages.PromptUnitsToProduce);
            int unitati = int.Parse(Console.ReadLine());

            fabrica.ExecutaComanda(idOp, idComanda, unitati);
        }

        static void ExecutaComanaPrioritara()
        {
            fabrica.AfiseazaAngajati();
            Console.Write(Messages.PromptMachineOperatorId);
            string idOp = Console.ReadLine();

            ProductionOrder nextOrder = fabrica.GetNextPriorityOrder(idOp);
            if (nextOrder == null)
            {
                Console.WriteLine(Messages.NoOrders);
                return;
            }

            Console.WriteLine(Messages.ProductionOrderHeader);
            nextOrder.Afiseaza();

            Console.Write(Messages.PromptUnitsToProduce);
            int unitati = int.Parse(Console.ReadLine());

            fabrica.ExecutaComanda(idOp, nextOrder.Id, unitati);
        }

        static void MeniuVanzari()
        {
            Console.Clear();
            Console.WriteLine(Messages.SalesMenuTitle);
            Console.WriteLine(Messages.MenuSalesSellProduct);
            Console.WriteLine(Messages.MenuSalesViewSalesReport);
            Console.WriteLine(Messages.MenuSalesViewGeneralReport);
            Console.Write(Messages.MenuPromptChoose);
            string alegere = Console.ReadLine();

            if (alegere == "1")
                VindeProdus();
            else if (alegere == "2")
            {
                fabrica.AfiseazaRaportVanzari();
                PauseAndContinue();
            }
            else if (alegere == "3")
            {
                fabrica.AfiseazaRaportGeneral();
                PauseAndContinue();
            }
        }

        static void VindeProdus()
        {
            //fabrica.AfiseazaAngajati();
            //Console.Write("ID SalesAgent: ");
            string idAgent = loggedInUser.EmployeeId;

        Employee ang = fabrica.GasesteAngajat(idAgent);
            if (ang == null || !(ang is SalesAgent))
            {
                Console.WriteLine(Messages.EmployeeDoesNotExist);
                return;
            }

            SalesAgent agent = (SalesAgent)ang;

            fabrica.AfiseazaProduse();
            Console.Write(Messages.PromptProductNameSell);
            string produsNume = Console.ReadLine();

            Product produs = fabrica.GasesteProdus(produsNume);
            if (produs == null)
            {
                Console.WriteLine(Messages.ProductNotFound);
                return;
            }

            Console.Write(Messages.PromptQuantitySell);
            int cantitate = int.Parse(Console.ReadLine());

            agent.VindeProdus(produs, cantitate, fabrica);
        }

        static void DateDemo()
        {
            fabrica.AdaugaAngajat(new Director("DIR001", "Alex Popescu", 8000, DateTime.Now.AddYears(-5)));
            fabrica.AdaugaAngajat(new ProductionManager("PM001", "Maria Ionescu", 5500, DateTime.Now.AddYears(-3)));
            fabrica.AdaugaAngajat(new Engineer("ENG001", "Ion Vasile", 5000, DateTime.Now.AddYears(-2)));
            fabrica.AdaugaAngajat(new Technician("TH001", "Andrei Marin", 4000, DateTime.Now.AddYears(-1)));
            fabrica.AdaugaAngajat(new MachineOperator("OP001", "Elena Dumitru", 3500, DateTime.Now.AddMonths(-8)));
            fabrica.AdaugaAngajat(new SalesAgent("SA001", "Ioana Radu", 3300, DateTime.Now.AddMonths(-4)));

            SewingMachine s1 = new SewingMachine("M001", "Juki Sewing", DateTime.Now.AddYears(-3));
            s1.AdaugaPiesa(new MachinePart("Industrial Needle", "Needle"));
            s1.AdaugaPiesa(new MachinePart("Polyester Thread", "Thread"));
            fabrica.AdaugaMasina(s1);

            CuttingMachine c1 = new CuttingMachine("M002", "Auto Cutter", DateTime.Now.AddYears(-2));
            c1.AdaugaPiesa(new MachinePart("Steel Blade", "Blade"));
            fabrica.AdaugaMasina(c1);

            fabrica.AdaugaProdus(new WoodenCubes("MagicBlocks", 15, 30, 3, "S"));
            fabrica.AdaugaProdus(new Doll("Barbie", 12, 50, 7, "S"));
            fabrica.AdaugaProdus(new TedyBear("Barnie", 20, 60, 15, "M"));
            fabrica.AdaugaProdus(new Ball("Football", 13, 50, 5, "Normal"));
            fabrica.AdaugaProdus(new Frisbee("OZN", 10, 25, 7, "S"));
        fabrica.InitializeazaMaterialeSiRetete();

        

        Console.WriteLine(Messages.DemoDataLoaded);
            Console.ReadLine();
        }
       
    }

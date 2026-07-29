using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SmartFactorySimple;

public class Factory
{
    public string Nume;

    
    private EmployeeRepository _employeeRepository = new EmployeeRepository();
    private MachineRepository _machineRepository = new MachineRepository();
    private ProductRepository _productRepository = new ProductRepository();
    private ProductionOrderRepository _orderRepository = new ProductionOrderRepository();

    private int _idComandaCounter = 1;
    private decimal _totalRevenue = 0;
    private int _totalSalesQuantity = 0;
    private bool _companyPubliclyListed = false;
    private decimal _publicSharePercentage = 0m;
    private int _issuedShares = 0;
    private decimal _sharePrice = 0m;
    private readonly Random _shareRandom = new Random();

    // Stocul pentru materialele prime (ex: "Lemn" -> 100 bucăți)
    private Dictionary<string, int> _stocMateriale = new Dictionary<string, int>();

    // Rețetele jucăriilor. Fiecare NumeJucărie are un dicționar cu Materiale și Cantități
    private Dictionary<string, Dictionary<string, int>> _retete = new Dictionary<string, Dictionary<string, int>>();

    public Factory(string nume)
    {
        Nume = nume;
    }

    public bool IsCompanyPublic => _companyPubliclyListed;
    public decimal PublicSharePercentage => _publicSharePercentage;
    public int IssuedShares => _issuedShares;
    public decimal SharePrice => _sharePrice;

    // File where orders are persisted. Search for orders.txt in app base dir and up to 4 parent folders.
    private string OrdersFilePath
    {
        get
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string candidate = AppFileNames.ResolvePath(AppFileNames.OrdersFileName);
            if (File.Exists(candidate)) return candidate;

            var dir = new DirectoryInfo(baseDir);
            for (int i = 0; i < 5 && dir != null; i++)
            {
                candidate = Path.Combine(dir.FullName, AppFileNames.OrdersFileName);
                if (File.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }

            // fallback to baseDir path (file may be created there)
            return AppFileNames.ResolvePath(AppFileNames.OrdersFileName);
        }
    }

    // Load orders from orders.txt. Expects lines in the format:
    // Id;MachineSerial;ProductName;Quantity;Priority;Status;CreatedBy;CreatedAt
    public void LoadOrdersFromFile()
    {
        try
        {
            if (!File.Exists(OrdersFilePath))
                return;

            string[] lines = File.ReadAllLines(OrdersFilePath);
            int maxIdSeen = 0;
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split(';');
                if (parts.Length < 8)
                {
                    Console.WriteLine(Messages.OrderLineWarning(line));
                    continue;
                }

                string id = parts[0].Trim();
                string machineSerial = parts[1].Trim();
                string productName = parts[2].Trim();
                if (!int.TryParse(parts[3].Trim(), out int qty))
                    qty = 0;

                if (!Enum.TryParse(parts[4].Trim(), true, out Priority prioritate))
                    prioritate = Priority.Medium;

                if (!Enum.TryParse(parts[5].Trim(), true, out ProductionOrderStatus status))
                    status = ProductionOrderStatus.Created;

                string createdBy = parts[6].Trim();
                DateTime createdAt = DateTime.Now;
                DateTime.TryParse(parts[7].Trim(), out createdAt);

                Machine masina = GasesteMasina(machineSerial);
                Employee emp = GasesteAngajat(createdBy);
                ProductionManager manager = emp as ProductionManager;

                if (masina == null || manager == null)
                {
                    // can't construct a valid order without machine and manager; skip
                    Console.WriteLine(Messages.SkippingOrder(id));
                    continue;
                }

                // if order already exists, update its properties, otherwise create new
                var existing = _orderRepository.FindById(id);
                if (existing != null)
                {
                    existing.Masina = masina;
                    existing.NumeProdus = productName;
                    existing.CantitateTarget = qty;
                    existing.Prioritate = prioritate;
                    existing.Status = status;
                    existing.DataCrearii = createdAt;
                }
                else
                {
                    var order = new ProductionOrder(id, masina, manager, productName, qty, prioritate);
                    order.Status = status;
                    order.CantitateProdusa = 0; // we don't persist produced amount in file currently
                    order.DataCrearii = createdAt;

                    _orderRepository.Add(order);
                }
                // track numeric suffix of ORD... ids so we can continue numbering
                if (id.StartsWith("ORD", StringComparison.OrdinalIgnoreCase))
                {
                    string numPart = id.Substring(3);
                    if (int.TryParse(numPart, out int parsed))
                    {
                        if (parsed > maxIdSeen) maxIdSeen = parsed;
                    }
                }
            }

            // ensure next generated id is higher than any existing one
            if (maxIdSeen > 0)
            {
                _idComandaCounter = maxIdSeen + 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(Messages.LoadOrderFailed(ex.Message));
        }
    }

    // Persist all orders to orders.txt (overwrites file)
    public void SaveOrdersToFile()
    {
        try
        {
            var orders = _orderRepository.GetAll();
            List<string> lines = new List<string>();
            lines.Add("# Production Orders");
            lines.Add("# Format: Id;MachineSerial;ProductName;Quantity;Priority;Status;CreatedBy;CreatedAt");
            foreach (var o in orders)
            {
                string createdBy = o.CreatDe != null ? o.CreatDe.Id : "";
                string createdAt = o.DataCrearii.ToString("s");
                string line = string.Join(";", o.Id, o.Masina?.SerialNumber ?? "", o.NumeProdus, o.CantitateTarget.ToString(), o.Prioritate.ToString(), o.Status.ToString(), createdBy, createdAt);
                lines.Add(line);
            }

            File.WriteAllLines(OrdersFilePath, lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine(Messages.SaveOrderFailed(ex.Message));
        }
    }

    public void InitializeazaMaterialeSiRetete()
    {
        // 1. Definim stocul inițial de materiale (poți adăuga o metodă să cumperi materiale mai târziu)
        _stocMateriale["Lemn"] = 500;
        _stocMateriale["Plastic"] = 500;
        _stocMateriale["Lana"] = 300;
        _stocMateriale["Piele"] = 200;

        // 2. Definim rețeta pentru fiecare produs (ce consumă o singură unitate din acea jucărie)
        _retete["MagicBlocks"] = new Dictionary<string, int> { { "Lemn", 2 }, { "Plastic", 1 } };
        _retete["Barbie"] = new Dictionary<string, int> { { "Plastic", 2 }, { "Lana", 1 } };
        _retete["Barnie"] = new Dictionary<string, int> { { "Lana", 3 } };
        _retete["Football"] = new Dictionary<string, int> { { "Piele", 3 }, { "Plastic", 1 } };
        _retete["OZN"] = new Dictionary<string, int> { { "Plastic", 3 } };
    }

    // O funcție ajutătoare pentru a vedea ce materiale mai ai
    public void AfiseazaStocMaterialePrime()
    {
        Console.WriteLine(Messages.RawMaterialsStockHeader);
        foreach (var material in _stocMateriale)
        {
            Console.WriteLine(Messages.InventoryMaterialLine(material.Key, material.Value));
        }
    }

    public bool AdaugaAngajat(Employee angajat)
    {
        bool added = _employeeRepository.Add(angajat);
        if (added)
        {
            Logging.Log(angajat.Id, $"Employee added: {angajat.Nume}");
        }
        return added;
    }

    public bool EmployeeIdExists(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        return _employeeRepository.ExistsById(id);
    }

    public void AfiseazaAngajati()
    {
        _employeeRepository.DisplayAll();
    }

    public Employee GasesteAngajat(string id)
    {
        return _employeeRepository.FindById(id);
    }

    public bool StergeAngajat(string id)
    {
        if (_employeeRepository.RemoveById(id))
        {
            Console.WriteLine(Messages.EmployeeDeletedSuccessfully);
            ApplyEmployeeRemovalImpact();
            Logging.Log(id, $"Employee removed: {id}");
            return true;
        }
        else
        {
            Console.WriteLine(Messages.EmployeeDoesNotExist);
            return false;
        }
    }


    public bool AdaugaMasina(Machine masina)
    {
        return _machineRepository.Add(masina);
    }

    public void AfiseazaMasini()
    {
        _machineRepository.DisplayAll();
    }

    public Machine GasesteMasina(string serial)
    {
        return _machineRepository.FindBySerialNumber(serial);
    }



    public bool AdaugaProdus(Product produs)
    {
        _productRepository.Add(produs);
        return true;
    }

    public void AfiseazaProduse()
    {
        _productRepository.DisplayAll();
    }

    public Product GasesteProdus(string nume)
    {
        return _productRepository.FindByName(nume);
    }



    public void CreazaComanda(string idManager, string serialMasina,
                              string produs, int cantitate, Priority prioritate)
    {
        Employee angajat = GasesteAngajat(idManager);
        if (angajat == null)
        {
            Console.WriteLine(Messages.EmployeeDoesNotExist);
            return;
        }

        if (!(angajat is ProductionManager))
        {
            Console.WriteLine(string.Format(Messages.EmployeeRoleMismatch(angajat.Nume, "ProductionManager")));
            return;
        }

        ProductionManager manager = (ProductionManager)angajat;

        Machine masina = GasesteMasina(serialMasina);
        if (masina == null)
        {
            Console.WriteLine(Messages.MachineDoesNotExist);
            return;
        }

        string idComanda = "ORD" + _idComandaCounter;
        _idComandaCounter++;
        // Create order with priority, add to repository, log and persist
        ProductionOrder comanda = manager.CreazaComanda(idComanda, masina, produs, cantitate, prioritate);
        _orderRepository.Add(comanda);
        Logging.Log(idManager, $"Order created: {idComanda} ({produs}) qty={cantitate} priority={prioritate}");
        SaveOrdersToFile();
    }
    public void AfiseazaMasiniInMentenanta()
    {
        // Preluăm toate mașinile din repository
        List<Machine> machines = _machineRepository.GetAll();

        // Filtrăm doar mașinile care au condiția "Critical"
        var masiniInMentenanta = machines.Where(m => m.Conditie.ToString() == "Critical").ToList();

        if (masiniInMentenanta.Count == 0)
        {
            Console.WriteLine(Messages.NoMachinesInMaintenance);
            return;
        }

        Console.WriteLine(Messages.MachinesInMaintenanceHeader);
        foreach (var machine in masiniInMentenanta)
        {
            Console.WriteLine($"{machine.SerialNumber} - {machine.Nume} | Condition: {machine.Conditie}");
        }
    }
    public void ExecutaComanda(string idOperator, string idComanda, int unitati)
    {
        Employee angajat = GasesteAngajat(idOperator);
        if (angajat == null) { Console.WriteLine(Messages.EmployeeDoesNotExist); return; }
        if (!(angajat is MachineOperator)) { Console.WriteLine(string.Format(Messages.EmployeeRoleMismatch(angajat.Nume, "MachineOperator"))); return; }

        MachineOperator op = (MachineOperator)angajat;
        ProductionOrder comanda = _orderRepository.FindById(idComanda);

        if (comanda == null) { Console.WriteLine(Messages.OrderDoesNotExist); return; }

        // --- INCEPUT LOGICĂ REȚETĂ ȘI MATERIALE ---
        string numeProdus = comanda.NumeProdus;

        if (_retete.ContainsKey(numeProdus))
        {
            var reteta = _retete[numeProdus];

            // 1. Verificăm mai întâi dacă avem destule materiale în stoc
            foreach (var ingredient in reteta)
            {
                string numeMaterial = ingredient.Key;
                int cantitateNecesarPentruTotal = ingredient.Value * unitati;

                if (!_stocMateriale.ContainsKey(numeMaterial) || _stocMateriale[numeMaterial] < cantitateNecesarPentruTotal)
                {
                    int stocCurent = _stocMateriale.ContainsKey(numeMaterial) ? _stocMateriale[numeMaterial] : 0;
                    Console.WriteLine(Messages.InsufficientMaterials(numeMaterial, cantitateNecesarPentruTotal, stocCurent));
                    return; // Oprim execuția comenzii pentru că nu avem materiale
                }
            }

            // 2. Dacă a trecut de verificarea de mai sus, înseamnă că avem materiale. Le scădem din stoc!
            foreach (var ingredient in reteta)
            {
                string numeMaterial = ingredient.Key;
                int cantitateNecesarPentruTotal = ingredient.Value * unitati;
                _stocMateriale[numeMaterial] -= cantitateNecesarPentruTotal;
            }
            Console.WriteLine(Messages.RawMaterialsConsumed(numeProdus, unitati));
        }
        else
        {
            Console.WriteLine(Messages.RecipeMissing(numeProdus));
        }
        // --- SFÂRȘIT LOGICĂ MATERIALE ---

        op.Opereaza(comanda.Masina);

        if (comanda.Masina.Status == MachineStatus.Running)
        {
            comanda.InregistreazaProductie(unitati);

            Product produs = GasesteProdus(comanda.NumeProdus);
            if (produs != null)
            {
                produs.AdaugaStoc(unitati);
                Console.WriteLine(Messages.NewStockAdded(comanda.NumeProdus, unitati));
            }

            Logging.Log(idOperator, $"Produced {unitati} units for order {idComanda} ({comanda.NumeProdus})");
        }
    }

    public void ReparaMasina(string idTehnician, string idEngineer, string serial)
    {
        Employee a1 = GasesteAngajat(idTehnician);
        Employee a2 = GasesteAngajat(idEngineer);

        if (a1 == null || a2 == null)
        {
            Console.WriteLine(Messages.EmployeeDoesNotExist);
            return;
        }

        if (a1 is not Technician)
        {
            Console.WriteLine(string.Format(Messages.EmployeeRoleMismatch(a1.Nume, "a Technician")));
            return;
        }

        if (a2 is not Engineer)
        {
            Console.WriteLine(string.Format(Messages.EmployeeRoleMismatch(a2.Nume, "an Engineer")));
            return;
        }

        Technician teh = (Technician)a1;
        Engineer eng = (Engineer)a2;

        Machine masina = GasesteMasina(serial);
        if (masina == null)
        {
            Console.WriteLine(Messages.MachineDoesNotExist);
            return;
        }

        if (masina.Status == MachineStatus.Running)
        {
            Console.WriteLine(Messages.StopCarBeforeRepair);
            return;
        }

        eng.Inspecteaza(masina);
        teh.Repara(masina);
    }

    public void AdaugaStocProduse(string numeProdus, int cantitate)
    {
        Product produs = GasesteProdus(numeProdus);
        if (produs == null)
        {
            Console.WriteLine(Messages.NoSuchProduct);
            return;
        }
        produs.AdaugaStoc(cantitate);
        Console.WriteLine(Messages.NewStockAdded(numeProdus, cantitate));
    }

    public void VindeProdus(string idAgent, string numeProdus, int cantitate)
    {
        Employee angajat = GasesteAngajat(idAgent);
        if (angajat == null)
        {
            Console.WriteLine(Messages.EmployeeDoesNotExist);
            return;
        }

        if (!(angajat is SalesAgent))
        {
            Console.WriteLine(string.Format(Messages.EmployeeRoleMismatch(angajat.Nume, "a SalesAgent")));
            return;
        }

        SalesAgent agent = (SalesAgent)angajat;

        Product produs = GasesteProdus(numeProdus);
        if (produs == null)
        {
            Console.WriteLine(Messages.ProductNotFound);
            return;
        }

        agent.VindeProdus(produs, cantitate, this);
    }

    

    public void AfiseazaRaportGeneral()
    {
        Console.WriteLine(string.Format(Messages.GeneralReportHeader, Nume));
        Console.WriteLine(string.Format(Messages.GeneralReportEmployees, _employeeRepository.Count));
        Console.WriteLine(string.Format(Messages.GeneralReportMachines, _machineRepository.Count));
        Console.WriteLine(string.Format(Messages.GeneralReportProducts, _productRepository.Count));
        Console.WriteLine(string.Format(Messages.GeneralReportOrders, _orderRepository.Count));
        Console.WriteLine(string.Format(Messages.GeneralReportRevenue, _totalRevenue));
        Console.WriteLine(string.Format(Messages.GeneralReportUnitsSold, _totalSalesQuantity));
        Console.WriteLine(string.Format(Messages.GeneralReportMaintenance, GetMachinesRequiringMaintenance(7).Count));
        Console.WriteLine(string.Format(Messages.GeneralReportLowStock, GetLowStockProducts().Count));
        if (_companyPubliclyListed)
        {
            Console.WriteLine(string.Format(Messages.CompanyValuation, GetCompanyValuation()));
        }
        Console.WriteLine(Messages.EmptyLine);
    }

    

    public void RecordSale(string productName, int quantity, decimal unitPrice)
    {
        decimal saleAmount = quantity * unitPrice;
        _totalRevenue += saleAmount;
        _totalSalesQuantity += quantity;

        Product p = GasesteProdus(productName);
        if (p != null)
        {
            p.VindeStoc(quantity);
            Console.WriteLine(Messages.SaleRecorded(productName, quantity, saleAmount));
            DisplayInventoryAlert(p);
            ApplySaleImpact();
        }
    }

    public decimal GetTotalRevenue()
    {
        return _totalRevenue;
    }

    public int GetTotalSalesQuantity()
    {
        return _totalSalesQuantity;
    }

    public decimal GetCompanyValuation()
    {
        if (!_companyPubliclyListed || _publicSharePercentage <= 0m)
        {
            return 0m;
        }

        decimal publicValue = _sharePrice * _issuedShares;
        return publicValue / (_publicSharePercentage / 100m);
    }

    public decimal CalculateProfit()
    {
        decimal totalCost = _productRepository
            .GetAll()
            .Sum(product => product.ProductionCost * (1000 - product.Cantitate));

        return _totalRevenue - totalCost;
    }
//
    public bool ListCompanyPublicly(decimal percentagePublic, int sharesIssued, decimal sharePrice)
    {
        if (_companyPubliclyListed)
        {
            Console.WriteLine(Messages.CompanyAlreadyPublic);
            return false;
        }

        if (percentagePublic <= 0m || percentagePublic > 100m)
        {
            Console.WriteLine(Messages.InvalidPublicSharePercentage);
            return false;
        }

        if (sharesIssued <= 0)
        {
            Console.WriteLine(Messages.InvalidShareCount);
            return false;
        }

        if (sharePrice <= 0m)
        {
            Console.WriteLine(Messages.InvalidSharePrice);
            return false;
        }

        _companyPubliclyListed = true;
        _publicSharePercentage = percentagePublic;
        _issuedShares = sharesIssued;
        _sharePrice = sharePrice;
        Console.WriteLine(string.Format(Messages.CompanyListedPublicly));
        Console.WriteLine(string.Format(Messages.PublicCompanySummary, _publicSharePercentage, _issuedShares, _sharePrice));
        return true;
    }

    public bool ListCompanyPubliclyFromConsole()
    {
        if (_companyPubliclyListed)
        {
            Console.WriteLine(Messages.CompanyAlreadyPublic);
            return false;
        }

        Console.Write(Messages.PublicSharePercentagePrompt);
        if (!decimal.TryParse(Console.ReadLine(), out decimal percentagePublic))
        {
            Console.WriteLine(Messages.InvalidPublicSharePercentage);
            return false;
        }

        Console.Write(Messages.IssuedSharesPrompt);
        if (!int.TryParse(Console.ReadLine(), out int sharesIssued))
        {
            Console.WriteLine(Messages.InvalidShareCount);
            return false;
        }

        Console.Write(Messages.SharePricePrompt);
        if (!decimal.TryParse(Console.ReadLine(), out decimal sharePrice))
        {
            Console.WriteLine(Messages.InvalidSharePrice);
            return false;
        }

        return ListCompanyPublicly(percentagePublic, sharesIssued, sharePrice);
    }

    public decimal ApplyMenuReturnFluctuation()
    {
        if (!_companyPubliclyListed)
        {
            return 0m;
        }

        int changePercent = _shareRandom.Next(-4, 5);
        decimal previousPrice = _sharePrice;
        decimal changeAmount = _sharePrice * changePercent / 100m;
        _sharePrice += changeAmount;
        if (_sharePrice < 0m)
        {
            _sharePrice = 0m;
        }

        Console.WriteLine(string.Format(Messages.SharePriceUpdated, _sharePrice, _sharePrice - previousPrice));
        return _sharePrice - previousPrice;
    }

    public decimal ApplySaleImpact()
    {
        if (!_companyPubliclyListed)
        {
            return 0m;
        }

        decimal previousPrice = _sharePrice;
        decimal changeAmount = _sharePrice * 0.15m;
        _sharePrice += changeAmount;
        Console.WriteLine(string.Format(Messages.SharePriceUpdated, _sharePrice, _sharePrice - previousPrice));
        return _sharePrice - previousPrice;
    }

    public decimal ApplyEmployeeRemovalImpact()
    {
        if (!_companyPubliclyListed)
        {
            return 0m;
        }

        decimal previousPrice = _sharePrice;
        int changePercent = _shareRandom.Next(8, 11);
        decimal changeAmount = _sharePrice * changePercent / 100m;
        _sharePrice -= changeAmount;
        if (_sharePrice < 0m)
        {
            _sharePrice = 0m;
        }

        Console.WriteLine(string.Format(Messages.SharePriceUpdated, _sharePrice, _sharePrice - previousPrice));
        return _sharePrice - previousPrice;
    }

    public void ShowMainMenuShareStatus()
    {
        if (!_companyPubliclyListed)
        {
            return;
        }

        ApplyMenuReturnFluctuation();
    }

    public List<Machine> GetMachinesRequiringMaintenance(int daysAhead = 7)
    {
        return _machineRepository
            .GetAll()
            .Where(machine => machine.EstimateDaysUntilMaintenance() <= daysAhead)
            .ToList();
    }
    public void IncarcaMasini()
    {
        _machineRepository.LoadMachines();
    }

    public void IncarcaProduse()
    {
        _productRepository.LoadProducts();
    }

    public void LoadPersistentData()
    {
        IncarcaMasini();
        IncarcaProduse();
        LoadOrdersFromFile();
    }

    public ProductionOrder GetOrderById(string id)
    {
        return _orderRepository.FindById(id);
    }

    public void SalveazaMasini()
    {
        _machineRepository.SaveAllMachines();
    }

    public void SalveazaProduse()
    {
        _productRepository.SaveAllProducts();
    }

    public void AfiseazaMentenantaPredictiva(int daysAhead = 7)
    {
        List<Machine> machines = GetMachinesRequiringMaintenance(daysAhead);
        Console.WriteLine(Messages.PredictiveMaintenanceHeader);

        if (machines.Count == 0)
        {
            Console.WriteLine(string.Format(Messages.NoMaintenanceInNextDays, daysAhead));
            return;
        }

        machines.ForEach(machine => Console.WriteLine(
            $"{machine.SerialNumber} - {machine.Nume}: maintenance due in {machine.EstimateDaysUntilMaintenance()} day(s)."));
    }

    public void AfiseazaDashboardEficienta() 
    {
        List<Machine> machines = _machineRepository.GetAll();
        Console.WriteLine(Messages.ProductionEfficiencyDashboardHeader);

        if (machines.Count == 0)
        {
            Console.WriteLine(Messages.NoMachines);
            return;
        }

        machines.ForEach(machine => Console.WriteLine(
            $"{machine.SerialNumber} - {machine.Nume}: {machine.CalculateEfficiencyPercentage():F2}% efficiency, {machine.ProductionCycles} production cycle(s)."));
        Console.WriteLine(string.Format(Messages.AverageEfficiency, machines.Average(machine => machine.CalculateEfficiencyPercentage())));
    }

    public void AfiseazaStareMasini()
    {
        List<Machine> machines = _machineRepository.GetAll();
        Console.WriteLine(Messages.MachineHealthMonitoringHeader);

        if (machines.Count == 0)
        {
            Console.WriteLine(Messages.NoMachines);
            return;
        }

        machines.ForEach(machine => Console.WriteLine(
            $"{machine.SerialNumber} - {machine.Nume} | {machine.Conditie} | {machine.GetHealthAlert()}"));
    }

    public List<Product> GetLowStockProducts(int threshold = 5)
    {
        return _productRepository
            .GetAll()
            .Where(product => product.Cantitate <= threshold)
            .ToList();
    }

    public void AfiseazaAlerteInventar(int threshold = 5)
    {
        Console.WriteLine(Messages.InventoryAlertsHeader);
        List<Product> products = GetLowStockProducts(threshold);

        if (products.Count == 0)
        {
            Console.WriteLine(string.Format(Messages.InventoryAboveThreshold, threshold));
            return;
        }

        products.ForEach(product => DisplayInventoryAlert(product, threshold));
    }

    public void AfiseazaDashboardGestionare(int threshold = 5)
    {
        var lowStockProducts = GetLowStockProducts(threshold);
        var activeOrders = _orderRepository.GetAllActive();
        var machines = _machineRepository.GetAll();
        var maintenanceMachines = GetMachinesRequiringMaintenance(7);
        var rawMaterials = _stocMateriale.OrderBy(kv => kv.Key).ToList();

        Console.WriteLine(Messages.ManagementDashboardHeader);
        Console.WriteLine(string.Format(Messages.ManagementDashboardSummary, Nume, _productRepository.Count, lowStockProducts.Count, activeOrders.Count, maintenanceMachines.Count));

        Console.WriteLine(Messages.ManagementDashboardInventoryHeader);
        if (_productRepository.Count == 0)
        {
            Console.WriteLine(Messages.ManagementDashboardNoProducts);
        }
        else
        {
            foreach (var product in _productRepository.GetAll().OrderBy(p => p.Nume))
            {
                Console.WriteLine(product.Cantitate <= threshold
                    ? $"- {product.Nume}: {product.Cantitate} units (LOW)"
                    : $"- {product.Nume}: {product.Cantitate} units");
            }
        }

        Console.WriteLine(Messages.ManagementDashboardRawMaterialsHeader);
        if (rawMaterials.Count == 0)
        {
            Console.WriteLine(Messages.ManagementDashboardNoRawMaterials);
        }
        else
        {
            foreach (var material in rawMaterials)
            {
                Console.WriteLine($"- {material.Key}: {material.Value} units");
            }
        }

        Console.WriteLine(Messages.ManagementDashboardLowStockHeader);
        if (lowStockProducts.Count == 0)
        {
            Console.WriteLine(Messages.ManagementDashboardNoLowStock);
        }
        else
        {
            foreach (var product in lowStockProducts)
            {
                Console.WriteLine($"- {product.Nume}: {product.Cantitate} units");
            }
        }

        Console.WriteLine(Messages.ManagementDashboardOrdersHeader);
        if (activeOrders.Count == 0)
        {
            Console.WriteLine(Messages.ManagementDashboardNoActiveOrders);
        }
        else
        {
            foreach (var order in activeOrders.OrderBy(o => o.Prioritate).ThenBy(o => o.Status))
            {
                Console.WriteLine($"- {order.Id}: {order.NumeProdus} | Qty: {order.CantitateTarget} | Priority: {order.Prioritate} | Status: {order.Status}");
            }
        }

        Console.WriteLine(Messages.ManagementDashboardMachinesHeader);
        if (machines.Count == 0)
        {
            Console.WriteLine(Messages.ManagementDashboardNoMachines);
        }
        else
        {
            foreach (var machine in machines)
            {
                Console.WriteLine($"- {machine.SerialNumber}: {machine.Nume} | {machine.Status} | {machine.Conditie} | {machine.GetHealthAlert()}");
            }
        }
    }

    private static void DisplayInventoryAlert(Product product, int threshold = 5)
    {
        if (product.Cantitate <= threshold)
            Console.WriteLine(string.Format(Messages.InventoryLowStockAlert, product.Nume, product.Cantitate, threshold));
    }

    public void AfiseazaRaportVanzari()
    {
        Console.WriteLine(string.Format(Messages.SalesReportHeader, Nume));
        Console.WriteLine(string.Format(Messages.SalesReportRevenue, _totalRevenue));
        Console.WriteLine(string.Format(Messages.SalesReportUnitsSold, _totalSalesQuantity));
        Console.WriteLine(string.Format(Messages.SalesReportAveragePrice, _totalSalesQuantity > 0 ? (_totalRevenue / _totalSalesQuantity).ToString("F2") : "N/A"));
        Console.WriteLine(string.Format(Messages.SalesReportEstimatedProfit, CalculateProfit()));
        Console.WriteLine(Messages.EmptyLine);
    }

    public void AfiseazaComenzi()
    {
        _orderRepository.DisplayAll();
    }

    public void AfiseazaComenziSortedByPriority()
    {
        List<ProductionOrder> comenziSortate = _orderRepository.GetSortedByPriority();

        if (comenziSortate.Count == 0)
        {
            Console.WriteLine(Messages.NoOrders);
            return;
        }

        Console.WriteLine(Messages.OrdersSortedByPriorityHeader);
        foreach (var comanda in comenziSortate)
        {
            comanda.Afiseaza();
        }
    }

    public ProductionOrder GetNextPriorityOrder(string idOperator)
    {
        // reload orders to make sure we consider persisted orders
        LoadOrdersFromFile();

        Employee angajat = GasesteAngajat(idOperator);
        if (angajat == null)
            return null;

        if (!(angajat is MachineOperator))
            return null;

        return _orderRepository.GetNextByPriority();
    }
}

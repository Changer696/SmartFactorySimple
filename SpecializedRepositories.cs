using SmartFactorySimple;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public class EmployeeRepository : RepositoryWithId<Employee>
{
    public override bool Add(Employee employee)
    {
        if (employee == null)
            return false;

        if (ExistsById(employee.Id))
        {
            Console.WriteLine($"There is already an employee with the ID {employee.Id}");
            return false;
        }

        return base.Add(employee);
    }

    public void DisplayAll()
    {
        if (_items.Count == 0)
        {
            Console.WriteLine("There are no employees!");
            return;
        }

        Console.WriteLine("=== EMPLOYEES ===");
        foreach (var employee in _items)
        {
            employee.Afiseaza();
        }
    }
}


public class MachineRepository : Repository<Machine>
{
    private readonly string MACHINES_FILE = AppFileNames.ResolvePath(AppFileNames.MachinesFileName);

    public Machine FindBySerialNumber(string serialNumber)
    {
        return _items.FirstOrDefault(m => m.SerialNumber == serialNumber);
    }

    public bool ExistsBySerialNumber(string serialNumber)
    {
        return _items.Any(m => m.SerialNumber == serialNumber);
    }

    public bool RemoveBySerialNumber(string serialNumber)
    {
        Machine machine = FindBySerialNumber(serialNumber);
        if (machine != null)
        {
            return Remove(machine);
        }
        return false;
    }

    public override bool Add(Machine machine)
    {
        if (machine == null)
            return false;

        if (ExistsBySerialNumber(machine.SerialNumber))
        {
            Console.WriteLine($"There is already a machine with the serial number {machine.SerialNumber}");
            return false;
        }

        return base.Add(machine);
    }

    public void DisplayAll()
    {
        if (_items.Count == 0)
        {
            Console.WriteLine("There are no machines!");
            return;
        }

        Console.WriteLine("=== MACHINES ===");
        foreach (var machine in _items)
        {
            machine.Afiseaza();
        }
    }

    // ---------- SALVARE ----------

    public bool SaveAllMachines()
    {
        try
        {
            var lines = _items.Select(SerializeMachine);
            File.WriteAllLines(MACHINES_FILE, lines);
            Console.WriteLine($"Saved {_items.Count} machines.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving machines: {ex.Message}");
            return false;
        }
    }

    private string SerializeMachine(Machine m)
    {
        string tip = m.GetType().Name;
        string lastMaint = m.LastMaintenanceDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";

        string pieseSerializate = string.Join("~", m.Piese.Take(m.NrPiese).Select(p =>
            $"{p.Nume},{p.Tip},{p.EFunctionala},{p.DataInstalarii:yyyy-MM-dd HH:mm:ss}"));

        return string.Join(";",
            tip,
            m.SerialNumber,
            m.Nume,
            m.Status,
            m.Conditie,
            m.DataFabricatiei.ToString("yyyy-MM-dd HH:mm:ss"),
            m.ProductionCycles,
            lastMaint,
            pieseSerializate);
    }

    // ---------- INCARCARE ----------

    public void LoadMachines()
    {
        Clear(); // golim ce era deja in _items, ca sa nu duplicam la reincarcare

        if (!File.Exists(MACHINES_FILE))
        {
            Console.WriteLine($"Error: {MACHINES_FILE} not found.");
            return;
        }

        string[] lines = File.ReadAllLines(MACHINES_FILE);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            string[] parts = line.Split(';');
            if (parts.Length != 9)
            {
                Console.WriteLine($"Warning: Invalid line format: {line}");
                continue;
            }

            try
            {
                string tip = parts[0].Trim();
                string serial = parts[1].Trim();
                string nume = parts[2].Trim();
                var status = Enum.Parse<MachineStatus>(parts[3].Trim());
                var conditie = Enum.Parse<MachineCondition>(parts[4].Trim());
                var dataFab = DateTime.Parse(parts[5].Trim());
                int cicluri = int.Parse(parts[6].Trim());
                DateTime? lastMaint = string.IsNullOrWhiteSpace(parts[7])
                    ? null
                    : DateTime.Parse(parts[7].Trim());
                string pieseRaw = parts[8].Trim();

                Machine masina = CreeazaMasina(tip, serial, nume, dataFab);
                if (masina == null)
                {
                    Console.WriteLine($"Warning: Unknown machine type '{tip}'");
                    continue;
                }

                masina.Status = status;
                masina.Conditie = conditie;
                masina.RestoreState(cicluri, lastMaint);

                if (!string.IsNullOrWhiteSpace(pieseRaw))
                {
                    foreach (string piesaStr in pieseRaw.Split('~'))
                    {
                        string[] pp = piesaStr.Split(',');
                        if (pp.Length != 4) continue;

                        var piesa = new MachinePart(pp[0].Trim(), pp[1].Trim())
                        {
                            EFunctionala = bool.Parse(pp[2].Trim()),
                            DataInstalarii = DateTime.Parse(pp[3].Trim())
                        };
                        masina.AdaugaPiesa(piesa);
                    }
                }

                Add(masina);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not parse line '{line}': {ex.Message}");
            }
        }

        Console.WriteLine($"Loaded {_items.Count} machines.");
    }

    // ---------- FACTORY ----------

    private Machine CreeazaMasina(string tip, string serial, string nume, DateTime dataFab)
    {
        return tip switch
        {
            "SewingMachine" => new SewingMachine(serial, nume, dataFab),
            "CuttingMachine" => new CuttingMachine(serial, nume, dataFab),
            _ => null
        };
    }
}


public class ProductRepository : Repository<Product>
{
    public Product FindByName(string name)
    {
        return _items.FirstOrDefault(p => p.Nume == name);
    }

    public bool ExistsByName(string name)
    {
        return _items.Any(p => p.Nume == name);
    }

    public bool RemoveByName(string name)
    {
        Product product = FindByName(name);
        if (product != null)
        {
            return Remove(product);
        }
        return false;
    }
    public void DisplayAll()
    {
        if (_items.Count == 0)
        {
            Console.WriteLine("There are no products!");
            return;
        }

        Console.WriteLine("=== PRODUCTS ===");
        foreach (var product in _items)
        {
            product.Afiseaza();
        }
    }

    private readonly string PRODUCTS_FILE = AppFileNames.ResolvePath(AppFileNames.ProductsFileName);

    // ---------- SAVE / LOAD ----------

    public bool SaveAllProducts()
    {
        try
        {
            var lines = _items.Select(SerializeProduct);
            File.WriteAllLines(PRODUCTS_FILE, lines);
            Console.WriteLine($"Saved {_items.Count} products.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving products: {ex.Message}");
            return false;
        }
    }

    private string SerializeProduct(Product p)
    {
        string tip = p.GetType().Name;
        // marime is a common extra property for derived products; reflect if present
        string marime = "";
        var prop = p.GetType().GetField("Marime");
        if (prop != null)
        {
            var val = prop.GetValue(p);
            marime = val?.ToString() ?? "";
        }

        return string.Join(";",
            tip,
            p.Nume,
            p.Category,
            p.ProductionCost.ToString(System.Globalization.CultureInfo.InvariantCulture),
            p.SellingPrice.ToString(System.Globalization.CultureInfo.InvariantCulture),
            p.Cantitate,
            marime);
    }

    public void LoadProducts()
    {
        Clear();

        if (!File.Exists(PRODUCTS_FILE))
        {
            Console.WriteLine($"Info: {PRODUCTS_FILE} not found. No products loaded.");
            return;
        }

        string[] lines = File.ReadAllLines(PRODUCTS_FILE);
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            string[] parts = line.Split(';');
            if (parts.Length < 6)
            {
                Console.WriteLine($"Warning: Invalid product line: {line}");
                continue;
            }

            try
            {
                string tip = parts[0].Trim();
                string nume = parts[1].Trim();
                var category = Enum.Parse<ProductCategory>(parts[2].Trim());
                decimal productionCost = decimal.Parse(parts[3].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                decimal sellingPrice = decimal.Parse(parts[4].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                int cantitate = int.Parse(parts[5].Trim());
                string marime = parts.Length >= 7 ? parts[6].Trim() : "";

                Product produs = CreeazaProdus(tip, nume, productionCost, sellingPrice, cantitate, marime);
                if (produs == null)
                {
                    Console.WriteLine($"Warning: Unknown product type '{tip}'");
                    continue;
                }

                Add(produs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not parse product line '{line}': {ex.Message}");
            }
        }

        Console.WriteLine($"Loaded {_items.Count} products.");
    }

    private Product CreeazaProdus(string tip, string nume, decimal productionCost, decimal sellingPrice, int cantitate, string marime)
    {
        return tip switch
        {
            "WoodenCubes" => new WoodenCubes(nume, productionCost, sellingPrice, cantitate, marime),
            "TedyBear" => new TedyBear(nume, productionCost, sellingPrice, cantitate, marime),
            "Ball" => new Ball(nume, productionCost, sellingPrice, cantitate, marime),
            "Doll" => new Doll(nume, productionCost, sellingPrice, cantitate, marime),
            "Frisbee" => new Frisbee(nume, productionCost, sellingPrice, cantitate, marime),
            _ => null
        };


    }

    public List<Product> FindByCategory(ProductCategory category)
    {
        return _items.Where(p => p.Category == category).ToList();
    }
}


public class ProductionOrderRepository : RepositoryWithId<ProductionOrder>
{
    public override bool Add(ProductionOrder order)
    {
        if (order == null)
            return false;

        return base.Add(order);
    }

    public void DisplayAll()
    {
        if (_items.Count == 0)
        {
            Console.WriteLine("There are no orders!");
            return;
        }

        Console.WriteLine("=== ORDERS ===");
        foreach (var order in _items)
        {
            order.Afiseaza();
        }
    }

    public List<ProductionOrder> GetAllActive()
    {
        return _items.Where(o => o.Status != ProductionOrderStatus.Completed).ToList();
    }

    public List<ProductionOrder> GetAllCompleted()
    {
        return _items.Where(o => o.Status == ProductionOrderStatus.Completed).ToList();
    }

    public List<ProductionOrder> GetByStatus(ProductionOrderStatus status)
    {
        return _items.Where(o => o.Status == status).ToList();
    }

    public List<ProductionOrder> GetByPriority(Priority priority)
    {
        return _items.Where(o => o.Prioritate == priority).ToList();
    }

    public List<ProductionOrder> GetSortedByPriority()
    {
        return _items
            .Where(o => o.Status != ProductionOrderStatus.Completed)
            .OrderByDescending(o => GetPriorityValue(o.Prioritate))
            .ThenBy(o => GetStatusValue(o.Status))
            .ToList();
    }

    public ProductionOrder GetNextByPriority()
    {
        return _items
            .Where(o => o.Status != ProductionOrderStatus.Completed)
            .OrderByDescending(o => GetPriorityValue(o.Prioritate))
            .ThenBy(o => GetStatusValue(o.Status))
            .FirstOrDefault();
    }

    private int GetPriorityValue(Priority priority)
    {
        return priority switch
        {
            Priority.High => 3,
            Priority.Medium => 2,
            Priority.Low => 1,
            _ => 0
        };
    }

    private int GetStatusValue(ProductionOrderStatus status)
    {
        return status switch
        {
            ProductionOrderStatus.Created => 2,
            ProductionOrderStatus.InProgress => 1,
            ProductionOrderStatus.Completed => 0,
            _ => 0
        };
    }
}
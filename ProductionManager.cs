using System;
using SmartFactorySimple;

public class ProductionManager : Employee
{
    public ProductionManager(string id, string nume, decimal salariu, DateTime dataAngajarii)
        : base(id, nume, salariu, dataAngajarii)
    {
        Rol = EmployeeRole.ProductionManager;
    }

    public ProductionOrder CreazaComanda(string idComanda, Machine masina,
                                          string produs, int cantitate, Priority prioritate)
    {
        Console.WriteLine(Messages.RoleDuty(Nume, "Production Manager") + " created the " + prioritate + " priority order " + idComanda + " for " + cantitate + " x " + produs);
        return new ProductionOrder(idComanda, masina, this, produs, cantitate, prioritate);
    }

    public override void PerformDuty()
    {
        Console.WriteLine(Messages.RoleDuty(Nume, "Production Manager") + " coordinates production.");
    }
}

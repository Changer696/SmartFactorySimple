using System;
using SmartFactorySimple;

public class Director : Employee
{
    public Director(string id, string nume, decimal salariu, DateTime dataAngajarii)
        : base(id, nume, salariu, dataAngajarii)
    {
        Rol = EmployeeRole.Director;
    }

    public override void PerformDuty()
    {
        Console.WriteLine(Messages.RoleDuty(Nume, "The director") + " checks the factory reports.");
    }

    public override void Afiseaza()
    {
        Console.WriteLine(Messages.DirectorDisplay(Id, Nume, Salariu, GetVechimeZile()));
    }
}

using System;
using SmartFactorySimple;

public abstract class Employee : IIdentifiable
{
    public string Id { get; set; }
    public string Nume;
    public EmployeeRole Rol;
    public decimal Salariu;
    public DateTime DataAngajarii;

    protected Employee(string id, string nume, decimal salariu, DateTime dataAngajarii)
    {
        Id = id;
        Nume = nume;
        Salariu = salariu;
        DataAngajarii = dataAngajarii;
    }

    public int GetVechimeZile()
    {
        TimeSpan diferenta = DateTime.Now - DataAngajarii;
        return diferenta.Days;
    }

    public abstract void PerformDuty();

    public virtual void Afiseaza()
    {
        Console.WriteLine(Messages.EmployeeDisplay(Id, Nume, Rol, Salariu, GetVechimeZile()));
    }
}

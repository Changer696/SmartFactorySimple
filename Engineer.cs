using System;
using SmartFactorySimple;

public class Engineer : Employee
{
    public Engineer(string id, string nume, decimal salariu, DateTime dataAngajarii)
        : base(id, nume, salariu, dataAngajarii)
    {
        Rol = EmployeeRole.Engineer;
    }

    public void Inspecteaza(Machine masina)
    {
        string rezultat = masina.RunDiagnostics();
        Console.WriteLine(Messages.RoleDuty(Nume, "Engineer") + " inspected the  " + masina.Nume + ": " + rezultat);
    }

    public override void PerformDuty()
    {
        Console.WriteLine(Messages.RoleDuty(Nume, "Engineer") + " inspects the machines.");
    }
}

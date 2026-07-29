using System;
using SmartFactorySimple;

public class MachineOperator : Employee
{
    public MachineOperator(string id, string nume, decimal salariu, DateTime dataAngajarii)
        : base(id, nume, salariu, dataAngajarii)
    {
        Rol = EmployeeRole.MachineOperator;
    }

    public void Opereaza(Machine masina)
    {

        if (masina.Status == MachineStatus.Running)
        {
            masina.Produce();
        }
        else if (masina.Status == MachineStatus.Maintenance)
        {
            Console.WriteLine(Messages.MachineOperatorMaintenanceMessage);
            return;
        }
        else
        {
            Console.WriteLine(Messages.MachineOperatorOffMessage);
            Console.WriteLine(Messages.MachineOperatorStartPrompt);
            string continuare = Console.ReadLine();
            if (continuare == "YES")
            { 
                masina.Status = MachineStatus.Running;
                masina.Produce();
            }
            else if(continuare == "NO")
                {
                 return;
                }

        }
    }

    public override void PerformDuty()
    {
        Console.WriteLine(Messages.RoleDuty(Nume, "Machine Operator") + " operates the machines.");
    }
}

using System;
using SmartFactorySimple;

public class SewingMachine : Machine
{
    public SewingMachine(string serial, string nume, DateTime dataFabricatiei)
        : base(serial, nume, dataFabricatiei)
    {
    }

    public override void Produce()
    {
        if (Status != MachineStatus.Running)
        {
            Console.WriteLine(Messages.SewingMachineNotStarted(Nume));
            return;
        }
        Console.WriteLine(Messages.SewingProduceMessage(Nume));
        DegradeazaConditia();
        StareVerificarePiesa();
        RegisterProductionCycle();
    }

    public override string RunDiagnostics()
    {
        if (Conditie == MachineCondition.Critical)
            return Messages.MachineDiagnosticWarning("The needle tension is irregular!");
        else
            return Messages.MachineDiagnosticHealthy("Needle and thread checked. Working normally.");
    }
}

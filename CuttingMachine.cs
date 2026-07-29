using System;
using SmartFactorySimple;

public class CuttingMachine : Machine
{
    public CuttingMachine(string serial, string nume, DateTime dataFabricatiei)
        : base(serial, nume, dataFabricatiei)
    {
    }

    public override void Produce()
    {
        if (Status != MachineStatus.Running)
        {
            Console.WriteLine(Messages.MachineNotRunning(Nume));
            return;
        }
        Console.WriteLine(Messages.MachineProduceMessage(Nume));
        DegradeazaConditia();
        StareVerificarePiesa();
        RegisterProductionCycle();

    }

    public override string RunDiagnostics()
    {
        if (Conditie == MachineCondition.Critical)
            return Messages.MachineDiagnosticWarning("The blade is dull, needs replacing!");
        else
            return Messages.MachineDiagnosticHealthy("Sharp blade. Normal operation.");
    }
}

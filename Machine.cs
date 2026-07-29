using System;
using System.Linq;
using SmartFactorySimple;

public abstract class Machine
{
    public string SerialNumber;
    public string Nume;
    public MachineStatus Status;
    public MachineCondition Conditie;
    public DateTime DataFabricatiei;
    public static Random _random = new Random();
    public int ProductionCycles { get; private set; }
    public DateTime? LastMaintenanceDate { get; private set; }

    public MachinePart[] Piese;
    public int NrPiese;

    protected Machine(string serialNumber, string nume, DateTime dataFabricatiei)
    {
        SerialNumber = serialNumber;
        Nume = nume;
        DataFabricatiei = dataFabricatiei;
        Status = MachineStatus.Stopped;
        Conditie = MachineCondition.Good;
        Piese = new MachinePart[10];
        NrPiese = 0;
    }

    public void AdaugaPiesa(MachinePart piesa)
    {
        Piese[NrPiese] = piesa;
        NrPiese++;
    }

    public bool ArePieseComplete()
    {
        return NrPiese > 0 && Piese
            .Take(NrPiese)
            .All(piesa => piesa.EFunctionala);
    }

    public void StareVerificarePiesa()
    {
        if (NrPiese == 0)
        {
            Console.WriteLine(Messages.NoExistingParts);
        }
         int sansa = _random.Next(1,101);
         if(sansa <= 20)
        {
            int index = _random.Next(NrPiese);
            if (Piese[index].EFunctionala)
            {
                Piese[index].Strica();
                Console.WriteLine(Messages.BrokenPart(Piese[index].Nume, Nume));
            }
        }
    }

    public int GetVarstaZile()
    {
        TimeSpan varsta = DateTime.Now - DataFabricatiei;
        return varsta.Days;
    }

    public int EstimateDaysUntilMaintenance()
    {
        int conditionAllowance = Conditie switch
        {
            MachineCondition.Excellent => 60,
            MachineCondition.Good => 30,
            MachineCondition.Worn => 7,
            _ => 0
        };

        return Math.Max(0, conditionAllowance - GetVarstaZile() / 365);
    }

    public decimal CalculateEfficiencyPercentage()
    {
        decimal conditionScore = Conditie switch
        {
            MachineCondition.Excellent => 100m,
            MachineCondition.Good => 85m,
            MachineCondition.Worn => 60m,
            _ => 30m
        };

        decimal functionalPartsRatio = NrPiese == 0
            ? 0m
            : Piese.Take(NrPiese).Count(piesa => piesa.EFunctionala) / (decimal)NrPiese;

        return Math.Round(conditionScore * functionalPartsRatio, 2);
    }

    public string GetHealthAlert()
    {
        int brokenParts = Piese.Take(NrPiese).Count(piesa => !piesa.EFunctionala);

        if (Status == MachineStatus.Maintenance || Conditie == MachineCondition.Critical)
            return Messages.MachineHealthCritical;
        if (brokenParts > 0)
            return Messages.MachineHealthWarning(brokenParts);
        if (Conditie == MachineCondition.Worn || EstimateDaysUntilMaintenance() <= 7)
            return Messages.MachineHealthPreventiveMaintenanceDue(EstimateDaysUntilMaintenance());

        return Messages.MachineHealthHealthy;
    }

    protected void RegisterProductionCycle()
    {
        ProductionCycles++;
    }


      public void RestoreState(int productionCycles, DateTime? lastMaintenanceDate)
    {
        ProductionCycles = productionCycles;
        LastMaintenanceDate = lastMaintenanceDate;
    }

    public virtual void Start()
    {
        if (Status == MachineStatus.Maintenance)
        {
            Console.WriteLine(Messages.MachineUnderMaintenance(Nume));
            return;
        }
        if (!ArePieseComplete())
        {
            Console.WriteLine(Messages.MachineHasBrokenParts(Nume));
            return;
        }
        Status = MachineStatus.Running;
        Console.WriteLine(Messages.MachineTurnedOn(Nume));
        Logging.Log($"Started machine {SerialNumber}");
    }

    public virtual void Stop()
    {
        Status = MachineStatus.Stopped;
        Console.WriteLine(Messages.MachineTurnedOff(Nume));
        Logging.Log($"Stopped machine {SerialNumber}");
    }

    public void SetMaintenance()
    {
        if (Status == MachineStatus.Running)
        {
            Console.WriteLine(Messages.StopMachineBeforeMaintenance);
            return;
        }
        Status = MachineStatus.Maintenance;
    }

    protected void DegradeazaConditia()
    {
        if (Conditie == MachineCondition.Excellent)
            Conditie = MachineCondition.Good;
        else if (Conditie == MachineCondition.Good)
            Conditie = MachineCondition.Worn;
        else if (Conditie == MachineCondition.Worn)
        { Conditie = MachineCondition.Critical; Status = MachineStatus.Maintenance; }
    
    }

    public void RestoreazaConditia()
    {
        Conditie = MachineCondition.Excellent;
        Status = MachineStatus.Stopped;
        LastMaintenanceDate = DateTime.Now;
    }

    public abstract void Produce();
    public abstract string RunDiagnostics();

    public virtual void Afiseaza()
    {
        Console.WriteLine(Messages.MachineConditionLine(SerialNumber, Nume, Status, Conditie, GetVarstaZile(), NrPiese));
    }
}

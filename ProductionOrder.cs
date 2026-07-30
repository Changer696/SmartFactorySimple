using System;
using SmartFactorySimple;

public class ProductionOrder : IIdentifiable
{
    public string Id { get; set; }
    public Machine Masina;
    public ProductionManager CreatDe;
    public string NumeProdus;
    public int CantitateTarget;
    public int CantitateProdusa;
    public ProductionOrderStatus Status;
    public Priority Prioritate;
    public DateTime DataCrearii;

    public ProductionOrder(string id, Machine masina, ProductionManager creatDe,
                           string numeProdus, int cantitateTarget, Priority prioritate)
    {
        Id = id;
        Masina = masina;
        CreatDe = creatDe;
        NumeProdus = numeProdus;
        CantitateTarget = cantitateTarget;
        CantitateProdusa = 0;
        Status = ProductionOrderStatus.Created;
        Prioritate = prioritate;
        DataCrearii = DateTime.Now;
    }

    public int InregistreazaProductie(int unitati)
    {
        if (unitati <= 0)
        {
            Console.WriteLine(Messages.ProductionQuantityMustBePositive);
            return 0;
        }

        if (Status == ProductionOrderStatus.Completed)
        {
            Console.WriteLine(Messages.OrderAlreadyCompleted);
            return 0;
        }

        int remaining = CantitateTarget - CantitateProdusa;
        int actualProduced = Math.Min(unitati, remaining);
        if (actualProduced < unitati)
        {
            Console.WriteLine(Messages.ProductionQuantityCapped(remaining));
        }

        CantitateProdusa += actualProduced;

        if (CantitateProdusa >= CantitateTarget)
        {
            CantitateProdusa = CantitateTarget;
            Status = ProductionOrderStatus.Completed;
            Console.WriteLine(Messages.OrderCompleted(Id));
            if (CreatDe != null)
                Logging.Log(CreatDe.Id, $"Produced {actualProduced} units for order {Id} ({NumeProdus}) - completed");
        }
        else
        {
            Status = ProductionOrderStatus.InProgress;
            Console.WriteLine(Messages.OrderProgress(Id, CantitateProdusa, CantitateTarget));
            if (CreatDe != null)
                Logging.Log(CreatDe.Id, $"Produced {actualProduced} units for order {Id} ({NumeProdus})");
        }

        return actualProduced;
    }
    public void Afiseaza()
    {
        Console.WriteLine("[" + Id + "] " + NumeProdus +
                          " x" + CantitateTarget +
                          " | Produced: " + CantitateProdusa + "/" + CantitateTarget +
                          " | Status: " + Status +
                          " | Priority: " + Prioritate +
                          " | Manager: " + CreatDe.Nume +
                          " | Machine: " + Masina.SerialNumber +
                          " | Date: " + DataCrearii.ToString("yyyy-MM-dd"));
    }
}

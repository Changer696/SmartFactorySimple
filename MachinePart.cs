using System;
using SmartFactorySimple;

public class MachinePart
{
    public string Nume;
    public string Tip;
    public bool EFunctionala;
    public DateTime DataInstalarii;

    public MachinePart(string nume, string tip)
    {
        Nume = nume;
        Tip = tip;
        EFunctionala = true;
        DataInstalarii = DateTime.Now;
    }

    public void Strica()
    {
        EFunctionala = false;
    }

    public void Inlocuieste()
    {
        EFunctionala = true;
        DataInstalarii = DateTime.Now;
    }

    public void Afiseaza()
    {
        string status;
        if (EFunctionala)
            status = "OK";
        else
            status = "BROKEN";

        Console.WriteLine(Messages.MachinePartStatus(Nume, Tip, status));
    }
}

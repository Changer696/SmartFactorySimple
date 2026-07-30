using System;
using SmartFactorySimple;

public abstract class Product
{
    public decimal productionCost;
    public decimal sellingPrice;
    public int quantity;
    public decimal SellingPrice
    {
        get { return sellingPrice; }
        set
        {
            if (value < 0) throw new ArgumentException("The selling price can't be negative.");
            sellingPrice = value;
        }
    }
    public decimal ProductionCost
    {
        get { return productionCost; }
        set
        {
            if (value < 0) throw new ArgumentException("The production cost can't be negative.");
            productionCost = value;
        }
    }
    public int Cantitate
    {
        get { return quantity; }
        set
        {
            if (value < 0) throw new ArgumentException("The quantity can't be negative.");
            quantity = value;
        }
    }
    public string Nume { get; set; }
    public ProductCategory Category { get; set; }
    
    protected Product(string nume, ProductCategory category, decimal productionCost, decimal sellingPrice, int cantitate)
    {
        Nume = nume;
        Category = category;
        SellingPrice = sellingPrice;
        Cantitate = cantitate;
        ProductionCost = productionCost;
        
    }
    public void AdaugaStoc(int cantitate)
    {
        if (cantitate < 0)
        {
            Console.WriteLine(Messages.ProductQuantityNegative);
            return;
        }
        Cantitate = Cantitate + cantitate;
    }
    public void VindeStoc(int cantitate)
    {
        if (cantitate > Cantitate)
        {
            Console.WriteLine(Messages.InsufficientStock);
            return;
        }
        Cantitate = Cantitate - cantitate;
    }
    public abstract string GetDescription();
    public virtual void Afiseaza()
    {
        Console.WriteLine(Messages.ProductDisplay(GetDescription(), ProductionCost, SellingPrice, Cantitate));
    }
}

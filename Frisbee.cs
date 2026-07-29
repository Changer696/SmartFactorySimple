using System;
namespace SmartFactorySimple
{
    internal class Frisbee : Product
    {
        public string Marime;
        public Frisbee(string nume, decimal productionCost, decimal sellingPrice, int cantitate, string marime)
        : base(nume, ProductCategory.OutdoorToys, productionCost, sellingPrice, cantitate)
        {
            Marime = marime;
        }

        public override string GetDescription()
        {
            return Messages.ProductDescription("Frisbee", Nume, Category, Marime);
        }
    }
}

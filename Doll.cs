using System;

namespace SmartFactorySimple
{
    internal class Doll : Product
    {
        public string Marime;
        public Doll(string nume, decimal productionCost, decimal sellingPrice, int cantitate, string marime)
        : base(nume, ProductCategory.PretendToys, productionCost, sellingPrice, cantitate)
        {
            Marime = marime;
        }

        public override string GetDescription()
        {
            return Messages.ProductDescription("Doll", Nume, Category, Marime);
        }
    }
}

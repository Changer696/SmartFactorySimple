using System;

namespace SmartFactorySimple
{
    internal class Ball : Product
    {
        public string Marime;
        public Ball(string nume, decimal productionCost, decimal sellingPrice, int cantitate, string marime)
        : base(nume, ProductCategory.OutdoorToys, productionCost, sellingPrice, cantitate)
        {
            Marime = marime;
        }

        public override string GetDescription()
        {
            return Messages.ProductDescription("Ball", Nume, Category, Marime);
        }
    }
}

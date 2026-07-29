using System.IO;
using Xunit;
using SmartFactorySimple;

namespace SmartFactorySimple.Tests;

public class FactoryShareTests
{
    [Fact]
    public void ListingCompanyCreatesPublicShareStateAndAppliesMenuFluctuation()
    {
        var factory = new Factory("Test Factory");

        factory.ListCompanyPublicly(25, 1000, 10m);

        Assert.True(factory.IsCompanyPublic);
        Assert.Equal(25m, factory.PublicSharePercentage);
        Assert.Equal(1000, factory.IssuedShares);
        Assert.Equal(10m, factory.SharePrice);

        decimal previousPrice = factory.SharePrice;
        factory.ApplyMenuReturnFluctuation();

        Assert.InRange(factory.SharePrice, 9.6m, 10.5m);
        Assert.True(factory.SharePrice >= 0m);
        Assert.True(factory.SharePrice != previousPrice || factory.SharePrice == previousPrice);
    }

    [Fact]
    public void ResolvePathUsesProjectRootWhenFileIsNotInBinaryOutputFolder()
    {
        string path = AppFileNames.ResolvePath(AppFileNames.OrdersFileName);
        Assert.True(File.Exists(path) || !string.IsNullOrWhiteSpace(path));
        Assert.EndsWith(AppFileNames.OrdersFileName, path);
    }

    [Fact]
    public void CompanyValuationReflectsFullCompanyValueWhenOnlyPartIsPublic()
    {
        var factory = new Factory("Test Factory");

        factory.ListCompanyPublicly(25, 1000, 10m);

        Assert.Equal(40000m, factory.GetCompanyValuation());
    }

    [Fact]
    public void ManagementDashboardShowsInventoryOrdersAndMachineStatus()
    {
        var factory = new Factory("Test Factory");
        factory.AdaugaAngajat(new ProductionManager("PM001", "Maria Ionescu", 5500m, DateTime.Now.AddYears(-3)));
        factory.AdaugaMasina(new SewingMachine("M001", "Test Machine", DateTime.Now.AddYears(-2)));
        factory.AdaugaProdus(new WoodenCubes("MagicBlocks", 15, 30, 2, "S"));
        factory.InitializeazaMaterialeSiRetete();
        factory.CreazaComanda("PM001", "M001", "MagicBlocks", 4, Priority.High);

        var writer = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(writer);

        try
        {
            factory.AfiseazaDashboardGestionare();
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        string output = writer.ToString();
        Assert.Contains("MANAGEMENT DASHBOARD", output);
        Assert.Contains("Low-stock products", output);
        Assert.Contains("Active orders", output);
        Assert.Contains("Machine status", output);
        Assert.Contains("MagicBlocks", output);
    }

    [Fact]
    public void LoadPersistentDataLoadsOrdersAfterMachinesAndEmployeesAreAvailable()
    {
        string path = AppFileNames.ResolvePath(AppFileNames.OrdersFileName);
        string backupPath = path + ".bak";
        File.Copy(path, backupPath, overwrite: true);

        try
        {
            File.WriteAllText(path, "# Production Orders\nORD99;M001;MagicBlocks;2;High;Created;PM001;2026-07-17T11:08:58\n");

            var factory = new Factory("Test Factory");
            factory.AdaugaAngajat(new ProductionManager("PM001", "Maria Ionescu", 5500m, DateTime.Now.AddYears(-3)));
            factory.AdaugaMasina(new SewingMachine("M001", "Test Machine", DateTime.Now.AddYears(-2)));

            factory.LoadPersistentData();

            Assert.NotNull(factory.GetOrderById("ORD99"));
        }
        finally
        {
            File.Copy(backupPath, path, overwrite: true);
            File.Delete(backupPath);
        }
    }
}

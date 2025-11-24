using FoodProduction.Models;

namespace FoodProduction.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Skip seeding if data already exists
        if (context.Products.Any())
        {
            return;
        }

        // Create default ZPL label template
        var defaultTemplate = new LabelTemplate
        {
            Name = "Default Food Label",
            Content = @"^XA
^FO10,10^AAN,28,20^FD{ProductName}^FS
^FO10,50^AAN,20,15^FDWeight: {Weight}^FS
^FO10,80^AAN,20,15^FDPrice: {Price}^FS
^FO10,110^AAN,15,12^FDDate: {Date}^FS
^FO10,140^BY2,3,80^BC^FD{ProductName}^FS
^XZ"
        };

        context.LabelTemplates.Add(defaultTemplate);
        await context.SaveChangesAsync();

        // Create ingredients
        var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Flour", CostPerKg = 2.50m },
            new Ingredient { Name = "Sugar", CostPerKg = 1.80m },
            new Ingredient { Name = "Butter", CostPerKg = 8.00m },
            new Ingredient { Name = "Eggs", CostPerKg = 5.50m },
            new Ingredient { Name = "Baking Powder", CostPerKg = 12.00m },
            new Ingredient { Name = "Vanilla Extract", CostPerKg = 15.00m },
            new Ingredient { Name = "Milk", CostPerKg = 1.20m },
            new Ingredient { Name = "Salt", CostPerKg = 0.50m }
        };

        context.Ingredients.AddRange(ingredients);
        await context.SaveChangesAsync();

        // Create products
        var product1 = new Product
        {
            Name = "Vanilla Cake",
            DefaultGainPercentage = 25m,
            DefaultLabelTemplateId = defaultTemplate.Id
        };

        var product2 = new Product
        {
            Name = "Chocolate Cake",
            DefaultGainPercentage = 30m,
            DefaultLabelTemplateId = defaultTemplate.Id
        };

        var product3 = new Product
        {
            Name = "Sugar Cookie",
            DefaultGainPercentage = 20m,
            DefaultLabelTemplateId = defaultTemplate.Id
        };

        context.Products.AddRange(product1, product2, product3);
        await context.SaveChangesAsync();

        // Reload ingredients to get updated IDs
        var flour = context.Ingredients.First(i => i.Name == "Flour");
        var sugar = context.Ingredients.First(i => i.Name == "Sugar");
        var butter = context.Ingredients.First(i => i.Name == "Butter");
        var eggs = context.Ingredients.First(i => i.Name == "Eggs");
        var bakingPowder = context.Ingredients.First(i => i.Name == "Baking Powder");
        var vanilla = context.Ingredients.First(i => i.Name == "Vanilla Extract");
        var milk = context.Ingredients.First(i => i.Name == "Milk");
        var salt = context.Ingredients.First(i => i.Name == "Salt");

        // Create formulations for Vanilla Cake
        var vanillaCakeFormulations = new List<ProductFormulation>
        {
            new ProductFormulation { ProductId = product1.Id, IngredientId = flour.Id, Ratio = 30m },
            new ProductFormulation { ProductId = product1.Id, IngredientId = sugar.Id, Ratio = 25m },
            new ProductFormulation { ProductId = product1.Id, IngredientId = butter.Id, Ratio = 20m },
            new ProductFormulation { ProductId = product1.Id, IngredientId = eggs.Id, Ratio = 15m },
            new ProductFormulation { ProductId = product1.Id, IngredientId = milk.Id, Ratio = 8m },
            new ProductFormulation { ProductId = product1.Id, IngredientId = bakingPowder.Id, Ratio = 1m },
            new ProductFormulation { ProductId = product1.Id, IngredientId = vanilla.Id, Ratio = 0.5m },
            new ProductFormulation { ProductId = product1.Id, IngredientId = salt.Id, Ratio = 0.5m }
        };

        // Create formulations for Chocolate Cake
        var chocolateCakeFormulations = new List<ProductFormulation>
        {
            new ProductFormulation { ProductId = product2.Id, IngredientId = flour.Id, Ratio = 28m },
            new ProductFormulation { ProductId = product2.Id, IngredientId = sugar.Id, Ratio = 30m },
            new ProductFormulation { ProductId = product2.Id, IngredientId = butter.Id, Ratio = 22m },
            new ProductFormulation { ProductId = product2.Id, IngredientId = eggs.Id, Ratio = 12m },
            new ProductFormulation { ProductId = product2.Id, IngredientId = milk.Id, Ratio = 6m },
            new ProductFormulation { ProductId = product2.Id, IngredientId = bakingPowder.Id, Ratio = 1.5m },
            new ProductFormulation { ProductId = product2.Id, IngredientId = salt.Id, Ratio = 0.5m }
        };

        // Create formulations for Sugar Cookie
        var sugarCookieFormulations = new List<ProductFormulation>
        {
            new ProductFormulation { ProductId = product3.Id, IngredientId = flour.Id, Ratio = 35m },
            new ProductFormulation { ProductId = product3.Id, IngredientId = sugar.Id, Ratio = 25m },
            new ProductFormulation { ProductId = product3.Id, IngredientId = butter.Id, Ratio = 30m },
            new ProductFormulation { ProductId = product3.Id, IngredientId = eggs.Id, Ratio = 5m },
            new ProductFormulation { ProductId = product3.Id, IngredientId = vanilla.Id, Ratio = 1m },
            new ProductFormulation { ProductId = product3.Id, IngredientId = salt.Id, Ratio = 4m }
        };

        context.ProductFormulations.AddRange(vanillaCakeFormulations);
        context.ProductFormulations.AddRange(chocolateCakeFormulations);
        context.ProductFormulations.AddRange(sugarCookieFormulations);

        await context.SaveChangesAsync();
    }
}

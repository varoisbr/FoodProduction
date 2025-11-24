using CsvHelper;
using FoodProduction.Services;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace FoodProduction.Services;

public interface IExportService
{
    Task<byte[]> ExportFormulationToPdfAsync(FormulationResult formulation);

    Task<byte[]> ExportFormulationToCsvAsync(FormulationResult formulation);
}

public class ExportService : IExportService
{
    public Task<byte[]> ExportFormulationToPdfAsync(FormulationResult formulation)
    {
        return Task.Run(() =>
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.MarginVertical(2, Unit.Centimetre);
                    page.MarginHorizontal(2, Unit.Centimetre);

                    page.Header()
                        .Text($"Product Formulation: {formulation.ProductName}")
                        .FontSize(16)
                        .Bold();

                    page.Content()
                        .Column(column =>
                        {
                            column.Item().Text($"Target Weight: {formulation.TargetWeightKg} kg").FontSize(11);
                            column.Item().Text($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(11);

                            column.Item().PaddingTop(1, Unit.Centimetre);

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                    });

                                    // Header
                                    table.Header(header =>
                                    {
                                        header.Cell().Padding(5).Background("#e9ecef").Text("Ingredient").Bold();
                                        header.Cell().Padding(5).Background("#e9ecef").Text("Ratio %").Bold();
                                        header.Cell().Padding(5).Background("#e9ecef").AlignRight().Text("Weight (kg)").Bold();
                                        header.Cell().Padding(5).Background("#e9ecef").AlignRight().Text("Cost/kg").Bold();
                                        header.Cell().Padding(5).Background("#e9ecef").AlignRight().Text("Subtotal").Bold();
                                    });

                                    // Rows
                                    foreach (var ingredient in formulation.Ingredients)
                                    {
                                        table.Cell().Padding(5).Text(ingredient.IngredientName);
                                        table.Cell().Padding(5).Text($"{ingredient.Ratio:F2}%");
                                        table.Cell().Padding(5).AlignRight().Text($"{ingredient.CalculatedWeightKg:F4}");
                                        table.Cell().Padding(5).AlignRight().Text($"${ingredient.CostPerKg:F2}");
                                        table.Cell().Padding(5).AlignRight().Text($"${ingredient.SubtotalCost:F2}");
                                    }

                                    // Total row
                                    table.Cell().ColumnSpan(4).Padding(5).Background("#f8f9fa").Text("TOTAL").Bold().AlignRight();
                                    table.Cell().Padding(5).Background("#f8f9fa").AlignRight().Text($"${formulation.TotalEstimatedCost:F2}").Bold();
                                });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                });
            }).GeneratePdf();

            return pdf;
        });
    }

    public Task<byte[]> ExportFormulationToCsvAsync(FormulationResult formulation)
    {
        return Task.Run(() =>
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Write header
                csv.WriteField("Product");
                csv.WriteField("Target Weight (kg)");
                csv.WriteField("Export Date");
                csv.NextRecord();

                csv.WriteField(formulation.ProductName);
                csv.WriteField(formulation.TargetWeightKg);
                csv.WriteField(DateTime.Now);
                csv.NextRecord();

                csv.NextRecord();

                // Write ingredient header
                csv.WriteField("Ingredient");
                csv.WriteField("Ratio (%)");
                csv.WriteField("Calculated Weight (kg)");
                csv.WriteField("Cost per kg");
                csv.WriteField("Subtotal Cost");
                csv.NextRecord();

                // Write ingredient data
                foreach (var ingredient in formulation.Ingredients)
                {
                    csv.WriteField(ingredient.IngredientName);
                    csv.WriteField(ingredient.Ratio);
                    csv.WriteField(ingredient.CalculatedWeightKg);
                    csv.WriteField(ingredient.CostPerKg);
                    csv.WriteField(ingredient.SubtotalCost);
                    csv.NextRecord();
                }

                csv.NextRecord();

                csv.WriteField("TOTAL ESTIMATED COST");
                csv.WriteField(string.Empty);
                csv.WriteField(string.Empty);
                csv.WriteField(string.Empty);
                csv.WriteField(formulation.TotalEstimatedCost);
                csv.NextRecord();

                writer.Flush();
                return memoryStream.ToArray();
            }
        });
    }
}

# Food Production Management System

A production-ready .NET 8 web application for managing product formulations, production batches, and ZPL label printing in food manufacturing facilities.

## Features

### 1. Formulation Calculator
- Select products from database
- Input target weight (kg)
- Automatic calculation of ingredient quantities based on formulation ratios
- Display ingredient list with costs
- Export formulations as PDF or CSV

### 2. Production & Packaging
- Create production batches with gain/loss percentage configuration
- Add packs with measured weights
- Automatic price calculation based on batch cost and gain percentage
- Automatic ZPL label generation and printing
- Track pack status (printed/pending)
- Batch summary with statistics (total weight, average yield, total value)

### 3. ZPL Label Printing
- Customizable ZPL label templates with placeholders
- Template management interface
- Support for TCP printing to physical printers
- Fallback to file-based output when printer unavailable
- Configurable printer IP and port

### 4. Master Data Management
- Product management (name, default gain %, default template)
- Ingredient management (name, cost per kg)
- Product formulations (ingredient ratios)
- Label template editor
- All with full CRUD operations

### 5. REST API
- Minimal API for external integrations
- Endpoints for:
  - Formulation calculation
  - Batch creation and pack management
  - Label rendering and printing
  - Template management
- Swagger UI documentation

## Technology Stack

- **Framework**: .NET 8
- **UI**: Razor Pages + Bootstrap 5
- **ORM**: Entity Framework Core 8
- **Database**: SQLite (default) / PostgreSQL (production)
- **Logging**: Serilog
- **PDF/CSV Export**: QuestPDF, CsvHelper
- **API Documentation**: Swagger

## Project Structure

```
FoodProduction/
├── Models/                      # Entity models
│   ├── Product.cs
│   ├── Ingredient.cs
│   ├── ProductFormulation.cs
│   ├── ProductionBatch.cs
│   ├── Pack.cs
│   └── LabelTemplate.cs
├── Data/                        # EF Core DbContext
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
├── Services/                    # Business logic
│   ├── FormulationService.cs
│   ├── ProductionService.cs
│   ├── ZplPrinterService.cs
│   └── ExportService.cs
├── Pages/                       # Razor Pages
│   ├── Index.cshtml
│   ├── Formulation.cshtml
│   ├── Production.cshtml
│   └── Admin/
│       ├── Products.cshtml
│       ├── Ingredients.cshtml
│       ├── Formulations.cshtml
│       └── LabelTemplates.cshtml
├── Controllers/                 # REST API
│   ├── FormulationController.cs
│   ├── ProductionController.cs
│   └── PrinterController.cs
├── wwwroot/                     # Static files
│   ├── css/site.css
│   └── js/site.js
├── Dockerfile                   # Container configuration
└── appsettings.json            # Configuration file
```

## Getting Started

### Prerequisites
- .NET 8 SDK or later
- SQLite or PostgreSQL
- Optionally: ZPL-compatible label printer

### Installation

1. **Clone/Extract the project**
   ```bash
   cd FoodProduction
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database** (creates SQLite database and applies migrations)
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - Open browser: `http://localhost:5000` (or `https://localhost:5001`)
   - Swagger API: `http://localhost:5000/swagger`

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "Printer": {
    "IP": "192.168.1.100",
    "Port": "9100",
    "Enabled": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Printer Configuration
- Modify `Printer:IP` to match your ZPL printer's IP address
- Set `Printer:Port` (default: 9100)
- Set `Printer:Enabled` to `true` to enable printing

### Database Configuration
- **SQLite** (default): Use `Data Source=app.db`
- **PostgreSQL**: Use `Server=localhost;Port=5432;Database=foodproduction;User Id=postgres;Password=your_password;`

## Database Models

### Product
```
- Id (int, PK)
- Name (string, required)
- DefaultGainPercentage (decimal)
- DefaultLabelTemplateId (int?, FK)
```

### Ingredient
```
- Id (int, PK)
- Name (string, required)
- CostPerKg (decimal)
```

### ProductFormulation
```
- Id (int, PK)
- ProductId (int, FK)
- IngredientId (int, FK)
- Ratio (decimal, %)
```

### ProductionBatch
```
- Id (int, PK)
- ProductId (int, FK)
- StartDate (datetime)
- GainPercentage (decimal)
- TotalCost (decimal)
- Notes (string, nullable)
```

### Pack
```
- Id (int, PK)
- BatchId (int, FK)
- WeightKg (decimal)
- Price (decimal)
- Printed (bool)
- CreatedAt (datetime)
```

### LabelTemplate
```
- Id (int, PK)
- Name (string, required)
- Content (string, ZPL code)
- CreatedAt (datetime)
- UpdatedAt (datetime)
```

## API Endpoints

### Formulation API
- `GET /api/formulation/calculate/{productId}/{targetWeightKg}` - Calculate formulation

### Production API
- `POST /api/production/batch/create` - Create production batch
- `POST /api/production/pack/add` - Add pack to batch
- `GET /api/production/batch/{batchId}/summary` - Get batch summary

### Printer API
- `POST /api/printer/print` - Print ZPL label
- `POST /api/printer/render` - Render ZPL from template
- `GET /api/printer/templates` - List all label templates

## ZPL Label Template Syntax

Create custom labels using ZPL placeholders:
- `{ProductName}` - Product name
- `{Weight}` - Package weight
- `{Price}` - Package price
- `{Date}` - Current date/time

Example:
```zpl
^XA
^FO10,10^AAN,28,20^FD{ProductName}^FS
^FO10,50^AAN,20,15^FDWeight: {Weight}^FS
^FO10,80^AAN,20,15^FDPrice: {Price}^FS
^FO10,110^AAN,15,12^FDDate: {Date}^FS
^FO10,140^BY2,3,80^BC^FD{ProductName}^FS
^XZ
```

## Docker Deployment

### Build Docker Image
```bash
docker build -t foodproduction:latest .
```

### Run Container
```bash
docker run -d \
  --name foodproduction \
  -p 80:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -v foodproduction-data:/app/data \
  -v foodproduction-logs:/app/logs \
  foodproduction:latest
```

### Docker Compose (Optional)
```yaml
version: '3.8'
services:
  foodproduction:
    build: .
    ports:
      - "80:80"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "Data Source=/app/data/app.db"
    volumes:
      - foodproduction-data:/app/data
      - foodproduction-logs:/app/logs
volumes:
  foodproduction-data:
  foodproduction-logs:
```

## Initial Data

The application seeds initial data on first run:
- 3 sample products (Vanilla Cake, Chocolate Cake, Sugar Cookie)
- 8 sample ingredients with costs
- Complete formulations for each product
- Default ZPL label template

## Usage Workflow

1. **Setup Phase**
   - Go to Admin > Products and create your products
   - Go to Admin > Ingredients and add your ingredients with costs
   - Go to Admin > Formulations and define ingredient ratios for each product
   - Go to Admin > Label Templates and customize your label designs

2. **Formulation Phase**
   - Go to Formulation Calculator
   - Select a product and enter desired weight
   - Review calculated ingredient quantities and costs
   - Export as PDF or CSV if needed

3. **Production Phase**
   - Go to Production & Packaging
   - Create a new batch for a product
   - Set gain/loss percentage
   - Add packs one by one with measured weights
   - Labels are automatically printed to configured printer

## Logging

Logs are written to:
- **Console** output (always)
- **File** at `logs/app-{date}.txt` (rolling daily)

Log level is configurable in `appsettings.json`

## Troubleshooting

### Printer not connecting
- Verify printer IP and port in `appsettings.json`
- Check that printer is on the same network
- Labels will be saved to `labels/` folder as fallback

### Database issues
- Delete `app.db` to reset the database
- Run `dotnet ef database update` to apply migrations

### Port already in use
- Default ports: 5000 (HTTP), 5001 (HTTPS)
- Change in `Properties/launchSettings.json`

## Performance Considerations

- SQLite is suitable for small to medium deployments
- For large-scale production, use PostgreSQL
- Label printing to TCP is asynchronous, non-blocking
- Database queries are optimized with proper indexing via EF Core

## Security Notes

- Application uses HTTPS in production
- Session-based batch tracking (can be extended with authentication)
- Recommend implementing ASP.NET Identity for multi-user scenarios
- Validate all user inputs on server-side

## License

This project is provided as-is for food production management.

## Support

For issues, questions, or contributions, please refer to project documentation or contact your development team.

---

**Built with ❤️ using .NET 8 + Razor Pages**

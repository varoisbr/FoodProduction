# Project Structure Documentation

## Complete File Organization

### Core Application Files

#### Root Level Configuration
- **Program.cs** - Application startup, DI registration, middleware configuration
- **appsettings.json** - Configuration for database, printer, logging
- **appsettings.Development.json** - Development-specific settings
- **FoodProduction.csproj** - Project file with NuGet dependencies
- **Dockerfile** - Container image definition for deployment

### Data Layer (`Data/`)

**ApplicationDbContext.cs**
- EF Core DbContext with all entity DbSets
- Fluent API configuration for relationships and constraints
- Decimal precision configurations
- UTC datetime conversion for consistency

**SeedData.cs**
- Static initialization method called on startup
- Creates default data:
  - 1 default ZPL label template
  - 8 sample ingredients with costs
  - 3 sample products (Vanilla Cake, Chocolate Cake, Sugar Cookie)
  - Complete formulations for each product
- Only runs if data doesn't exist

### Models (`Models/`)

**Product.cs**
```csharp
- Id: int (Primary Key)
- Name: string (required)
- DefaultGainPercentage: decimal
- DefaultLabelTemplateId: int? (Foreign Key)
- Relationships: Formulations, ProductionBatches, DefaultLabelTemplate
```

**Ingredient.cs**
```csharp
- Id: int (Primary Key)
- Name: string (required)
- CostPerKg: decimal
- Relationships: ProductFormulations
```

**ProductFormulation.cs**
```csharp
- Id: int (Primary Key)
- ProductId: int (Foreign Key)
- IngredientId: int (Foreign Key)
- Ratio: decimal (percentage 0-100)
- Relationships: Product, Ingredient
```

**ProductionBatch.cs**
```csharp
- Id: int (Primary Key)
- ProductId: int (Foreign Key)
- StartDate: DateTime (UTC)
- GainPercentage: decimal
- TotalCost: decimal (accumulated)
- Notes: string (optional)
- Relationships: Product, Packs
```

**Pack.cs**
```csharp
- Id: int (Primary Key)
- BatchId: int (Foreign Key)
- WeightKg: decimal
- Price: decimal (calculated)
- Printed: bool
- CreatedAt: DateTime (UTC)
- Relationships: ProductionBatch
```

**LabelTemplate.cs**
```csharp
- Id: int (Primary Key)
- Name: string (required)
- Content: string (ZPL code with placeholders)
- CreatedAt: DateTime (UTC)
- UpdatedAt: DateTime (UTC)
- Relationships: Products (inverse)
```

### Services Layer (`Services/`)

**FormulationService.cs** - Business logic for recipe calculations
```
Methods:
- GetProductAsync(int): Task<Product?>
- GetProductFormulationsAsync(int): Task<List<ProductFormulation>>
- CalculateFormulationAsync(int, decimal): Task<FormulationResult>

Returns:
- FormulationResult with ingredient details and total cost
```

**ProductionService.cs** - Production batch and pack management
```
Methods:
- CreateBatchAsync(int, decimal, string?): Task<ProductionBatch>
- AddPackAsync(int, decimal): Task<Pack>
- GetBatchAsync(int): Task<ProductionBatch?>
- GetBatchPacksAsync(int): Task<List<Pack>>
- GetBatchSummaryAsync(int): Task<BatchSummary>
- MarkPackAsPrintedAsync(int): Task<bool>

Returns:
- Batch and pack entities with calculated prices
- Summary statistics
```

**ZplPrinterService.cs** - ZPL label rendering and printing
```
Methods:
- RenderLabelAsync(template, name, weight, price, date): Task<string>
- PrintLabelAsync(zplContent): Task<bool>
- PrintLabelToFileAsync(zplContent, path): Task<bool>
- GetTemplateAsync(int): Task<LabelTemplate?>
- GetAllTemplatesAsync(): Task<List<LabelTemplate>>

Features:
- TCP printing to physical printers
- Fallback file output
- Placeholder replacement
```

**ExportService.cs** - PDF and CSV export functionality
```
Methods:
- ExportFormulationToPdfAsync(FormulationResult): Task<byte[]>
- ExportFormulationToCsvAsync(FormulationResult): Task<byte[]>

Libraries:
- QuestPDF for PDF generation
- CsvHelper for CSV creation
```

### Razor Pages (`Pages/`)

#### Root Pages

**Index.cshtml / Index.cshtml.cs**
- Dashboard with system statistics
- Navigation cards to main features
- Quick start guide

**Formulation.cshtml / Formulation.cshtml.cs**
- Product selection dropdown
- Target weight input
- Dynamic formulation table
- PDF/CSV export buttons
- Uses FormulationService

**Production.cshtml / Production.cshtml.cs**
- Batch creation form
- Active batch display
- Pack addition interface
- Label printing integration
- Batch summary metrics
- Session-based batch tracking
- Uses ProductionService, ZplPrinterService

#### Admin Pages (`Pages/Admin/`)

**Products.cshtml / Products.cshtml.cs**
- CRUD for products
- Form for name, gain %, template
- Products list table

**Ingredients.cshtml / Ingredients.cshtml.cs**
- CRUD for ingredients
- Form for name, cost per kg
- Used-in-products counter
- Delete protection for used ingredients

**Formulations.cshtml / Formulations.cshtml.cs**
- Product selection
- Dynamic ingredient addition
- Ratio percentage input
- Ingredient list with ratios
- Remove ingredient functionality

**LabelTemplates.cshtml / LabelTemplates.cshtml.cs**
- CRUD for ZPL templates
- Large text area for ZPL code
- Template preview
- Placeholder documentation
- Used-by-products counter

### REST API Controllers (`Controllers/`)

**FormulationController.cs**
```
Routes:
GET /api/formulation/calculate/{productId}/{targetWeightKg}

Returns:
- FormulationResult with ingredients and costs
- 400 BadRequest on error
```

**ProductionController.cs**
```
Routes:
POST /api/production/batch/create
POST /api/production/pack/add
GET /api/production/batch/{batchId}/summary

Payloads:
- CreateBatchRequest: productId, gainPercentage, notes
- AddPackRequest: batchId, weightKg

Returns:
- Operation results with IDs
- Batch summary with statistics
```

**PrinterController.cs**
```
Routes:
POST /api/printer/print
POST /api/printer/render
GET /api/printer/templates

Payloads:
- PrintLabelRequest: zplContent
- RenderLabelRequest: templateId, productName, weight, price

Returns:
- Rendered ZPL content
- Template list
- Print status
```

### Static Files (`wwwroot/`)

**css/site.css**
- Bootstrap 5 customizations
- Card hover effects
- Table styling
- Form input styling
- Alert and badge styles
- Production status badges
- Responsive design
- Custom utility classes

**js/site.js**
- Bootstrap popover initialization
- Bootstrap tooltip initialization
- Alert auto-dismiss
- Currency formatting
- Delete confirmation dialogs
- Form validation
- Number formatting utilities
- Session storage management

### Shared Views (`Pages/Shared/`)

**_Layout.cshtml**
- Master page template
- Bootstrap 5 navbar with dropdowns
- Admin menu with all management pages
- Swagger link
- Footer with copyright
- Bootstrap & Icons CDN links
- JavaScript bundle reference

**_ViewImports.cshtml**
- Global using directives
- Default namespace for views
- Tag helper imports

### Configuration & Deployment

**appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "Printer": {
    "IP": "localhost",
    "Port": "9100",
    "Enabled": false
  },
  "Logging": { ... }
}
```

**Dockerfile**
- Multi-stage build
- SDK stage for compilation
- Runtime stage for execution
- Health check configuration
- Volume mounts for logs and labels
- Port 80/443 exposure

## Dependency Injection Hierarchy

```
Program.cs registers:
├── DbContext (ApplicationDbContext)
├── Razor Pages (AddRazorPages)
├── Controllers (AddControllers)
├── Sessions (AddSession)
├── Swagger (AddEndpointsApiExplorer, AddSwaggerGen)
└── Services
    ├── IFormulationService → FormulationService (Scoped)
    ├── IProductionService → ProductionService (Scoped)
    ├── IZplPrinterService → ZplPrinterService (Scoped)
    └── IExportService → ExportService (Scoped)
```

## Data Flow Patterns

### Formulation Calculation Flow
```
User Input (Product + Weight)
  ↓
Page Handler (Formulation.cshtml.cs)
  ↓
FormulationService.CalculateFormulationAsync()
  ↓
EF Query for ProductFormulations with Ingredients
  ↓
Calculate quantities and costs
  ↓
Return FormulationResult
  ↓
Display in Table / Export to PDF/CSV
```

### Production Batch Flow
```
Create Batch (Product + Gain%)
  ↓
ProductionService.CreateBatchAsync()
  ↓
Store in Session (CurrentBatchId)
  ↓
Add Packs with Weight
  ↓
ProductionService.AddPackAsync()
  ↓
Calculate price (base cost × gain%)
  ↓
Retrieve Label Template
  ↓
ZplPrinterService.RenderLabelAsync()
  ↓
ZplPrinterService.PrintLabelAsync()
  ↓
Display summary and pack table
```

### Label Printing Flow
```
Template Selection
  ↓
Placeholder Values (Product, Weight, Price, Date)
  ↓
Template.Content.Replace(placeholders, values)
  ↓
TcpClient connection to printer
  ↓
Send ZPL bytes
  ↓
Success: Mark pack as printed
  ↓
Failure: Save to labels/ folder
```

## File Organization Summary

```
FoodProduction/
├── Models/                           (6 entity classes)
├── Data/
│   ├── ApplicationDbContext.cs       (EF Core configuration)
│   └── SeedData.cs                   (Initial data)
├── Services/                         (Business logic)
│   ├── FormulationService.cs
│   ├── ProductionService.cs
│   ├── ZplPrinterService.cs
│   └── ExportService.cs
├── Pages/                            (Razor Pages UI)
│   ├── Index.cshtml / .cs            (Dashboard)
│   ├── Formulation.cshtml / .cs
│   ├── Production.cshtml / .cs
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _ViewImports.cshtml
│   └── Admin/
│       ├── Products.cshtml / .cs
│       ├── Ingredients.cshtml / .cs
│       ├── Formulations.cshtml / .cs
│       └── LabelTemplates.cshtml / .cs
├── Controllers/                      (REST API)
│   ├── FormulationController.cs      (3 routes)
│   ├── ProductionController.cs       (3 routes)
│   └── PrinterController.cs          (3 routes)
├── wwwroot/
│   ├── css/site.css
│   └── js/site.js
├── Program.cs                        (Startup configuration)
├── appsettings.json                  (Configuration)
├── FoodProduction.csproj             (Project dependencies)
├── Dockerfile                        (Container definition)
├── README.md                         (Full documentation)
├── QUICK_START.md                    (Getting started guide)
└── PROJECT_STRUCTURE.md              (This file)
```

## Total Line Count Estimate

- Models: ~150 lines
- Data: ~350 lines
- Services: ~650 lines
- Pages (Razor): ~1000 lines
- Page Models: ~600 lines
- Controllers: ~200 lines
- Static Files: ~350 lines
- Configuration: ~100 lines
- **Total: ~3,400 lines of code**

## Build & Deployment

**Development:**
```bash
dotnet run
```

**Production Build:**
```bash
dotnet publish -c Release -o ./publish
```

**Docker:**
```bash
docker build -t foodproduction:latest .
docker run -p 80:80 foodproduction:latest
```

---

This structure provides a complete, scalable, and maintainable food production management system with clear separation of concerns and following ASP.NET Core best practices.

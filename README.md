# Sistema de Producao - Food Production Management System

A comprehensive Windows Forms application for managing food production, formulations, costs, and label printing using Zebra printers.

## Overview

This application has been refactored from a simple single-form label printer into a full-featured production management system with SQLite database storage. It maintains all original printing functionality while adding extensive new capabilities for managing products, formulations, costs, and generating reports.

## Features

### 1. Production Management (Producao)
- Enter production batch name, weight, and price per kg
- Calculate total price automatically
- Print labels using ZPL templates via `type file.zpl > LPT1` command
- Store all production entries in SQLite database (replacing CSV storage)
- Link productions to specific products
- View production history in real-time
- Support for custom ZPL templates

### 2. Products Management (Produtos)
- CRUD operations for products (Create, Read, Update, Delete)
- Manage both finished products and raw materials
- Set default price per kg for each product
- Search and filter products by type
- Products can be linked to productions and formulations

### 3. Formulations Management (Formulacoes)
- Create recipes with multiple ingredients
- Set yield percentage for each formulation
- Add ingredients with quantities and percentages
- Link ingredients to existing products
- View and edit formulation details
- Delete formulations (cascade deletes ingredients)

### 4. Costs Management (Custos)
- Manual entry of production costs
- Link costs to production batches
- Filter costs by date range
- Calculate cost per kg (when linked to production)
- View total costs summary
- Add notes to cost entries

### 5. Reports (Relatorios)
- View production summaries grouped by product
- View cost summaries grouped by production
- Calculate profit: Sales - Costs
- Calculate profit margin percentage
- Filter reports by date range
- **Note:** Currently uses production values as sales proxy. Will integrate with actual sales data from Flutter app via API.

## Technical Stack

- **.NET 8** - Windows Forms application
- **C# 12** - Modern C# language features
- **Entity Framework Core 8.0** - ORM for database access
- **SQLite** - Local database storage (data.db)
- **Repository Pattern** - Clean separation of data access logic

## Project Structure

```
ZebraLabelPrinter/
├── Models/                      # Database entities
│   ├── Product.cs              # Product model (finished/raw)
│   ├── Formulation.cs          # Recipe/formulation model
│   ├── FormulationIngredient.cs # Ingredient in a formulation
│   ├── Production.cs           # Production entry model
│   └── Cost.cs                 # Cost entry model
├── Data/
│   └── AppDbContext.cs         # EF Core database context
├── Repositories/               # Data access layer
│   ├── ProductRepository.cs
│   ├── FormulationRepository.cs
│   ├── ProductionRepository.cs
│   └── CostRepository.cs
├── Forms/                      # UI forms
│   ├── MainMenuForm.cs         # Main menu with navigation
│   ├── ProductionForm.cs       # Production & label printing
│   ├── ProductsForm.cs         # Products CRUD
│   ├── FormulationsForm.cs     # Formulations management
│   ├── CostsForm.cs           # Costs management
│   └── ReportsForm.cs         # Reports and profit analysis
├── Program.cs                  # Application entry point
└── README.md                   # This file
```

## Database Schema

### Products Table
- **Id** (Primary Key)
- **Name** - Product name
- **Type** - "Finished" or "Raw"
- **DefaultPricePerKg** - Default price per kilogram

### Formulations Table
- **Id** (Primary Key)
- **Name** - Formulation/recipe name
- **YieldPercentage** - Production yield percentage
- **CreatedDate** - When formulation was created

### FormulationIngredients Table
- **Id** (Primary Key)
- **FormulationId** (Foreign Key → Formulations)
- **ProductId** (Foreign Key → Products)
- **Quantity** - Ingredient quantity in kg
- **Percentage** - Percentage in recipe

### Productions Table
- **Id** (Primary Key)
- **Name** - Production batch name
- **ProductId** (Foreign Key → Products, nullable)
- **Weight** - Weight in kg
- **PricePerKg** - Price per kilogram
- **Total** - Total value (Weight × PricePerKg)
- **Date** - Production date/time
- **ZplTemplatePath** - Template used for printing

### Costs Table
- **Id** (Primary Key)
- **ProductionId** (Foreign Key → Productions, nullable)
- **ProductionName** - Production batch name
- **TotalCost** - Total cost amount
- **Date** - Cost entry date
- **Notes** - Additional information

## Getting Started

### Prerequisites
- Windows OS (for LPT1 printer access)
- .NET 8 SDK
- Visual Studio 2022 or VS Code with C# extension

### Installation

1. Clone or download the project
2. Open the project in Visual Studio or VS Code
3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

### Building the Application

```bash
# Debug build
dotnet build

# Release build
dotnet build --configuration Release
```

### Running the Application

```bash
dotnet run
```

Or build and run the executable:
```
bin\Release\net8.0-windows\ZebraLabelPrinter.exe
```

### Database Location

The SQLite database `data.db` is created automatically in the application directory on first run. Sample raw material products are seeded for testing.

## Printing Configuration

The application uses the Windows command `type file.zpl > LPT1` to send labels to the Zebra printer. Ensure your printer is mapped to LPT1:

```cmd
net use LPT1 \\computer\printer
```

### ZPL Template Variables
- `##TOTAL##` - Replaced with total price
- `##PESO##` - Replaced with weight

## Future Enhancements - API Integration

The codebase is prepared for future API integration to sync data with a Flutter mobile app. See comments in the code for detailed integration notes.

### Planned API Endpoints

When creating a Web API for Flutter synchronization:

1. **GET /api/products** - List all products
2. **GET /api/productions?startDate=...&endDate=...** - Get production data
3. **POST /api/sales** - Receive sales data from Flutter app
4. **GET /api/formulations** - Get recipes for mobile app
5. **GET /api/costs** - Get cost data

### Future Database Changes

- Add **Sales** table to track actual sales (separate from production)
- Add sync timestamps for mobile synchronization
- Add authentication tables for API security

### How to Extend with API

1. Create a new ASP.NET Core Web API project
2. Reference the Models and Data layers from this project
3. Implement API controllers for each entity
4. Add JWT authentication for security
5. The Flutter app can then:
   - Download product catalog and formulations
   - Upload sales transactions
   - View production reports remotely

## Code Documentation

All classes and methods are documented with XML comments explaining:
- Purpose and functionality
- Parameters and return values
- Database relationships
- Future extension points for API integration

## License

This is a proprietary application for food production management.

## Support

For issues or questions, please contact the development team.

---

**Last Updated:** January 2025
**Version:** 2.0 (Refactored with SQLite)
**Target Framework:** .NET 8.0

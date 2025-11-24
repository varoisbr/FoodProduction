# Database Setup Instructions

## Overview

The application uses **Entity Framework Core 8** with **SQLite** by default. The database is automatically created and seeded on the first run.

## Automatic Setup (First Run)

When you run `dotnet run` for the first time:

1. **Migrations are applied** - EF Core creates all tables from the migrations
2. **Database file is created** - `app.db` is generated automatically
3. **Seed data is inserted** - Initial data (products, ingredients, templates) is added

### What Gets Seeded

- **3 Sample Products**: Vanilla Cake, Chocolate Cake, Sugar Cookie
- **8 Ingredients**: Flour, Sugar, Butter, Eggs, Baking Powder, Vanilla Extract, Milk, Salt
- **Formulations**: Complete recipes for each product
- **1 Label Template**: Default ZPL template with placeholders

## Manual Setup (If Needed)

If the automatic setup didn't work or you want to reset:

### Step 1: Create/Reset Migrations

```bash
# Navigate to project directory
cd FoodProduction

# Remove old database if it exists
rm app.db app.db-shm app.db-wal

# Create initial migration (if not already created)
dotnet ef migrations add InitialCreate

# Apply migrations to database
dotnet ef database update
```

### Step 2: Verify Database Creation

Check that `app.db` file exists:

```bash
ls -la app.db
```

Expected output: File should be ~60KB+

### Step 3: Seed Data

Data should be seeded automatically when the application starts. To verify:

1. Run the application: `dotnet run`
2. Navigate to http://localhost:5000
3. The home page should show:
   - Products: 3
   - Ingredients: 8
   - Label Templates: 1
   - Active Batches: 0

## Database Schema

### Tables Created

```sql
-- Products
CREATE TABLE "Products" (
    "Id" INTEGER PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "DefaultGainPercentage" REAL NOT NULL,
    "DefaultLabelTemplateId" INTEGER
);

-- Ingredients
CREATE TABLE "Ingredients" (
    "Id" INTEGER PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "CostPerKg" REAL NOT NULL
);

-- ProductFormulations
CREATE TABLE "ProductFormulations" (
    "Id" INTEGER PRIMARY KEY,
    "ProductId" INTEGER NOT NULL,
    "IngredientId" INTEGER NOT NULL,
    "Ratio" REAL NOT NULL,
    FOREIGN KEY ("ProductId") REFERENCES "Products"("Id"),
    FOREIGN KEY ("IngredientId") REFERENCES "Ingredients"("Id")
);

-- ProductionBatches
CREATE TABLE "ProductionBatches" (
    "Id" INTEGER PRIMARY KEY,
    "ProductId" INTEGER NOT NULL,
    "StartDate" TEXT NOT NULL,
    "GainPercentage" REAL NOT NULL,
    "TotalCost" REAL NOT NULL,
    "Notes" TEXT,
    FOREIGN KEY ("ProductId") REFERENCES "Products"("Id")
);

-- Packs
CREATE TABLE "Packs" (
    "Id" INTEGER PRIMARY KEY,
    "BatchId" INTEGER NOT NULL,
    "WeightKg" REAL NOT NULL,
    "Price" REAL NOT NULL,
    "Printed" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    FOREIGN KEY ("BatchId") REFERENCES "ProductionBatches"("Id")
);

-- LabelTemplates
CREATE TABLE "LabelTemplates" (
    "Id" INTEGER PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL
);
```

## Connection String Configuration

### SQLite (Default)

In `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  }
}
```

### PostgreSQL (Production)

To switch to PostgreSQL:

1. Install PostgreSQL package:
   ```bash
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
   ```

2. Update `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=5432;Database=foodproduction;User Id=postgres;Password=your_password;"
     }
   }
   ```

3. Update `Program.cs`:
   ```csharp
   builder.Services.AddDbContext<ApplicationDbContext>(options =>
       options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
   );
   ```

4. Create and apply migrations:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

## Troubleshooting

### Error: "no such table: Products"

**Cause**: Migrations were not applied

**Solution**:
```bash
dotnet ef database update
```

### Error: "database is locked"

**Cause**: Multiple instances accessing the database

**Solution**:
```bash
# Close all instances
# Remove lock files
rm app.db-shm app.db-wal

# Run the app again
dotnet run
```

### Database is empty (no seed data)

**Cause**: SeedData.InitializeAsync() may have failed silently

**Solution**:
1. Check the logs in `logs/` directory
2. Delete the database: `rm app.db app.db-shm app.db-wal`
3. Restart the application: `dotnet run`

### How to Reset Everything

```bash
# Stop the application (Ctrl+C if running)

# Remove database and lock files
rm app.db app.db-shm app.db-wal

# Remove migrations folder (optional)
rm -rf Migrations

# Recreate migrations
dotnet ef migrations add InitialCreate

# Apply migrations (will create new database)
dotnet ef database update

# Run the application
dotnet run
```

## Environment-Specific Configuration

### Development

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

### Production

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db-server;Database=foodproduction;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

Use `appsettings.Production.json` for production settings.

## Migrations File Structure

The migrations are stored in the `Migrations/` folder:

```
Migrations/
├── 20251025002325_InitialCreate.cs          # Migration code
├── 20251025002325_InitialCreate.Designer.cs # Designer metadata
└── ApplicationDbContextModelSnapshot.cs      # Current model snapshot
```

## Adding a New Migration

If you modify the entity models:

```bash
# Create a new migration
dotnet ef migrations add YourMigrationName

# Apply the migration
dotnet ef database update
```

## Viewing Applied Migrations

Migrations are tracked in the `__EFMigrationsHistory` table in the database.

```bash
# List all migrations
dotnet ef migrations list
```

## Backup & Restore

### Backup Database

```bash
# For SQLite, simply copy the file
cp app.db app.db.backup
```

### Restore Database

```bash
# Restore from backup
cp app.db.backup app.db
```

## Performance Tips

1. **Add Indexes**: Foreign keys are automatically indexed
2. **Enable WAL Mode**: SQLite Write-Ahead Logging (better concurrency)
   ```bash
   sqlite3 app.db "PRAGMA journal_mode=WAL;"
   ```
3. **Connection Pooling**: Already configured by EF Core

## Resources

- [EF Core SQLite Documentation](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [SQLite Official Site](https://www.sqlite.org/)

---

**Database setup is complete!** The application will automatically create and seed the database on first run.

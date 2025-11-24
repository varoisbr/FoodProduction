# Quick Start Guide - Food Production Management System

## Prerequisites
- .NET 8 SDK installed
- Visual Studio Code, Visual Studio 2022, or any text editor
- Optional: ZPL-compatible label printer

## Step 1: First Run

```bash
# Navigate to project directory
cd FoodProduction

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

The application will:
- Create a SQLite database (`app.db`)
- Apply EF Core migrations automatically
- Seed initial data (3 sample products, 8 ingredients, label template)
- Start on `https://localhost:5001` or `http://localhost:5000`

## Step 2: Access the Application

Open your browser and navigate to:
- **Main Application**: `http://localhost:5000`
- **Swagger API Documentation**: `http://localhost:5000/swagger`

## Step 3: Explore the Features

### 1. View Home Dashboard
- Home page shows system statistics
- Quick navigation to main features

### 2. Use Formulation Calculator
1. Go to **Formulation** in the navigation bar
2. Select a sample product (e.g., "Vanilla Cake")
3. Enter a target weight (e.g., 5 kg)
4. Click **Calculate**
5. View ingredient breakdown and costs
6. Export as PDF or CSV

### 3. Test Production & Packaging
1. Go to **Production** in the navigation bar
2. Click **Create Batch** with:
   - Product: Select any product
   - Gain/Loss: 25% (or any value)
   - Notes: Optional
3. Click **Create Batch**
4. Add packs:
   - Enter measured weight (e.g., 2.5 kg)
   - Click **Add Pack & Print Label**
5. View batch summary and pack status

### 4. Manage Master Data
Go to **Admin** dropdown menu:

#### Products
- View, create, edit, delete products
- Set default gain percentage
- Assign default label template

#### Ingredients
- Manage ingredient names and costs per kg
- Used in formulations (cannot delete if in use)

#### Formulations
- Select a product
- Add ingredients with ratios
- Ratios should sum to 100 for complete formulation

#### Label Templates
- Create custom ZPL label templates
- Use placeholders: `{ProductName}`, `{Weight}`, `{Price}`, `{Date}`
- Template preview available

## Step 4: Configure Printer (Optional)

Edit `appsettings.json`:

```json
{
  "Printer": {
    "IP": "192.168.1.100",
    "Port": "9100",
    "Enabled": true
  }
}
```

- Replace IP with your ZPL printer's IP address
- Set Enabled to `true` to enable printing
- Labels will be saved to `labels/` folder if printer unavailable

## Step 5: API Testing

Use Swagger UI at `http://localhost:5000/swagger`:

### Example API Calls

**Calculate Formulation:**
```
GET /api/formulation/calculate/1/5
```

**Create Production Batch:**
```
POST /api/production/batch/create
{
  "productId": 1,
  "gainPercentage": 25,
  "notes": "Sample batch"
}
```

**Get Batch Summary:**
```
GET /api/production/batch/{batchId}/summary
```

**Render Label:**
```
POST /api/printer/render
{
  "templateId": 1,
  "productName": "Vanilla Cake",
  "weight": 2.5,
  "price": 25.50
}
```

## Directory Structure

```
FoodProduction/
├── Models/                    # Database entities
├── Data/                      # EF Core context
├── Services/                  # Business logic
├── Pages/                     # Razor Pages UI
├── Controllers/               # REST API endpoints
├── wwwroot/                   # CSS, JavaScript, assets
├── app.db                     # SQLite database (auto-created)
├── appsettings.json          # Configuration
├── Dockerfile                # Docker deployment
└── README.md                 # Full documentation
```

## Database Files

- **Database**: `app.db` (SQLite, created on first run)
- **Logs**: `logs/` directory (created automatically)
- **Labels**: `labels/` directory (fallback when printer unavailable)

## Troubleshooting

### Port Already in Use
```bash
# Change in appsettings.json or use environment variable
dotnet run --urls "http://localhost:5002"
```

### Database Errors
```bash
# Reset database
rm app.db
dotnet build
dotnet run
```

### Printer Not Connecting
- Check printer IP in `appsettings.json`
- Labels are automatically saved to `labels/` folder
- Check logs in `logs/` directory

### Missing Dependencies
```bash
dotnet restore --no-cache
dotnet clean
dotnet build
```

## Next Steps

1. **Create your products** - Go to Admin > Products
2. **Add ingredients** - Go to Admin > Ingredients with costs
3. **Define formulations** - Go to Admin > Formulations
4. **Customize labels** - Go to Admin > Label Templates
5. **Start calculating** - Use Formulation Calculator
6. **Begin production** - Create batches and print labels

## Support & Documentation

- Full documentation: See `README.md`
- API Documentation: Swagger UI at `/swagger`
- Logs: Check `logs/` directory for detailed error messages

## Sample Data

The application comes pre-loaded with:
- **Products**: Vanilla Cake, Chocolate Cake, Sugar Cookie
- **Ingredients**: Flour, Sugar, Butter, Eggs, etc. with costs
- **Formulations**: Complete recipes for each product
- **Label Template**: Default ZPL template

You can modify or delete these and add your own!

---

**Happy Producing!** 🍰

# How to Run and Test the Application

## Step 1: Build the Application

```bash
cd D:\Projetos\Food\etiqueta
dotnet build --configuration Release
```

## Step 2: Run the Application

```bash
cd bin\Release\net8.0-windows
.\ZebraLabelPrinter.exe
```

OR simply:

```bash
cd D:\Projetos\Food\etiqueta
dotnet run
```

## What Should Happen

When you run the application, you should see:

1. **First MessageBox**: "Inicializando banco de dados em: [path]\data.db"
   - Click OK

2. **Second MessageBox**: "Banco de dados criado com sucesso!"
   - Click OK

3. **Main Menu**: The main window should open with menu items:
   - Producao
   - Produtos
   - Formulacoes
   - Custos
   - Relatorios

## Testing Each Module

### Test 1: Producao
1. Click "Producao" in the menu
2. **Expected**: Production form opens with:
   - Input fields for production name, product dropdown, price, weight
   - A DataGridView showing production history (empty at first)
   - Calculate and Print buttons

**If it shows an error**, the error message will tell you exactly what went wrong!

### Test 2: Produtos
1. Click "Produtos" in the menu
2. **Expected**: Products form opens with:
   - A DataGridView (empty at first, except for 3 seed products)
   - Buttons: "Novo Produto", "Editar", "Excluir"
   - Search box and filter dropdown

**If you see seed data**: Sal, Pimenta do Reino, Alho em Po - the database is working!

### Test 3: Formulacoes
1. Click "Formulacoes" in the menu
2. **Expected**: Formulations form opens
3. Click "Nova Formulacao" button
4. Should show a form to add formulation details

### Test 4: Custos
1. Click "Custos" in the menu
2. **Expected**: Costs form opens with date filters and a grid

### Test 5: Relatorios
1. Click "Relatorios" in the menu
2. **Expected**: Reports form opens showing:
   - Financial summary (all zeros if no data)
   - Production summary grid
   - Cost summary grid

## Common Issues and Solutions

### Issue: "Erro ao inicializar banco de dados"

This means EF Core couldn't create the database. Check:
- Do you have write permissions in the application directory?
- Is SQLite DLL present in the bin folder?

### Issue: "Erro ao inicializar formulario de producao/produtos/etc"

The error message will show the exact exception. Common causes:
- Database not created
- Missing SQLite native libraries
- Permissions issue

### Issue: Nothing happens when clicking menu items

This should NOT happen anymore - we added error handlers that will show any exception!

## Verify Database Was Created

After running the app, check if the database file exists:

```bash
cd D:\Projetos\Food\etiqueta\bin\Release\net8.0-windows
ls -la data.db
```

OR

```bash
cd D:\Projetos\Food\etiqueta\bin\Debug\net8.0-windows
ls -la data.db
```

**The database file should exist!**

## Check Database Contents

You can use a SQLite viewer like:
- DB Browser for SQLite (https://sqlitebrowser.org/)
- SQLite Studio
- Or command line: `sqlite3 data.db`

To verify tables were created:
```sql
.tables
-- Should show: Costs, FormulationIngredients, Formulations, Productions, Products
```

To see seed data:
```sql
SELECT * FROM Products;
-- Should show: Sal, Pimenta do Reino, Alho em Po
```

## What to Report

If you get errors, please report:
1. The EXACT error message shown in the MessageBox
2. The StackTrace shown
3. Which menu item you clicked
4. Whether the database file exists (data.db)

With the improved error handling, we'll see EXACTLY what's failing!

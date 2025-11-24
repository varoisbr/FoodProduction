using ZebraLabelPrinter.Data;
using ZebraLabelPrinter.Forms;

namespace ZebraLabelPrinter;

/// <summary>
/// Application entry point
/// Initializes the SQLite database and launches the main menu
///
/// DATABASE LOCATION:
/// - The SQLite database (data.db) is created in the application directory
/// - Full path can be obtained via: AppDbContext.GetDatabasePath()
///
/// FUTURE API INTEGRATION NOTES:
/// When creating a Web API for Flutter app synchronization:
/// 1. Create a new ASP.NET Core Web API project
/// 2. Reference this project or share the Models/Data layers
/// 3. Create API endpoints:
///    - GET /api/products - list all products
///    - GET /api/productions?startDate=...&endDate=... - get production data
///    - POST /api/sales - receive sales data from Flutter app
///    - GET /api/formulations - get recipes for mobile app
/// 4. Add authentication (JWT tokens) for security
/// 5. Consider adding sync timestamps to track last sync
/// 6. The Flutter app can then:
///    - Download product catalog and formulations
///    - Upload sales transactions
///    - View production reports
/// </summary>
static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Set up better exception handling
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += Application_ThreadException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            // Initialize database - create tables if they don't exist
            // Database file will be created at: [Application Directory]/data.db
            // This includes seed data for testing (some raw material products)

            MessageBox.Show($"Inicializando banco de dados em:\n{AppDbContext.GetDatabasePath()}",
                "Inicializacao", MessageBoxButtons.OK, MessageBoxIcon.Information);

            AppDbContext.EnsureCreated();

            MessageBox.Show("Banco de dados criado com sucesso!",
                "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Launch the main menu form
            Application.Run(new MainMenuForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao inicializar aplicacao:\n\n{ex.Message}\n\n{ex.StackTrace}",
                "Erro Critico", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
        MessageBox.Show($"Erro na aplicacao:\n\n{e.Exception.Message}\n\n{e.Exception.StackTrace}",
            "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        MessageBox.Show($"Erro fatal:\n\n{ex?.Message}\n\n{ex?.StackTrace}",
            "Erro Fatal", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

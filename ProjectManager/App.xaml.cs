using Microsoft.Extensions.DependencyInjection;
using ProjectManager.Data;
using ProjectManager.Service;
using ProjectManager.Services;
using ProjectManager.StartUp;
using ProjectManager.ViewModels;
using ProjectManager.ViewModels.CapitalManagement;
using ProjectManager.ViewModels.Identities;
using ProjectManager.Views;
using ProjectManager.Views.Dialogs;
using System.Windows;

namespace ProjectManager
{
    public partial class App : Application
    {
        public readonly ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Database Context
            services.AddDbContext<AppDbContext>();

            // Services
            services.AddScoped<IConsumerService, ConsumerService>();
            // Add other services your ViewModels depend on here...

            // ViewModels
            services.AddTransient<ConsumerViewModel>();
            services.AddTransient<ProjectCategoryViewModel>();
            services.AddTransient<ProjectViewModel>();
            services.AddTransient<ProjectOrderViewModel>();
            services.AddTransient<UserManagementViewModel>();

            // BusinessBalanceViewModel - ensure this matches its constructor requirements
            services.AddSingleton<BusinessBalanceViewModel>(provider =>
                BusinessBalanceViewModel.Instance);

            services.AddTransient<LocationViewModel>();
            services.AddTransient<WelcomeViewModel>();

            // MainViewModel should be singleton as it's the root VM
            //services.AddSingleton<MainViewModel>();

            // Views
            services.AddSingleton<MainWindow>();
            services.AddTransient<ConsumerView>();
            // Register other views...
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Setup global exception handling first
                RegisterExceptionHandlers();

                // Show splash screen immediately
                await StartUp.SplashScreen.StartAsync(_serviceProvider);

                // Run initialization tasks sequentially
                await InitializeApplicationAsync();

            }
            catch (Exception ex)
            {
                HandleStartupFailure(ex);
                Shutdown(1); // Non-zero exit code indicates error
            }
        }

        private async Task InitializeApplicationAsync()
        {
            // Order matters - migrate first, then seed
            await AutoMigration.MigrateAsync();
            await Seed.SeedRolesAsync();
            await Seed.SeedAdminAsync();
        }

        private void RegisterExceptionHandlers()
        {
            DispatcherUnhandledException += ExceptionHandler.App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += ExceptionHandler.CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += ExceptionHandler.TaskScheduler_UnobservedTaskException;
        }

        private void HandleStartupFailure(Exception ex)
        {
            // Ensure we're on the UI thread for message boxes
            Current.Dispatcher.Invoke(() =>
            {
                new ErrorDialog("Fatal Error", $"Application failed to start: {ex.Message}");
                Shutdown(1);
            });
        }
    }
}
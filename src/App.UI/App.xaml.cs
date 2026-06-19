using System;
using System.Windows;

namespace AppUI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);
            
            // Перехват непойманных исключений
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                MessageBox.Show($"Критическая ошибка: {ex?.Message}\n{ex?.StackTrace}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Не закрываем приложение, чтобы увидеть ошибку
                // Environment.Exit(1);
            };
            
            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"Ошибка в UI: {args.Exception.Message}\n{args.Exception.StackTrace}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true; // Не закрываем приложение
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при запуске: {ex.Message}\n{ex.StackTrace}", 
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
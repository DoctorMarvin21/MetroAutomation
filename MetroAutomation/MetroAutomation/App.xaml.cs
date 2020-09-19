using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace MetroAutomation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ru-RU");

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            SetExceptionHandlers();

            base.OnStartup(e);
        }

        private void SetExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Debug.Write((Exception)e.ExceptionObject);
            };

            DispatcherUnhandledException += (s, e) =>
            {
                // Hook for OpenClipboard Failed exception.
                if (e.Exception is COMException comException && comException.ErrorCode == -2147221040)
                {
                    e.Handled = true;
                }
                else
                {
                    Debug.Write(e.Exception);
                }
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Debug.Write(e.Exception);
            };
        }
    }
}

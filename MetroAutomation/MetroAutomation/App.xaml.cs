using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        private const int ShowNormal = 1;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        protected override void OnStartup(StartupEventArgs e)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(App));
            string name = assembly.GetName().Name;

            new Mutex(true, name, out bool createdNew);

            if (!createdNew)
            {
                ActivateWindowsByProcessName(assembly.GetName().Name);
                Shutdown();
            }

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
                WriteToFile(((Exception)e.ExceptionObject).ToString());
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
                    WriteToFile(e.Exception.ToString());
                    Debug.Write(e.Exception);
                }
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                WriteToFile(e.Exception.ToString());
                Debug.Write(e.Exception);
            };
        }

        private void ActivateWindowsByProcessName(string processName)
        {
            Process[] instances = Process.GetProcessesByName(processName);

            foreach (Process instance in instances)
            {
                IntPtr windowHandle = instance.MainWindowHandle;

                if (windowHandle != IntPtr.Zero)
                {
                    ShowWindow(windowHandle, ShowNormal);
                    SetForegroundWindow(windowHandle);
                }
            }
        }

        private void WriteToFile(string text)
        {
            try
            {
                using var writer = File.AppendText("FailLog.log");
                writer.Write(text);
                writer.Flush();
            }
            catch
            {

            }
        }
    }
}

using System;
using System.Windows.Forms;

namespace UI
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FirebaseService.SetEnvironmentVariable();
            Application.Run(new FormInicioSesion());
        }
    }
}

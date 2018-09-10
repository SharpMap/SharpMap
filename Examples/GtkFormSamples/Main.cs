using System;
using System.Configuration;
using System.IO;
using System.Text;

namespace TestGtk
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            PrepareEnvironment();

		    Gtk.Application.Init();

		    using (var win = new MainWindow())
                win.Show();

		    Gtk.Application.Run();
		}

	    private static void PrepareEnvironment()
	    {
            var asr = new AppSettingsReader();
	        var msysDir = new StringBuilder((string) asr.GetValue("MSYS2InstallDir", typeof(string)));
            if (!msysDir.ToString().EndsWith("\\")) msysDir.Append('\\');
	        if (IntPtr.Size == 4)
	            msysDir.AppendFormat("mingw32\\bin");
            else
	            msysDir.AppendFormat("mingw64\\bin");

            if (!Directory.Exists(msysDir.ToString()))
                throw new Exception("MSYS INSTALLATION DIR NOT FOUND");

	        msysDir.AppendFormat(";{0}", Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process));
            Environment.SetEnvironmentVariable("PATH", msysDir.ToString(), EnvironmentVariableTarget.Process);
        }
	}
}


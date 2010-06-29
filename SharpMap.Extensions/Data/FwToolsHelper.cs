// Copyright 2009 John Diss www.newgrove.com
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 


using System;
using System.Configuration;
using System.IO;

namespace SharpMap.Extensions.Data
{
    public static class FwToolsHelper
    {
        static FwToolsHelper()
        {
            string fwtoolsPath = ConfigurationManager.AppSettings["FWToolsBinPath"];

            if (String.IsNullOrEmpty(fwtoolsPath) || !Directory.Exists(fwtoolsPath))
                throw new FwToolsPathException(fwtoolsPath);


            string path = Environment.GetEnvironmentVariable("PATH");

            string[] paths = path.Split(new[] {';', ','});

            bool pathFound = false;
            foreach (string pth in paths)
            {
                if (String.Compare(pth, fwtoolsPath, StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    pathFound = true;
                    break;
                }
            }
            if (!pathFound)
                //Environment.SetEnvironmentVariable("PATH", path + (!path.EndsWith(";") ? ";" : "") + fwtoolsPath);
                Environment.SetEnvironmentVariable("PATH", fwtoolsPath + ";" + path);

            SetFWToolsEnvironmentVariable("FWToolsProjLib", "PROJ_LIB");
            SetFWToolsEnvironmentVariable("FWToolsGeoTiffCsv", "GEOTIFF_CSV");
            SetFWToolsEnvironmentVariable("FWToolsGdalData", "GDAL_DATA");
            SetFWToolsEnvironmentVariable("FWToolsGdalDriver", "GDAL_DRIVER");

        }

        private static void SetFWToolsEnvironmentVariable(String setting, String envVariable)
        {
            string set = ConfigurationManager.AppSettings[setting];
            if (String.IsNullOrEmpty(set))
                System.Diagnostics.Debug.WriteLine(string.Format(
                                                       "\nValue for environment variable '{0}' not set!\nPlease add\n\t<add key=\"{1}\" value=\"...\"/>\n to your app.config file",
                                                       envVariable, setting));

            Environment.SetEnvironmentVariable(envVariable, set);
        }

        public static string FwToolsVersion
        {
            get { return "2.4.7"; }
        }

        public static void Configure()
        {
            //does nothing but ensure that the Static initializer has been called.
        }

        #region Nested type: OsGeo4WPathException

        public class FwToolsPathException : Exception
        {
            public FwToolsPathException(string path)
                : base(
                    string.Format("'{0}' is an Invalid Path to FWTools{1}. Create an application setting in [app|web].config key='FWToolsBinPath' pointing to the bin directory of FWTools{1} (absolute file path) . FWTools is downloaded from http://home.gdal.org/fwtools/",
                                  path, FwToolsVersion))
            {
            }
        }

        #endregion
    }
}
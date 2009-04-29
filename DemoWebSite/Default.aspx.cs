using System;
using System.Reflection;
using System.Web.UI;
using SharpMap;

public partial class _Default : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Title += " - v." + Assembly.GetAssembly(typeof (Map)).GetName().Version.ToString();
    }
}
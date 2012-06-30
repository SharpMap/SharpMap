using System;
using System.Web.UI;
using SharpMap.Utilities;

public partial class Provider : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        ProviderList.DataSource = Providers.GetProviders();
        ProviderList.DataBind();
    }
}
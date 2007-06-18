<%@ WebHandler Language="C#" Class="wms" %>

using System;
using System.Web;

public class wms : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) {
		//Get the path of this page
		string url = (context.Request.Url.Query.Length > 0 ? 
			context.Request.Url.AbsoluteUri.Replace(context.Request.Url.Query, "") : context.Request.Url.AbsoluteUri);
		SharpMap.Web.Wms.Capabilities.WmsServiceDescription description =
			new SharpMap.Web.Wms.Capabilities.WmsServiceDescription("Acme Corp. Map Server", url);

		// The following service descriptions below are not strictly required by the WMS specification.

		// Narrative description and keywords providing additional information 
		description.Abstract = "Map Server maintained by Acme Corporation. Contact: webmaster@wmt.acme.com. High-quality maps showing roadrunner nests and possible ambush locations.";
		description.Keywords = new string[3];
		description.Keywords[0] = "bird";
		description.Keywords[1] = "roadrunner";
		description.Keywords[2] = "ambush";

		//Contact information 
		description.ContactInformation.PersonPrimary.Person = "John Doe";
		description.ContactInformation.PersonPrimary.Organisation = "Acme Inc";
		description.ContactInformation.Address.AddressType = "postal";
		description.ContactInformation.Address.Country = "Neverland";
		description.ContactInformation.VoiceTelephone = "1-800-WE DO MAPS";
		//Impose WMS constraints
		description.MaxWidth = 1000; //Set image request size width
		description.MaxHeight = 500; //Set image request size height


		//Call method that sets up the map
		//We just add a dummy-size, since the wms requests will set the image-size
		SharpMap.Map myMap = MapHelper.InitializeMap(new System.Drawing.Size(1, 1));

		//Parse the request and create a response
		SharpMap.Web.Wms.WmsServer.ParseQueryString(myMap, description);
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}
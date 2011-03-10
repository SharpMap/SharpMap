// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using SharpMap.Geometries;
using SharpMap.Layers;

namespace SharpMap.Web.Wfs
{
    /// <summary>
    /// Class for generating the WmsCapabilities Xml
    /// </summary>
    public class Capabilities
    {
        private const string wfsNamespaceURI = "http://www.opengis.net/wfs";
        private const string xlinkNamespaceURI = "http://www.w3.org/1999/xlink";

        #region Nested type: WfsServiceIdentification

        public struct WfsServiceIdentification
        {
            public string Title;
            public string Abstract;
            public string[] Keywords;
            public string ServiceType;
            public string ServiceTypeVersion;
            public string Fees;
            public string AccessConstraints;

            public void WfsServiceIdentifiication(string title)
            {
                Title = title;
                Abstract = "";
                Keywords = null;
                ServiceType = "";
                ServiceTypeVersion = "";
                Fees = "";
                AccessConstraints = "";
            }
        }

        #endregion

        #region Nested type: WfsServiceProvider

        public struct WfsServiceProvider
        {
            public string ProviderName;
            public string ProviderSite;
            public ServiceContact ServiceContactDetail;

            #region Nested type: ServiceContact
            public struct ServiceContact
            {
                public string IndividualName;
                public string PositionName;
                public ContactInfo ContactInformation;
                public string Role;

                #region Nested type: ContactInfo
                public struct ContactInfo
                {
                    public Phone Telephone;
                    public Address AddressDetails;
                    public string OnlineResource;
                    public string HoursOfService;
                    public string ContactInstructions;

                    #region Nested type: Phone
                    public struct Phone
                    {
                        public string Voice;
                        public string Facsimile;  
                    }
                    #endregion

                    #region Nested type: Address
                    public struct Address
                    {
                        public string DeliveryPoint;
                        public string City;
                        public string AdministrativeArea;
                        public string PostalCode;
                        public string Country;
                        public string ElectronicMailAddress;
                        public string Voice;
                        public string Facsimile;
                    }
                    #endregion
                }
                #endregion
            }
            #endregion

        }

        #endregion

        


    }
}

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

namespace SharpMap.Web.Wfs
{
    /// <summary>
    /// Class for generating the WmsCapabilities Xml
    /// </summary>
    public class Capabilities
    {
        private const string WfsNamespaceUri = "http://www.opengis.net/wfs";
        private const string XlinkNamespaceUri = "http://www.w3.org/1999/xlink";

        #region Nested type: WfsServiceIdentification

        /// <summary>
        /// Web Feature Service identification object
        /// </summary>
        public struct WfsServiceIdentification
        {
            /// <summary>
            /// Title
            /// </summary>
            public string Title;
            /// <summary>
            /// Abstract
            /// </summary>
            public string Abstract;
            /// <summary>
            /// Keywords
            /// </summary>
            public string[] Keywords;
            /// <summary>
            /// Service type
            /// </summary>
            public string ServiceType;
            /// <summary>
            /// Service type version
            /// </summary>
            public string ServiceTypeVersion;
            /// <summary>
            /// Fees
            /// </summary>
            public string Fees;
            /// <summary>
            /// Access constraints
            /// </summary>
            public string AccessConstraints;

            /// <summary>
            /// Initializer
            /// </summary>
            /// <param name="title">The title of the Web Feature Service</param>
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

        /// <summary>
        /// Web Feature Service Service provider object
        /// </summary>
        public struct WfsServiceProvider
        {
            /// <summary>
            /// Provider name
            /// </summary>
            public string ProviderName;

            /// <summary>
            /// Provider site
            /// </summary>
            public string ProviderSite;

            /// <summary>
            /// Service contact detail
            /// </summary>
            public ServiceContact ServiceContactDetail;

            #region Nested type: ServiceContact

            /// <summary>
            /// Service contact object
            /// </summary>
            public struct ServiceContact
            {
                /// <summary>
                /// Individual name
                /// </summary>
                public string IndividualName;

                /// <summary>
                /// Position
                /// </summary>
                public string PositionName;

                /// <summary>
                /// Contact information
                /// </summary>
                public ContactInfo ContactInformation;

                /// <summary>
                /// Role
                /// </summary>
                public string Role;

                #region Nested type: ContactInfo
                
                /// <summary>
                /// Contact info structure
                /// </summary>
                public struct ContactInfo
                {
                    /// <summary>
                    /// Telephone
                    /// </summary>
                    public Phone Telephone;
                    
                    /// <summary>
                    /// Address
                    /// </summary>
                    public Address AddressDetails;
                    
                    /// <summary>
                    /// Online resource
                    /// </summary>
                    public string OnlineResource;
                    
                    /// <summary>
                    /// Hours of service
                    /// </summary>
                    public string HoursOfService;
                    
                    /// <summary>
                    /// Contact instructions
                    /// </summary>
                    public string ContactInstructions;

                    #region Nested type: Phone
                    /// <summary>
                    /// Phone structure
                    /// </summary>
                    public struct Phone
                    {
                        /// <summary>
                        /// Voice number
                        /// </summary>
                        public string Voice;
                        /// <summary>
                        /// Facsimile number
                        /// </summary>
                        public string Facsimile;  
                    }
                    #endregion

                    #region Nested type: Address
                    /// <summary>
                    /// Address structure
                    /// </summary>
                    public struct Address
                    {
                        /// <summary>
                        /// Delivery point
                        /// </summary>
                        public string DeliveryPoint;
                        /// <summary>
                        /// City
                        /// </summary>
                        public string City;
                        /// <summary>
                        /// Administrative area
                        /// </summary>
                        public string AdministrativeArea;
                        /// <summary>
                        /// Postal code
                        /// </summary>
                        public string PostalCode;
                        /// <summary>
                        /// Country
                        /// </summary>
                        public string Country;
                        /// <summary>
                        /// Email address
                        /// </summary>
                        public string ElectronicMailAddress;
                        /// <summary>
                        /// Voice number
                        /// </summary>
                        public string Voice;
                        /// <summary>
                        /// Facsimile number
                        /// </summary>
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

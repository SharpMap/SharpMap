// Copyright 2006 - Morten Nielsen (www.iter.dk)
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
using System.ComponentModel;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.Design;

namespace SharpMap.Web.UI.Ajax
{
    /// <summary>
    /// Control used for the VS designer
    /// </summary>
    public class AjaxMapControlDesigner : ContainerControlDesigner
    {
        /// <summary>
        /// Allows the control to be resized
        /// </summary>
        public override bool AllowResize
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the design-time HTML
        /// </summary>
        /// <returns></returns>
        public override string GetDesignTimeHtml()
        {
            ControlCollection childControls = ((AjaxMapControl) Component).Controls;
            return base.GetDesignTimeHtml();
        }

        /// <summary>
        /// Initializes the designer control
        /// </summary>
        /// <param name="component"></param>
        public override void Initialize(IComponent component)
        {
            if (!(component is AjaxMapControl))
                throw (new ArgumentException("Component must be an AjaxMapControl", "Component"));
            AjaxMapControl mapControl = component as AjaxMapControl;
            mapControl.Map = new Map(new Size((int) mapControl.Width.Value, (int) mapControl.Height.Value));
            base.Initialize(component);
        }
    }
}
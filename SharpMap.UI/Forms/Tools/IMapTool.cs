// Copyright 2014 - Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.UI.
// SharpMap.UI is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.UI is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Windows.Forms;
using GeoAPI.Geometries;

namespace SharpMap.Forms.Tools
{
    /// <summary>
    /// Interface for custom map tools
    /// </summary>
    public interface IMapTool
    {
        /// <summary>
        /// Gets or sets a value indicating the map the tool is to be applied to
        /// </summary>
        Map Map { get; set; }

        /// <summary>
        /// Gets the tools name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value describing the purpose of this tool
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets or sets a value indicating that this tool is enabled
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Event that is raised when the <see cref="Enabled"/> value has changed
        /// </summary>
        event EventHandler EnabledChanged;

        /// <summary>
        /// Gets or sets a value indicating how the cursor is supposed to look when this tool is active
        /// </summary>
        Cursor Cursor { get; }

        /// <summary>
        /// Event raised when the curser for this tool has changed
        /// </summary>
        event EventHandler CursorChanged;

        
        /// <summary>
        /// Function to perform some action on mouse hover
        /// </summary>
        /// <param name="mapPosition">The position at which the mouse hovers</param>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        bool DoMouseHover(Coordinate mapPosition);

        /// <summary>
        /// Function to perform some action when the mouse enters the map
        /// </summary>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        bool DoMouseEnter();

        /// <summary>
        /// Function to perform some action when the mouse leaves the map
        /// </summary>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        bool DoMouseLeave();

        /// <summary>
        /// Function to perform some action when the map was double clicked at a certain position
        /// </summary>
        /// <param name="mapPosition">The position at which the mouse hovers</param>
        /// <param name="mouseEventArgs">The mouse event arguments</param>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        bool DoMouseDoubleClick(Coordinate mapPosition, MouseEventArgs mouseEventArgs);

        /// <summary>
        /// Function to perform some action when a mouse button was "downed" on the map
        /// </summary>
        /// <param name="mapPosition">The position at which the mouse button was downed</param>
        /// <param name="mouseEventArgs">The mouse event arguments</param>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        bool DoMouseDown(Coordinate mapPosition, MouseEventArgs mouseEventArgs);

        /// <summary>
        /// Function to perform some action when a mouse button was moved on the map
        /// </summary>
        /// <param name="mapPosition">The position to which the mouse moved</param>
        /// <param name="mouseEventArgs">The mouse event arguments</param>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        bool DoMouseMove(Coordinate mapPosition, MouseEventArgs mouseEventArgs);

        /// <summary>
        /// Function to perform some action when a mouse button was "uped" on the map
        /// </summary>
        /// <param name="mapPosition">The position at which the mouse hovers</param>
        /// <param name="mouseEventArgs">The mouse event arguments</param>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        bool DoMouseUp(Coordinate mapPosition, MouseEventArgs mouseEventArgs);

        /// <summary>
        /// Function to perform some action when a mouse wheel was scrolled on the map
        /// </summary>
        /// <param name="mapPosition">The position at which the mouse hovers</param>
        /// <param name="mouseEventArgs">The mouse event arguments</param>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        bool DoMouseWheel(Coordinate mapPosition, MouseEventArgs mouseEventArgs);

        /// <summary>
        /// Some drawing operation of the tool
        /// </summary>
        /// <param name="e">The event's arguments</param>
        void DoPaint(PaintEventArgs e);

        /// <summary>
        /// Function to perform some action when a key was "downed" on the map
        /// </summary>
        /// <param name="mapPosition">The position at which the mouse hovers</param>
        /// <param name="keyEventArgs">The key event arguments</param>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        bool DoKeyDown(Coordinate mapPosition, KeyEventArgs keyEventArgs);

        /// <summary>
        /// Function to perform some action when a key was "uped" on the map
        /// </summary>
        /// <param name="mapPosition">The position at which the mouse hovers</param>
        /// <param name="keyEventArgs">The key event arguments</param>
        /// <returns><value>true</value> if the action was handled and <b>no</b> other action should be taken</returns>
        bool DoKeyUp(Coordinate mapPosition, KeyEventArgs keyEventArgs);
        
    }
}


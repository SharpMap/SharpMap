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
    public abstract class MapTool : IMapTool
    {
        //private Map _mapView;
        private bool _enabled;
        private Cursor _cursor;

        /// <summary>
        /// Creates an instance of this class assigning the 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        protected MapTool(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public Map Map { get; set; }

        public string Name { get; protected set; }

        public string Description { get; protected set; }

        public virtual void Cancel()
        {
            
        }

        /// <summary>
        /// Event that is raised when the <see cref="Enabled"/> value has changed
        /// </summary>
        public event EventHandler EnabledChanged;

        /// <summary>
        /// Gets or sets a value indicating that this tool is enabled
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value == _enabled)
                    return;
                _enabled = value;
                OnEnabledChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event invoker for the <see cref="EnabledChanged"/> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnEnabledChanged(EventArgs e)
        {
            var h = EnabledChanged;
            if (h != null) h(this, e);
        }

        /// <summary>
        /// Gets the cursor used for map operation
        /// </summary>
        public Cursor Cursor
        {
            get { return _cursor; }
            protected set
            {
                if (value == _cursor)
                    return;
                _cursor = value;
                OnCursorChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event raised when the cursor has changed
        /// </summary>
        public event EventHandler CursorChanged;

        /// <summary>
        /// Event invoker for the <see cref="CursorChanged"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCursorChanged(EventArgs e)
        {
            var handler = CursorChanged;
            if (handler != null)
                handler(this, e);
        }
        
        public virtual bool DoMouseHover(Coordinate mapPosition)
        {
            return Enabled;
        }

        public virtual bool DoMouseEnter()
        {
            return Enabled;
        }

        public virtual bool DoMouseLeave()
        {
            return Enabled;
        }

        public virtual bool DoMouseDoubleClick(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            return Enabled;
        }

        public virtual bool DoMouseDown(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            return Enabled;
        }

        public virtual bool DoMouseMove(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            return Enabled;
        }

        public virtual bool DoMouseUp(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            return Enabled;
        }

        public virtual bool DoMouseWheel(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            return Enabled;
        }

        public virtual void DoPaint(PaintEventArgs e)
        {
        }

        public virtual bool DoKeyDown(Coordinate mapPosition, KeyEventArgs keyEventArgs)
        {
            return Enabled;
        }

        public virtual bool DoKeyUp(Coordinate mapPosition, KeyEventArgs keyEventArgs)
        {
            return Enabled;
        }
    }
}
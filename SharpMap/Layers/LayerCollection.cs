// Copyright 2007 - Christian Gräfe (SharpMap@SharpTools.de)
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
using System.Collections.ObjectModel;
using System.Collections;
using SharpMap.Layers;

namespace SharpMap.Layers
{
	public class LayerCollection : CollectionBase
	{
		
	    public void Add(ILayer layer)
	    { 
	    	List.Add(layer);
	    }
	
	    public new void RemoveAt(int index)
	    {
	        List.RemoveAt(index);
	    }	
	    
	    public void Remove(ILayer layer) 
	    {
			List.Remove(layer);
	    }
	    
	    public void Insert(int index, ILayer layer) 
	    {
	        if ((index > (Count - 1)) || (index < 0))
	            throw (new Exception("Index not valid for LayerCollection!"));
	        List.Insert(index, layer);
	    }    
		
	    public virtual ILayer this[int index]
	    {
	    	get { return (ILayer) this.List[index]; }
	    	set { this.List[index] = value; }
	    }    
	    
	    public virtual ILayer this[string LayerName]
	    {
	    	get { return this.GetLayerByName(LayerName); }
	    	set { ILayer layer = this.GetLayerByName(LayerName);
	    		  layer = value; }
	    }    
	    
	    private ILayer GetLayerByName(string LayerName)
	    {
	    	for(int i=0;i<List.Count;i++)
	    		if((List[i] as ILayer).LayerName == LayerName)
	    			return (List[i] as ILayer);
	    	
	    	return null;
	    }
	    
	    protected override void OnInsert(int index, object value)
		{
	    	ILayer newLayer = (value as ILayer);
	    	
			for(int i=0;i<List.Count;i++)
				if(String.Equals(newLayer.LayerName, (List[i] as ILayer).LayerName))
					throw new Exception("This layer name already exists. Layer name: " + newLayer.LayerName);
	    }
	}
}

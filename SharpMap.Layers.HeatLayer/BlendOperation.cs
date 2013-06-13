// Copyright 2013 - Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.Layers.HeatLayer.
// SharpMap.Layers.HeatLayer is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Layers.HeatLayer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

// ***********************************************************************************************
//
// Original idea by Konstantin Vasserman, published on 
// http://www.codeproject.com/Articles/5527/Blending-of-images-raster-operations-and-basic-col
//
// ***********************************************************************************************
// 
// Modifications
// - ReSharper Renaming
//

namespace SharpMap.Layers
{
    /// <summary>
    /// List of possible blend operations
    /// </summary>
    public enum BlendOperation
    {
        SourceCopy = 1,
        RopMergePaint,
        RopNotSourceErase,
        RopSourceAnd,
        RopSourceErase,
        RopSourceInvert,
        RopSourcePaint,
        BlendDarken,
        BlendMultiply,
        BlendColorBurn,
        BlendLighten,
        BlendScreen,
        BlendColorDodge,
        BlendOverlay,
        BlendSoftLight,
        BlendHardLight,
        BlendPinLight,
        BlendDifference,
        BlendExclusion,
        BlendHue,
        BlendSaturation,
        BlendColor,
        BlendLuminosity
    }
}
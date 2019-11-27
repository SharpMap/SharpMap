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
// - removed unsafe constructs
// - removed cloning of bitmaps in 
//   - PerChannelProcess
//   - RgbProcess
// - Changed argument type from Image to Bitmap
//

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SharpMap.Layers
{

    /// <summary>
    /// Class for image blending operations.
    /// </summary>
    /// <seealso href="http://www.codeproject.com/KB/GDI-plus/KVImageProcess.aspx"/>
    /// <remarks>
    /// ToDo This class is neat, but overkill: 
    /// Only <see cref="BlendOperation.BlendMultiply"/> is used along with the 
    /// <see cref="BlendImages(System.Drawing.Bitmap,int,int,int,int,SharpMap.Layers.BlendOperation)"/> 
    /// function</remarks>
    internal static class ImageBlender
    {
        ///NTSC defined color weights
        private const float NtscRedWeight = 0.299f;

        private const float NtscGreenWeight = 0.587f;
        private const float NtscBlueWeight = 0.144f;

        private const ushort HlsMax = 360;
        private const byte RgbMax = 255;
        private const byte HueUndefined = 0;

        private delegate byte PerChannelProcessDelegate(ref byte nSrc, ref byte nDst);

        private delegate void RgbProcessDelegate(byte sR, byte sG, byte sB, ref byte dR, ref byte dG, ref byte dB);

        /// <summary>
        /// Method to invert an image
        /// </summary>
        /// <param name="img">The image to invert</param>
        public static void Invert(Image img)
        {
            if (img == null)
                throw new Exception("Image must be provided");

            var cMatrix = new ColorMatrix(new[]
                {
                    new[] {-1.0f, 0.0f, 0.0f, 0.0f, 0.0f },
                    new[] { 0.0f,-1.0f, 0.0f, 0.0f, 0.0f },
                    new[] { 0.0f, 0.0f,-1.0f, 0.0f, 0.0f },
                    new[] { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f },
                    new[] { 1.0f, 1.0f, 1.0f, 0.0f, 1.0f }
                });
            ApplyColorMatrix(img, cMatrix);
        }

        /// <summary>
        /// Method to adjust an image's brightness
        /// </summary>
        /// <param name="img">The image</param>
        /// <param name="adjValueR">Adjustment value for the red channel [-1f, 1f]</param>
        /// <param name="adjValueG">Adjustment value for the green channel [-1f, 1f]</param>
        /// <param name="adjValueB">Adjustment value for the blue channel [-1f, 1f]</param>
        public static void AdjustBrightness(Image img, float adjValueR, float adjValueG, float adjValueB)
        {
            if (img == null)
                throw new Exception("Image must be provided");

            var cMatrix = new ColorMatrix(new[]
                {
                    new[] { 1.0f, 0.0f, 0.0f, 0.0f, 0.0f },
                    new[] { 0.0f, 1.0f, 0.0f, 0.0f, 0.0f },
                    new[] { 0.0f, 0.0f, 1.0f, 0.0f, 0.0f },
                    new[] { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f },
                    new[] { adjValueR, adjValueG, adjValueB, 0.0f, 1.0f }
                });
            ApplyColorMatrix(img, cMatrix);
        }

        /// <summary>
        /// Method to adjust an image's brightness
        /// </summary>
        /// <param name="img">The image</param>
        /// <param name="adjValue">Adjustment value for all channels [-1f, 1f]</param>
        public static void AdjustBrightness(Image img, float adjValue)
        {
            AdjustBrightness(img, adjValue, adjValue, adjValue);
        }

        // Saturation. 0.0 = desaturate, 1.0 = identity, -1.0 = complementary colors
        public static void AdjustSaturation(Image img, float sat, float rweight, float gweight, float bweight)
        {
            if (img == null)
                throw new Exception("Image must be provided");

            var cMatrix = new ColorMatrix(new[] {
                new [] { (1.0f-sat)*rweight+sat, (1.0f-sat)*rweight, (1.0f-sat)*rweight, 0.0f, 0.0f },
                new [] { (1.0f-sat)*gweight, (1.0f-sat)*gweight+sat, (1.0f-sat)*gweight, 0.0f, 0.0f },
                new [] { (1.0f-sat)*bweight, (1.0f-sat)*bweight, (1.0f-sat)*bweight+sat, 0.0f, 0.0f },
                new [] { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f },
                new [] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f }
            });
            ApplyColorMatrix(img, cMatrix);
        }

        // Saturation. 0.0 = desaturate, 1.0 = identity, -1.0 = complementary colors
        public static void AdjustSaturation(Image img, float sat)
        {
            AdjustSaturation(img, sat, NtscRedWeight, NtscGreenWeight, NtscBlueWeight);
        }

        // Weights between 0.0 and 1.0
        public static void Desaturate(Image img, float redWeight, float greenWeight, float blueWeight)
        {
            AdjustSaturation(img, 0.0f, redWeight, greenWeight, blueWeight);
        }

        // Desaturate using "default" NTSC defined color weights
        public static void Desaturate(Image img)
        {
            AdjustSaturation(img, 0.0f, NtscRedWeight, NtscGreenWeight, NtscBlueWeight);
        }

        public static void ApplyColorMatrix(Image img, ColorMatrix colMatrix)
        {
            using (var gr = Graphics.FromImage(img))
            {
                var attrs = new ImageAttributes();
                attrs.SetColorMatrix(colMatrix);
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height),
                             0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attrs);
            }
        }

        #region BlendImages functions ...

        /*
			destImage - image that will be used as background
			destX, destY - define position on destination image where to start applying blend operation
			destWidth, destHeight - width and height of the area to apply blending
			srcImage - image to use as foreground (source of blending)
			srcX, srcY - starting position of the source image
		*/

        public static void BlendImages(Bitmap destImage, int destX, int destY, int destWidth, int destHeight,
                                       Bitmap srcImage, int srcX, int srcY, BlendOperation blendOp)
        {
            if (destImage == null)
                throw new Exception("Destination image must be provided");

            if (destImage.Width < destX + destWidth || destImage.Height < destY + destHeight)
                throw new Exception("Destination image is smaller than requested dimensions");

            if (srcImage == null)
                throw new Exception("Source image must be provided");

            if (srcImage.Width < srcX + destWidth || srcImage.Height < srcY + destHeight)
                throw new Exception("Source image is smaller than requested dimentions");

            Bitmap tempBmp = null;
            using (var gr = Graphics.FromImage(destImage))
            {
                gr.CompositingMode = CompositingMode.SourceCopy;

                switch (blendOp)
                {
                    case BlendOperation.SourceCopy:
                        gr.DrawImage(srcImage, new Rectangle(destX, destY, destWidth, destHeight),
                                     srcX, srcY, destWidth, destHeight, GraphicsUnit.Pixel);
                        break;

                    case BlendOperation.RopMergePaint:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, MergePaint);
                        break;

                    case BlendOperation.RopNotSourceErase:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, NotSourceErase);
                        break;

                    case BlendOperation.RopSourceAnd:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, SourceAnd);
                        break;

                    case BlendOperation.RopSourceErase:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, SourceErase);
                        break;

                    case BlendOperation.RopSourceInvert:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, SourceInvert);
                        break;

                    case BlendOperation.RopSourcePaint:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, SourcePaint);
                        break;

                    case BlendOperation.BlendDarken:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, BlendDarken);
                        break;

                    case BlendOperation.BlendMultiply:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, BlendMultiply);
                        break;

                    case BlendOperation.BlendScreen:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, BlendScreen);
                        break;

                    case BlendOperation.BlendLighten:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, BlendLighten);
                        break;

                    case BlendOperation.BlendHardLight:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, BlendHardLight);
                        break;

                    case BlendOperation.BlendDifference:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, BlendDifference);
                        break;

                    case BlendOperation.BlendPinLight:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, BlendPinLight);
                        break;

                    case BlendOperation.BlendOverlay:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, BlendOverlay);
                        break;

                    case BlendOperation.BlendExclusion:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, BlendExclusion);
                        break;

                    case BlendOperation.BlendSoftLight:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, BlendSoftLight);
                        break;

                    case BlendOperation.BlendColorBurn:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, BlendColorBurn);
                        break;

                    case BlendOperation.BlendColorDodge:
                        tempBmp = PerChannelProcess(destImage, destX, destY, destWidth, destHeight,
                                                    srcImage, srcX, srcY, BlendColorDodge);
                        break;

                    case BlendOperation.BlendHue:
                        tempBmp = RgbProcess(destImage, destX, destY, destWidth, destHeight,
                                             srcImage, srcX, srcY, BlendHue);
                        break;

                    case BlendOperation.BlendSaturation:
                        tempBmp = RgbProcess(destImage, destX, destY, destWidth, destHeight,
                                             srcImage, srcX, srcY, BlendSaturation);
                        break;

                    case BlendOperation.BlendColor:
                        tempBmp = RgbProcess(destImage, destX, destY, destWidth, destHeight,
                                             srcImage, srcX, srcY, BlendColor);
                        break;

                    case BlendOperation.BlendLuminosity:
                        tempBmp = RgbProcess(destImage, destX, destY, destWidth, destHeight,
                                             srcImage, srcX, srcY, BlendLuminosity);
                        break;
                }

                if (tempBmp != null && tempBmp != destImage)
                {
                    gr.DrawImage(tempBmp, 0, 0, tempBmp.Width, tempBmp.Height);
                    tempBmp.Dispose();
                }
            }
        }

        public static void BlendImages(Bitmap destImage, Bitmap srcImage, BlendOperation blendOp)
        {
            BlendImages(destImage, 0, 0, destImage.Width, destImage.Height, srcImage, 0, 0, blendOp);
        }

        public static void BlendImages(Bitmap destImage, BlendOperation blendOp)
        {
            BlendImages(destImage, 0, 0, destImage.Width, destImage.Height, null, 0, 0, blendOp);
        }

        public static void BlendImages(Bitmap destImage, int destX, int destY, BlendOperation blendOp)
        {
            BlendImages(destImage, destX, destY, destImage.Width - destX, destImage.Height - destY, null, 0, 0, blendOp);
        }

        public static void BlendImages(Bitmap destImage, int destX, int destY, int destWidth, int destHeight, BlendOperation blendOp)
        {
            BlendImages(destImage, destX, destY, destWidth, destHeight, null, 0, 0, blendOp);
        }

        #endregion BlendImages functions ...

        #region Private Blending Functions ...

        private static Bitmap PerChannelProcess(Bitmap destImg, int destX, int destY, int destWidth, int destHeight,
                                                Bitmap srcImg, int srcX, int srcY,
                                                PerChannelProcessDelegate channelProcessFunction)
        {
            var dst = destImg; // new Bitmap(destImg);
            var dstData = dst.LockBits(new Rectangle(destX, destY, destWidth, destHeight), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var dstStride = Math.Abs(dstData.Stride);
            var dstScan0 = dstData.Scan0;
            var dstBuffer = new byte[dstStride];

            var src = srcImg; //new Bitmap(srcImg);
            var srcData = src.LockBits(new Rectangle(srcX, srcY, destWidth, destHeight), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var srcStride = Math.Abs(srcData.Stride);
            var srcScan0 = srcData.Scan0;
            var srcBuffer = new byte[srcStride];

            for (var y = 0; y < destHeight; y++)
            {
                System.Runtime.InteropServices.Marshal.Copy(srcScan0 + y * srcStride, srcBuffer, 0, srcStride);
                System.Runtime.InteropServices.Marshal.Copy(dstScan0 + y * dstStride, dstBuffer, 0, dstStride);

                for (var x = 0; x < 3* destWidth; x++)
                {
                    dstBuffer[x] = channelProcessFunction(ref srcBuffer[x], ref dstBuffer[x]);
                }

                System.Runtime.InteropServices.Marshal.Copy(dstBuffer, 0, dstScan0 + y * dstStride, dstStride);
            }

            //unsafe
            //{
            //    byte* pDst = (byte*)(void*)dstScan0;
            //    byte* pSrc = (byte*)(void*)srcScan0;

            //    for (int y = 0; y < destHeight; y++)
            //    {
            //        for (int x = 0; x < destWidth * 3; x++)
            //        {
            //            pDst[x + y * dstStride] = channelProcessFunction(ref pSrc[x + y * srcStride], ref pDst[x + y * dstStride]);
            //        }
            //    }
            //}

            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);

            return dst;
        }

        private static Bitmap RgbProcess(Bitmap destImg, int destX, int destY, int destWidth, int destHeight,
                                         Bitmap srcImg, int srcX, int srcY,
                                         RgbProcessDelegate rgbProcessFunction)
        {
            var dst = destImg; //new Bitmap(destImg);
            var dstData = dst.LockBits(new Rectangle(destX, destY, destWidth, destHeight), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var dstStride = Math.Abs(dstData.Stride);
            var dstScan0 = dstData.Scan0;
            var dstBuffer = new byte[dstStride];

            var src = srcImg; //new Bitmap(srcImg);
            var srcData = src.LockBits(new Rectangle(srcX, srcY, destWidth, destHeight), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var srcStride = Math.Abs(srcData.Stride);
            var srcScan0 = srcData.Scan0;
            var srcBuffer = new byte[srcStride];

            for (var y = 0; y < destHeight; y++)
            {
                System.Runtime.InteropServices.Marshal.Copy(srcScan0 + y * srcStride, srcBuffer, 0, srcStride);
                System.Runtime.InteropServices.Marshal.Copy(dstScan0 + y * dstStride, dstBuffer, 0, dstStride);

                for (var x = 0; x < destWidth; x += 3)
                {
                    rgbProcessFunction(srcBuffer[x + 2], srcBuffer[x + 1], srcBuffer[x],
                                       ref dstBuffer[x + 2], ref dstBuffer[x + 1], ref dstBuffer[x]);
                }

                System.Runtime.InteropServices.Marshal.Copy(dstBuffer, 0, dstScan0 + y * dstStride, dstStride);
            }

            //unsafe
            //{
            //    byte* pDst = (byte*)(void*)dstScan0;
            //    byte* pSrc = (byte*)(void*)srcScan0;

            //    for (int y = 0; y < destHeight; y++)
            //    {
            //        for (int x = 0; x < destWidth; x++)
            //        {
            //            rgbProcessFunction(
            //                pSrc[x * 3 + 2 + y * srcStride], pSrc[x * 3 + 1 + y * srcStride], pSrc[x * 3 + y * srcStride],
            //                ref pDst[x * 3 + 2 + y * dstStride], ref pDst[x * 3 + 1 + y * dstStride], ref pDst[x * 3 + y * dstStride]
            //                );
            //        }
            //    }
            //}

            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);

            return dst;
        }

        #endregion Private Blending Functions ...

        #region HLS Conversion Functions ...

        private static void RgbToHls(byte red, byte green, byte blue, out ushort hue, out ushort lightness, out ushort saturation)
        {
            /* calculate lightness */
            var cMax = Math.Max(Math.Max(red, green), blue);
            var cMin = Math.Min(Math.Min(red, green), blue);
            lightness = (ushort)((((cMax + cMin) * HlsMax) + RgbMax) / (2 * RgbMax));

            if (cMax == cMin)
            {   /* r=g=b --> achromatic case */
                saturation = 0;          /* saturation */
                hue = HueUndefined;      /* hue */
            }
            else
            {   /* chromatic case */
                /* saturation */
                if (lightness <= (HlsMax / 2))
                    saturation = (ushort)((((cMax - cMin) * HlsMax) + ((cMax + cMin) / 2)) / (cMax + cMin));
                else
                    saturation = (ushort)((((cMax - cMin) * HlsMax) + ((2 * RgbMax - cMax - cMin) / 2)) / (2 * RgbMax - cMax - cMin));

                /* hue */
                var redDelta = (((cMax - red) * (HlsMax / 6f)) + ((cMax - cMin) / 2f)) / (cMax - cMin);
                var greendelta = (((cMax - green) * (HlsMax / 6f)) + ((cMax - cMin) / 2f)) / (cMax - cMin);
                var blueDelta = (((cMax - blue) * (HlsMax / 6f)) + ((cMax - cMin) / 2f)) / (cMax - cMin);

                if (red == cMax)
                    hue = (ushort)(blueDelta - greendelta);
                else if (green == cMax)
                    hue = (ushort)((HlsMax / 3) + redDelta - blueDelta);
                else /* B == cMax */
                    hue = (ushort)(((2 * HlsMax) / 3) + greendelta - redDelta);

                //if (hue < 0)
                //    hue += HlsMax;
                if (hue > HlsMax)
                    hue -= HlsMax;
            }
        }

        private static void HlsToRgb(ushort hue, ushort lightness, ushort saturation, out byte red, out byte green, out byte blue)
        {
            if (saturation == 0)
            {/* achromatic case */
                red = green = blue = (byte)((lightness * RgbMax) / HlsMax);
            }
            else
            {/* chromatic case */
                /* set up magic numbers */
                float magic2;       /* calculated magic numbers (really!) */
                if (lightness <= (HlsMax / 2))
                    magic2 = (lightness * (HlsMax + saturation) + (HlsMax / 2f)) / HlsMax;
                else
                    magic2 = lightness + saturation - ((lightness * saturation) + (HlsMax / 2f)) / HlsMax;

                var magic1 = 2 * lightness - magic2;       /* calculated magic number 1 (really!) */

                /* get RGB, change units from HLSMAX to RGBMAX */
                red = (byte)((HueToRgb(magic1, magic2, hue + (HlsMax / 3)) * RgbMax + (HlsMax / 2)) / HlsMax);
                green = (byte)((HueToRgb(magic1, magic2, hue) * RgbMax + (HlsMax / 2)) / HlsMax);
                blue = (byte)((HueToRgb(magic1, magic2, hue - (HlsMax / 3)) * RgbMax + (HlsMax / 2)) / HlsMax);
            }
        }

        /* utility routine for HLStoRGB */

        private static float HueToRgb(float n1, float n2, float hue)
        {
            /* range check: note values passed add/subtract thirds of range */
            if (hue < 0)
                hue += HlsMax;

            if (hue > HlsMax)
                hue -= HlsMax;

            /* return r,g, or b value from this tridrant */
            if (hue < (HlsMax / 6))
                return n1 + (((n2 - n1) * hue + (HlsMax / 12)) / (HlsMax / 6));
            if (hue < (HlsMax / 2))
                return n2;
            if (hue < ((HlsMax * 2) / 3))
                return n1 + (((n2 - n1) * (((HlsMax * 2) / 3) - hue) + (HlsMax / 12)) / (HlsMax / 6));
            return n1;
        }

        #endregion HLS Conversion Functions ...

        #region Raster Operation Functions ...

        // (NOT Source) OR Destination
        private static byte MergePaint(ref byte src, ref byte dst)
        {
            return (byte)Math.Max(Math.Min((255 - src) | dst, 255), 0);
        }

        // NOT (Source OR Destination)
        private static byte NotSourceErase(ref byte src, ref byte dst)
        {
            return (byte)Math.Max(Math.Min(255 - (src | dst), 255), 0);
        }

        // Source AND Destination
        private static byte SourceAnd(ref byte src, ref byte dst)
        {
            return (byte)Math.Max(Math.Min(src & dst, 255), 0);
        }

        // Source AND (NOT Destination)
        private static byte SourceErase(ref byte src, ref byte dst)
        {
            return (byte)Math.Max(Math.Min(src & (255 - dst), 255), 0);
        }

        // Source XOR Destination
        private static byte SourceInvert(ref byte src, ref byte dst)
        {
            return (byte)Math.Max(Math.Min(src ^ dst, 255), 0);
        }

        // Source OR Destination
        private static byte SourcePaint(ref byte src, ref byte dst)
        {
            return (byte)Math.Max(Math.Min(src | dst, 255), 0);
        }

        #endregion Raster Operation Functions ...

        #region Blend Pixels Functions ...

        // Choose darkest color
        private static byte BlendDarken(ref byte src, ref byte dst)
        {
            return ((src < dst) ? src : dst);
        }

        // Multiply
        private static byte BlendMultiply(ref byte src, ref byte dst)
        {
            return (byte)Math.Max(Math.Min((src / 255.0f * dst / 255.0f) * 255.0f, 255), 0);
        }

        // Screen
        private static byte BlendScreen(ref byte src, ref byte dst)
        {
            return (byte)Math.Max(Math.Min(255 - ((255 - src) / 255.0f * (255 - dst) / 255.0f) * 255.0f, 255), 0);
        }

        // Choose lightest color
        private static byte BlendLighten(ref byte src, ref byte dst)
        {
            return ((src > dst) ? src : dst);
        }

        // hard light
        private static byte BlendHardLight(ref byte src, ref byte dst)
        {
            return ((src < 128)
                        ? (byte)Math.Max(Math.Min((src / 255.0f * dst / 255.0f) * 255.0f * 2, 255), 0)
                        : (byte)Math.Max(Math.Min(255 - ((255 - src) / 255.0f * (255 - dst) / 255.0f) * 255.0f * 2, 255), 0));
        }

        // difference
        private static byte BlendDifference(ref byte src, ref byte dst)
        {
            return (byte)((src > dst) ? src - dst : dst - src);
        }

        // pin light
        private static byte BlendPinLight(ref byte src, ref byte dst)
        {
            return (src < 128) ? ((dst > src) ? src : dst) : ((dst < src) ? src : dst);
        }

        // overlay
        private static byte BlendOverlay(ref byte src, ref byte dst)
        {
            return ((dst < 128)
                        ? (byte)Math.Max(Math.Min((src / 255.0f * dst / 255.0f) * 255.0f * 2, 255), 0)
                        : (byte)Math.Max(Math.Min(255 - ((255 - src) / 255.0f * (255 - dst) / 255.0f) * 255.0f * 2, 255), 0));
        }

        // exclusion
        private static byte BlendExclusion(ref byte src, ref byte dst)
        {
            return (byte)(src + dst - 2 * (dst * src) / 255f);
        }

        // Soft Light (XFader formula)
        private static byte BlendSoftLight(ref byte src, ref byte dst)
        {
            return (byte)Math.Max(Math.Min((dst * src / 255f) + dst * (255 - ((255 - dst) * (255 - src) / 255f) - (dst * src / 255f)) / 255f, 255), 0);
        }

        // Color Burn
        private static byte BlendColorBurn(ref byte src, ref byte dst)
        {
            return (src == 0) ? (byte)0 : (byte)Math.Max(Math.Min(255 - (((255 - dst) * 255) / src), 255), 0);
        }

        // Color Dodge
        private static byte BlendColorDodge(ref byte src, ref byte dst)
        {
            return (src == 255) ? (byte)255 : (byte)Math.Max(Math.Min((dst * 255) / (255 - src), 255), 0);
        }

        // use source Hue
        private static void BlendHue(byte sR, byte sG, byte sB, ref byte dR, ref byte dG, ref byte dB)
        {
            ushort sH, sL, sS, dH, dL, dS;
            RgbToHls(sR, sG, sB, out sH, out sL, out sS);
            RgbToHls(dR, dG, dB, out dH, out dL, out dS);
            HlsToRgb(sH, dL, dS, out dR, out dG, out dB);
        }

        // use source Saturation
        private static void BlendSaturation(byte sR, byte sG, byte sB, ref byte dR, ref byte dG, ref byte dB)
        {
            ushort sH, sL, sS, dH, dL, dS;
            RgbToHls(sR, sG, sB, out sH, out sL, out sS);
            RgbToHls(dR, dG, dB, out dH, out dL, out dS);
            HlsToRgb(dH, dL, sS, out dR, out dG, out dB);
        }

        // use source Color
        private static void BlendColor(byte sR, byte sG, byte sB, ref byte dR, ref byte dG, ref byte dB)
        {
            ushort sH, sL, sS, dH, dL, dS;
            RgbToHls(sR, sG, sB, out sH, out sL, out sS);
            RgbToHls(dR, dG, dB, out dH, out dL, out dS);
            HlsToRgb(sH, dL, sS, out dR, out dG, out dB);
        }

        // use source Luminosity
        private static void BlendLuminosity(byte sR, byte sG, byte sB, ref byte dR, ref byte dG, ref byte dB)
        {
            ushort sH, sL, sS, dH, dL, dS;
            RgbToHls(sR, sG, sB, out sH, out sL, out sS);
            RgbToHls(dR, dG, dB, out dH, out dL, out dS);
            HlsToRgb(dH, sL, dS, out dR, out dG, out dB);
        }

        #endregion Blend Pixels Functions ...
    }
}

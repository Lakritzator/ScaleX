#region Greenshot GNU General Public License

// Greenshot - a free and open source screenshot tool
// Copyright (C) 2007-2018 Thomas Braun, Jens Klingen, Robin Krom
// 
// For more information see: http://getgreenshot.org/
// The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 1 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dapplo.Log;
using Dapplo.Windows.Common.Extensions;
using Dapplo.Windows.Common.Structs;
using ScaleX.Legacy.Scaler.FastBitmap;

namespace ScaleX.Legacy.Scaler
{
    /// <summary>
    ///     The BitmapHelper contains extensions for Bitmaps
    /// </summary>
    public static class BitmapHelper
    {
        private static readonly LogSource Log = new LogSource();

        /// <summary>
        ///     Create a Thumbnail
        /// </summary>
        /// <param name="image">Image</param>
        /// <param name="thumbWidth">int</param>
        /// <param name="thumbHeight">int</param>
        /// <param name="maxWidth">int</param>
        /// <param name="maxHeight">int</param>
        /// <returns></returns>
        public static Bitmap CreateThumbnail(this Image image, int thumbWidth, int thumbHeight, int maxWidth = -1, int maxHeight = -1)
        {
            var srcWidth = image.Width;
            var srcHeight = image.Height;
            if (thumbHeight < 0)
            {
                thumbHeight = (int)(thumbWidth * (srcHeight / (float)srcWidth));
            }
            if (thumbWidth < 0)
            {
                thumbWidth = (int)(thumbHeight * (srcWidth / (float)srcHeight));
            }
            if (maxWidth > 0 && thumbWidth > maxWidth)
            {
                thumbWidth = Math.Min(thumbWidth, maxWidth);
                thumbHeight = (int)(thumbWidth * (srcHeight / (float)srcWidth));
            }
            if (maxHeight > 0 && thumbHeight > maxHeight)
            {
                thumbHeight = Math.Min(thumbHeight, maxHeight);
                thumbWidth = (int)(thumbHeight * (srcWidth / (float)srcHeight));
            }

            var bmp = new Bitmap(thumbWidth, thumbHeight);
            using (var graphics = Graphics.FromImage(bmp))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var rectDestination = new NativeRect(0, 0, thumbWidth, thumbHeight);
                graphics.DrawImage(image, rectDestination, 0, 0, srcWidth, srcHeight, GraphicsUnit.Pixel);
            }
            return bmp;
        }

        /// <summary>
        ///     Crops the image to the specified rectangle
        /// </summary>
        /// <param name="bitmap">Bitmap to crop</param>
        /// <param name="cropRectangle">NativeRect with bitmap coordinates, will be "intersected" to the bitmap</param>
        public static bool Crop(ref Bitmap bitmap, ref NativeRect cropRectangle)
        {
            if (bitmap.Width * bitmap.Height > 0)
            {
                cropRectangle = cropRectangle.Intersect(new NativeRect(0, 0, bitmap.Width, bitmap.Height));
                if (cropRectangle.Width != 0 || cropRectangle.Height != 0)
                {
                    var returnImage = bitmap.CloneBitmap(PixelFormat.DontCare, cropRectangle);
                    bitmap.Dispose();
                    bitmap = returnImage;
                    return true;
                }
            }
            Log.Warn().WriteLine("Can't crop a null/zero size image!");
            return false;
        }

        /// <summary>
        ///     This method fixes the problem that we can't apply a filter outside the target bitmap,
        ///     therefor the filtered-bitmap will be shifted if we try to draw it outside the target bitmap.
        ///     It will also account for the Invert flag.
        /// </summary>
        /// <param name="applySize"></param>
        /// <param name="rect"></param>
        /// <param name="invert"></param>
        /// <returns></returns>
        public static NativeRect CreateIntersectRectangle(Size applySize, NativeRect rect, bool invert)
        {
            NativeRect myRect;
            if (invert)
            {
                myRect = new NativeRect(0, 0, applySize.Width, applySize.Height);
            }
            else
            {
                var applyRect = new NativeRect(0, 0, applySize.Width, applySize.Height);
                myRect = new NativeRect(rect.X, rect.Y, rect.Width, rect.Height).Intersect(applyRect);
            }
            return myRect;
        }


        /// <summary>
        ///     Apply a color matrix to the image
        /// </summary>
        /// <param name="source">Image to apply matrix to</param>
        /// <param name="colorMatrix">ColorMatrix to apply</param>
        public static void ApplyColorMatrix(this Bitmap source, ColorMatrix colorMatrix)
        {
            source.ApplyColorMatrix(NativeRect.Empty, source, NativeRect.Empty, colorMatrix);
        }

        /// <summary>
        ///     Apply a color matrix by copying from the source to the destination
        /// </summary>
        /// <param name="source">Image to copy from</param>
        /// <param name="sourceRect">NativeRect to copy from</param>
        /// <param name="destRect">NativeRect to copy to</param>
        /// <param name="dest">Image to copy to</param>
        /// <param name="colorMatrix">ColorMatrix to apply</param>
        public static void ApplyColorMatrix(this Bitmap source, NativeRect sourceRect, Bitmap dest, NativeRect destRect, ColorMatrix colorMatrix)
        {
            using (var imageAttributes = new ImageAttributes())
            {
                imageAttributes.ClearColorMatrix();
                imageAttributes.SetColorMatrix(colorMatrix);
                source.ApplyImageAttributes(sourceRect, dest, destRect, imageAttributes);
            }
        }

        /// <summary>
        ///     Apply image attributes to the image
        /// </summary>
        /// <param name="source">Image to apply matrix to</param>
        /// <param name="imageAttributes">ImageAttributes to apply</param>
        public static void ApplyColorMatrix(this Bitmap source, ImageAttributes imageAttributes)
        {
            source.ApplyImageAttributes(NativeRect.Empty, source, NativeRect.Empty, imageAttributes);
        }

        /// <summary>
        ///     Apply a color matrix by copying from the source to the destination
        /// </summary>
        /// <param name="source">Image to copy from</param>
        /// <param name="sourceRect">NativeRect to copy from</param>
        /// <param name="destRect">NativeRect to copy to</param>
        /// <param name="dest">Image to copy to</param>
        /// <param name="imageAttributes">ImageAttributes to apply</param>
        public static void ApplyImageAttributes(this Bitmap source, NativeRect sourceRect, Bitmap dest, NativeRect destRect, ImageAttributes imageAttributes)
        {
            if (sourceRect == NativeRect.Empty)
            {
                sourceRect = new NativeRect(0, 0, source.Width, source.Height);
            }
            if (dest == null)
            {
                dest = source;
            }
            if (destRect == NativeRect.Empty)
            {
                destRect = new NativeRect(0, 0, dest.Width, dest.Height);
            }
            using (var graphics = Graphics.FromImage(dest))
            {
                // Make sure we draw with the best quality!
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingMode = CompositingMode.SourceCopy;

                graphics.DrawImage(source, destRect, sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height, GraphicsUnit.Pixel, imageAttributes);
            }
        }

        /// <summary>
        ///     Checks if the supplied Bitmap has a PixelFormat we support
        /// </summary>
        /// <param name="image">bitmap to check</param>
        /// <returns>bool if we support it</returns>
        public static bool IsPixelFormatSupported(this Image image)
        {
            return image.PixelFormat.IsPixelFormatSupported();
        }

        /// <summary>
        ///     Checks if we support the pixel format
        /// </summary>
        /// <param name="pixelformat">PixelFormat to check</param>
        /// <returns>bool if we support it</returns>
        public static bool IsPixelFormatSupported(this PixelFormat pixelformat)
        {
            return pixelformat.Equals(PixelFormat.Format32bppArgb) ||
                   pixelformat.Equals(PixelFormat.Format32bppPArgb) ||
                   pixelformat.Equals(PixelFormat.Format32bppRgb) ||
                   pixelformat.Equals(PixelFormat.Format24bppRgb);
        }


        /// <summary>
        ///     Rotate the bitmap
        /// </summary>
        /// <param name="sourceBitmap">Image</param>
        /// <param name="rotateFlipType">RotateFlipType</param>
        /// <returns>Image</returns>
        public static Bitmap ApplyRotateFlip(this Bitmap sourceBitmap, RotateFlipType rotateFlipType)
        {
            var returnImage = sourceBitmap.CloneBitmap();
            returnImage.RotateFlip(rotateFlipType);
            return returnImage;
        }


        /// <summary>
        ///     Get a scaled version of the sourceBitmap
        /// </summary>
        /// <param name="sourceImage">Image</param>
        /// <param name="percent">1-99 to make smaller, use 101 and more to make the picture bigger</param>
        /// <returns>Bitmap</returns>
        public static Bitmap ScaleByPercent(this Image sourceImage, int percent)
        {
            var nPercent = (float)percent / 100;

            var sourceWidth = sourceImage.Width;
            var sourceHeight = sourceImage.Height;
            var destWidth = (int)(sourceWidth * nPercent);
            var destHeight = (int)(sourceHeight * nPercent);

            var scaledBitmap = BitmapFactory.CreateEmpty(destWidth, destHeight, sourceImage.PixelFormat, Color.Empty, sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
            using (var graphics = Graphics.FromImage(scaledBitmap))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(sourceImage, new NativeRect(0, 0, destWidth, destHeight), new NativeRect(0, 0, sourceWidth, sourceHeight), GraphicsUnit.Pixel);
            }
            return scaledBitmap;
        }

        /// <summary>
        ///     Resize canvas with pixel to the left, right, top and bottom
        /// </summary>
        /// <param name="sourceImage">Image</param>
        /// <param name="backgroundColor">The color to fill with, or Color.Empty to take the default depending on the pixel format</param>
        /// <param name="left">int</param>
        /// <param name="right">int</param>
        /// <param name="top">int</param>
        /// <param name="bottom">int</param>
        /// <param name="matrix">Matrix</param>
        /// <returns>a new bitmap with the source copied on it</returns>
        public static Bitmap ResizeCanvas(this Image sourceImage, Color backgroundColor, int left, int right, int top, int bottom, Matrix matrix)
        {
            matrix.Translate(left, top, MatrixOrder.Append);
            var newBitmap = BitmapFactory.CreateEmpty(sourceImage.Width + left + right, sourceImage.Height + top + bottom, sourceImage.PixelFormat, backgroundColor,
                sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
            using (var graphics = Graphics.FromImage(newBitmap))
            {
                graphics.DrawImageUnscaled(sourceImage, left, top);
            }
            return newBitmap;
        }

        /// <summary>
        ///     Wrapper for the more complex Resize, this resize could be used for e.g. Thumbnails
        /// </summary>
        /// <param name="sourceImage">Image</param>
        /// <param name="maintainAspectRatio">true to maintain the aspect ratio</param>
        /// <param name="newWidth">int</param>
        /// <param name="newHeight">int</param>
        /// <param name="matrix">Matrix</param>
        /// <param name="interpolationMode">InterpolationMode</param>
        /// <returns>Image</returns>
        public static Bitmap Resize(this Image sourceImage, bool maintainAspectRatio, int newWidth, int newHeight, Matrix matrix = null, InterpolationMode interpolationMode = InterpolationMode.HighQualityBicubic)
        {
            return sourceImage.Resize(maintainAspectRatio, false, Color.Empty, newWidth, newHeight, matrix, interpolationMode);
        }

        /// <summary>
        ///     Scale the bitmap, keeping aspect ratio, but the canvas will always have the specified size.
        /// </summary>
        /// <param name="sourceImage">Image to scale</param>
        /// <param name="maintainAspectRatio">true to maintain the aspect ratio</param>
        /// <param name="canvasUseNewSize">Makes the image maintain aspect ratio, but the canvas get's the specified size</param>
        /// <param name="backgroundColor">The color to fill with, or Color.Empty to take the default depending on the pixel format</param>
        /// <param name="newWidth">new width</param>
        /// <param name="newHeight">new height</param>
        /// <param name="matrix">Matrix</param>
        /// <param name="interpolationMode">InterpolationMode</param>
        /// <returns>a new bitmap with the specified size, the source-Image scaled to fit with aspect ratio locked</returns>
        public static Bitmap Resize(this Image sourceImage, bool maintainAspectRatio, bool canvasUseNewSize, Color backgroundColor, int newWidth, int newHeight, Matrix matrix, InterpolationMode interpolationMode = InterpolationMode.HighQualityBicubic)
        {
            var destX = 0;
            var destY = 0;

            var nPercentW = newWidth / (float)sourceImage.Width;
            var nPercentH = newHeight / (float)sourceImage.Height;
            if (maintainAspectRatio)
            {
                if ((int)nPercentW == 1 || (int)nPercentH != 0 && nPercentH < nPercentW)
                {
                    nPercentW = nPercentH;
                    if (canvasUseNewSize)
                    {
                        destX = Math.Max(0, Convert.ToInt32((newWidth - sourceImage.Width * nPercentW) / 2));
                    }
                }
                else
                {
                    nPercentH = nPercentW;
                    if (canvasUseNewSize)
                    {
                        destY = Math.Max(0, Convert.ToInt32((newHeight - sourceImage.Height * nPercentH) / 2));
                    }
                }
            }

            var destWidth = (int)(sourceImage.Width * nPercentW);
            var destHeight = (int)(sourceImage.Height * nPercentH);
            if (newWidth == 0)
            {
                newWidth = destWidth;
            }
            if (newHeight == 0)
            {
                newHeight = destHeight;
            }
            Bitmap newBitmap;
            if (maintainAspectRatio && canvasUseNewSize)
            {
                newBitmap = BitmapFactory.CreateEmpty(newWidth, newHeight, sourceImage.PixelFormat, backgroundColor, sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
                matrix?.Scale((float)newWidth / sourceImage.Width, (float)newHeight / sourceImage.Height, MatrixOrder.Append);
            }
            else
            {
                newBitmap = BitmapFactory.CreateEmpty(destWidth, destHeight, sourceImage.PixelFormat, backgroundColor, sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
                matrix?.Scale((float)destWidth / sourceImage.Width, (float)destHeight / sourceImage.Height, MatrixOrder.Append);
            }

            using (var graphics = Graphics.FromImage(newBitmap))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = interpolationMode;
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(sourceImage, new NativeRect(destX, destY, destWidth, destHeight), 0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return newBitmap;
        }

        /// <summary>
        ///     Count how many times the supplied color exists
        /// </summary>
        /// <param name="sourceImage">Image to count the pixels of</param>
        /// <param name="colorToCount">Color to count</param>
        /// <param name="includeAlpha">true if Alpha needs to be checked</param>
        /// <returns>int with the number of pixels which have colorToCount</returns>
        public static int CountColor(this Image sourceImage, Color colorToCount, bool includeAlpha = true)
        {
            var lockObject = new object();
            var colors = 0;
            var toCount = colorToCount.ToArgb();
            if (!includeAlpha)
            {
                toCount &= 0xffffff;
            }
            using (var bb = FastBitmapFactory.Create((Bitmap)sourceImage))
            {
                Parallel.For(0, bb.Height, () => 0, (y, state, initialColorCount) =>
                {
                    var currentColors = initialColorCount;
                    for (var x = 0; x < bb.Width; x++)
                    {
                        var bitmapcolor = bb.GetColorAt(x, y).ToArgb();
                        if (!includeAlpha)
                        {
                            bitmapcolor &= 0xffffff;
                        }
                        if (bitmapcolor == toCount)
                        {
                            currentColors++;
                        }
                    }
                    return currentColors;
                }, lineColorCount =>
                {
                    lock (lockObject)
                    {
                        colors += lineColorCount;
                    }
                });

                return colors;
            }
        }

        /// <summary>
        /// Check if the bitmaps are equal
        /// </summary>
        /// <param name="bitmap1">Bitmap</param>
        /// <param name="bitmap2">Bitmap</param>
        /// <returns>bool true if they are equal</returns>
        public static bool IsEqualTo(this Bitmap bitmap1, Bitmap bitmap2)
        {
            if (bitmap1.Width != bitmap2.Width || bitmap1.Height != bitmap2.Height)
            {
                Log.Debug().WriteLine("Different sizes 1={0}, 2={1}", bitmap1.Size, bitmap2.Size);
                // Different sizes
                return false;
            }

            if (bitmap1.PixelFormat != bitmap2.PixelFormat)
            {
                // Different pixel formats
                Log.Debug().WriteLine("Different pixel formats 1={0}, 2={1}", bitmap1.PixelFormat, bitmap2.PixelFormat);
                return false;
            }
            bool result = true;
            using (var fastBitmap1 = FastBitmapFactory.Create(bitmap1))
            using (var fastBitmap2 = FastBitmapFactory.Create(bitmap2))
            {
                Parallel.For(0, fastBitmap1.Height, (y, state) =>
                {
                    unsafe
                    {
                        var tmpColor1 = stackalloc byte[4];
                        var tmpColor2 = stackalloc byte[4];
                        for (int x = 0; x < fastBitmap1.Width; x++)
                        {
                            fastBitmap1.GetColorAt(x, y, tmpColor1);
                            fastBitmap2.GetColorAt(x, y, tmpColor2);
                            if (AreColorsSame(tmpColor1, tmpColor2, fastBitmap1.HasAlphaChannel))
                            {
                                continue;
                            }
                            Log.Debug().WriteLine("Different colors at {0},{1}", x, y);
                            result = false;
                            state.Break();
                        }
                    }
                });
            }
            return result;
        }

        /// <summary>
        ///     Checks if the colors are the same.
        /// </summary>
        /// <param name="aColor">Color first</param>
        /// <param name="bColor">Color second</param>
        /// <param name="hasAlpha">bool hasAlpha</param>
        /// <returns>True if they are; otherwise false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe bool AreColorsSame(byte* aColor, byte* bColor, bool hasAlpha = false)
        {
            return aColor[0] == bColor[0] && aColor[1] == bColor[1] && aColor[2] == bColor[2] && (hasAlpha ? aColor[3] == bColor[3] : true);
        }
    }
}
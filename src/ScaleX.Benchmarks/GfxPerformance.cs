using BenchmarkDotNet.Attributes;
using ScaleX.Legacy.Scaler;
using ScaleX.Scaler;
using ScaleX.Scaler.Structs;
using System.Drawing;
using System.Drawing.Imaging;

namespace ScaleX.Benchmarks
{
    /// <summary>
    /// This defines the benchmarks which can be done
    /// </summary>
    [MinColumn, MaxColumn, MemoryDiagnoser]
    public class GfxPerformance
    {
        private UnmanagedBitmap<Bgr32> _unmanagedTestBitmap;
        private Bitmap _testBitmap;

        [GlobalSetup]
        public void CreateTestImage()
        {
            _unmanagedTestBitmap = new UnmanagedBitmap<Bgr32>(400, 400);
            _unmanagedTestBitmap.Span.Fill(new Bgr32 { B = 255, G = 255, R = 255, Unused = 0});
            using (var bitmap = _unmanagedTestBitmap.AsBitmap())
            using (var graphics = Graphics.FromImage(bitmap))
            using (var pen = new SolidBrush(Color.Blue))
            {
                graphics.FillRectangle(pen, new Rectangle(30, 30, 340, 340));
            }
            _testBitmap = _unmanagedTestBitmap.AsBitmap();
        }

        [GlobalCleanup]
        public void Dispose()
        {
            _testBitmap.Dispose();
            _unmanagedTestBitmap.Dispose();
        }


        //[Benchmark]
        public void Blur_FastBitmap()
        {
            using (var bitmap = BitmapFactory.CreateEmpty(400, 400, PixelFormat.Format32bppRgb, Color.White))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                using (var pen = new SolidBrush(Color.Blue))
                {
                    graphics.FillRectangle(pen, new Rectangle(30, 30, 340, 340));
                }
                bitmap.ApplyBoxBlur(10);
            }
        }

        //[Benchmark]
        public void Blur_UnmanagedBitmap()
        {
            using (var unmanagedBitmap = new UnmanagedBitmap<Bgr32>(400, 400))
            {
                unmanagedBitmap.Span.Fill(new Bgr32 { B = 255, G = 255, R = 255 });
                using (var bitmap = unmanagedBitmap.AsBitmap())
                using (var graphics = Graphics.FromImage(bitmap))
                using (var pen = new SolidBrush(Color.Blue))
                {
                    graphics.FillRectangle(pen, new Rectangle(30, 30, 340, 340));
                }

                unmanagedBitmap.ApplyBoxBlur(10);
            }
        }


        [Benchmark]
        public void Scale2x_FastBitmap()
        {
            _testBitmap.Scale2X().Dispose();
        }

        [Benchmark]
        public void Scale2x_Unmanaged()
        {
            _unmanagedTestBitmap.Scale2X().Dispose();
        }

        [Benchmark]
        public void Scale2x_Unmanaged_Reference()
        {
            _unmanagedTestBitmap.Scale2XReference().Dispose();
        }

        [Benchmark]
        public void Scale3x_FastBitmap()
        {
            _testBitmap.Scale3X().Dispose();
        }

        [Benchmark]
        public void Scale3x_Unmanaged()
        {
            _unmanagedTestBitmap.Scale3X().Dispose();
        }
    }
}

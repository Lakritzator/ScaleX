This is a dotnet core implementation of the ScaleX algorithm
=============================================================

This repository is used to see what implementation of the ScaleX algorithm is the fastest, and uses the least memory.
This code will Scale a bitmap 2 or 3 times with the algorithm described here: https://www.scale2x.it/algorithm

This implementation is used by Greenshot, to scale the icons for HighDPI screens so they are not so small.
It should only solve a temporary issue, until Greenshot can use vector graphics...

The reason to place it in this repository, is to see & discus what tricks can be used to optimize the code.

Usage:
Change the solution to Release.
The ScaleX.Benchmarks has the benchmarks, set this as startup project.
Run with Ctrl+F5

The Scale2x_FastBitmap and Scale3x_FastBitmap tests are for the old and "current" Greenshot code, using an implementation to directly go to the bitmap raw data. **It uses Parallel.ForEach, and thus blocks the CPU.**

The new implementation uses Span<T> and is using a single thread, which is faster and uses less CPU power. They also don't directly use System.Drawing.Bitmap, as an experiment.

The tests Scale2x_Unmanaged_Reference & Scale3x_Unmanaged_Reference use my initial code , this is used as a reference.
The [Scale2x_Unmanaged](https://github.com/Lakritzator/ScaleX/blob/master/src/ScaleX.Scaler/ScaleXUnmanaged.cs#L51) & [Scale3x_Unmanaged](https://github.com/Lakritzator/ScaleX/blob/master/src/ScaleX.Scaler/ScaleXUnmanaged.cs#L139) is a copy of the reference implementation, used to make improvements and being able to compare them, a few changes are already in there, using refs, which does seem to improve a bit.

Besides being curious about what performance improvements are possible, one question does bother me:
When running on **.NET 472** I noticed a huge increase vs dotnet core 3.0 in the Scale3x_Unmanaged memory usage (from 56 B to 4096 B), I would love to understand why!

This is the current state for my Surface Pro 5 (2017) Core-5

``` ini

BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17763.379 (1809/October2018Update/Redstone5)
Intel Core i5-7300U CPU 2.60GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=3.0.100-preview4-010896
  [Host]     : .NET Core 3.0.0-preview4-27518-01 (CoreCLR 4.6.27518.71, CoreFX 4.7.19.16310), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview4-27518-01 (CoreCLR 4.6.27518.71, CoreFX 4.7.19.16310), 64bit RyuJIT


```
|                      Method |      Mean |     Error |    StdDev |       Min |       Max | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|---------------------------- |----------:|----------:|----------:|----------:|----------:|------------:|------------:|------------:|--------------------:|
|          Scale2x_FastBitmap |  7.753 ms | 0.0816 ms | 0.0681 ms |  7.680 ms |  7.941 ms |           - |           - |           - |              2144 B |
|           Scale2x_Unmanaged |  1.965 ms | 0.0223 ms | 0.0208 ms |  1.938 ms |  1.996 ms |           - |           - |           - |                56 B |
| Scale2x_Unmanaged_Reference |  1.971 ms | 0.0224 ms | 0.0187 ms |  1.929 ms |  1.999 ms |           - |           - |           - |                56 B |
|          Scale3x_FastBitmap | 15.157 ms | 0.1383 ms | 0.1294 ms | 14.936 ms | 15.361 ms |           - |           - |           - |              2149 B |
|           Scale3x_Unmanaged |  4.208 ms | 0.0461 ms | 0.0431 ms |  4.121 ms |  4.290 ms |    500.0000 |      7.8125 |           - |                56 B |

Running it on an old *Windows 7* PC:

``` ini

BenchmarkDotNet=v0.11.4, OS=Windows 7 SP1 (6.1.7601.0)
Intel Core i5-2400 CPU 3.10GHz (Sandy Bridge), 1 CPU, 4 logical and 4 physical cores
Frequency=3020634 Hz, Resolution=331.0563 ns, Timer=TSC
.NET Core SDK=3.0.100-preview4-010937
  [Host]     : .NET Core 3.0.0-preview4-27521-07 (CoreCLR 4.6.27521.73, CoreFX 4.7.19.16407), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview4-27521-07 (CoreCLR 4.6.27521.73, CoreFX 4.7.19.16407), 64bit RyuJIT
```


|                      Method |      Mean |     Error |    StdDev |       Min |       Max | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|---------------------------- |----------:|----------:|----------:|----------:|----------:|------------:|------------:|------------:|--------------------:|
|          Scale2x_FastBitmap |  7.013 ms | 0.1335 ms | 0.1249 ms |  6.843 ms |  7.305 ms |           - |           - |           - |              2140 B |
|           Scale2x_Unmanaged |  2.482 ms | 0.0027 ms | 0.0024 ms |  2.476 ms |  2.486 ms |           - |           - |           - |                40 B |
| Scale2x_Unmanaged_Reference |  2.707 ms | 0.0040 ms | 0.0037 ms |  2.702 ms |  2.715 ms |           - |           - |           - |                40 B |
|          Scale3x_FastBitmap | 13.906 ms | 0.0724 ms | 0.0677 ms | 13.815 ms | 14.050 ms |           - |           - |           - |              2115 B |
|           Scale3x_Unmanaged |  5.616 ms | 0.0061 ms | 0.0054 ms |  5.605 ms |  5.625 ms |    500.0000 |     39.0625 |           - |                40 B |
| Scale3x_Unmanaged_Reference |  5.890 ms | 0.0154 ms | 0.0144 ms |  5.857 ms |  5.911 ms |    500.0000 |     46.8750 |           - |                40 B |




``` ini
BenchmarkDotNet=v0.11.4, OS=Windows 7 SP1 (6.1.7601.0)
Intel Core i5-2400 CPU 3.10GHz (Sandy Bridge), 1 CPU, 4 logical and 4 physical cores
Frequency=3020634 Hz, Resolution=331.0563 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3324.0
  DefaultJob : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3324.0

```

|                      Method |      Mean |     Error |    StdDev |    Median |       Min |       Max | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|---------------------------- |----------:|----------:|----------:|----------:|----------:|----------:|------------:|------------:|------------:|--------------------:|
|          Scale2x_FastBitmap |  7.017 ms | 0.1592 ms | 0.3179 ms |  6.851 ms |  6.780 ms |  7.874 ms |           - |           - |           - |              1920 B |
|           Scale2x_Unmanaged |  3.375 ms | 0.0039 ms | 0.0036 ms |  3.375 ms |  3.370 ms |  3.382 ms |           - |           - |           - |                64 B |
| Scale2x_Unmanaged_Reference |  3.763 ms | 0.0050 ms | 0.0047 ms |  3.763 ms |  3.753 ms |  3.770 ms |           - |           - |           - |                64 B |
|          Scale3x_FastBitmap | 13.659 ms | 0.0269 ms | 0.0239 ms | 13.656 ms | 13.626 ms | 13.704 ms |           - |           - |           - |              2048 B |
|           Scale3x_Unmanaged |  7.196 ms | 0.0103 ms | 0.0092 ms |  7.194 ms |  7.183 ms |  7.212 ms |    500.0000 |      7.8125 |           - |              4096 B |
| Scale3x_Unmanaged_Reference |  7.499 ms | 0.0078 ms | 0.0073 ms |  7.497 ms |  7.486 ms |  7.509 ms |    500.0000 |      7.8125 |           - |              4096 B |

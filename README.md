This is a dotnet core implementation of the ScaleX algorithm
=============================================================

This repository is used to see what implementation of the ScaleX algorithm is the fastest, and uses the least memory.
This code will Scale a bitmap 2 or 3 times with the algorithm described here: https://www.scale2x.it/algorithm

This implementation is used by Greenshot, to scale the icons for HighDPI screens so they are not so small.
It should only solve a temporary issue, until Greenshot can use vector graphics.

The reason to place it in this repository, is to see & discus what tricks can be used to optimize the code.

Usage:
Change the solution to Release.
The ScaleX.Benchmarks has the benchmarks, set this as startup project.
Run with Ctrl+F5

The Scale2x_FastBitmap and Scale3x_FastBitmap tests are for the old and "current Greenshot code, using an implementation to directly go to the bitmap raw data.

The tests Scale2x_Unmanaged_Reference & Scale3x_Unmanaged_Reference use my initial code using Span<T>, this is used as a reference.
The Scale2x_Unmanaged & Scale3x_Unmanaged is used to make improvements, a few changes are already in there, using refs, which does seem to improve a bit.

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
|          Scale2x_FastBitmap |  7.062 ms | 0.0922 ms | 0.0863 ms |  6.927 ms |  7.189 ms |           - |           - |           - |              2135 B |
|           Scale2x_Unmanaged |  2.463 ms | 0.0038 ms | 0.0034 ms |  2.456 ms |  2.469 ms |           - |           - |           - |                56 B |
| Scale2x_Unmanaged_Reference |  2.744 ms | 0.0027 ms | 0.0024 ms |  2.740 ms |  2.749 ms |           - |           - |           - |                56 B |
|          Scale3x_FastBitmap | 13.829 ms | 0.0870 ms | 0.0771 ms | 13.736 ms | 13.988 ms |           - |           - |           - |              2107 B |
|           Scale3x_Unmanaged |  5.655 ms | 0.0058 ms | 0.0052 ms |  5.647 ms |  5.666 ms |    500.0000 |      7.8125 |           - |                56 B |


``` ini
BenchmarkDotNet=v0.11.4, OS=Windows 7 SP1 (6.1.7601.0)
Intel Core i5-2400 CPU 3.10GHz (Sandy Bridge), 1 CPU, 4 logical and 4 physical cores
Frequency=3020634 Hz, Resolution=331.0563 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3324.0
  DefaultJob : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3324.0

```
|                      Method |      Mean |     Error |    StdDev |       Min |       Max | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|---------------------------- |----------:|----------:|----------:|----------:|----------:|------------:|------------:|------------:|--------------------:|
|          Scale2x_FastBitmap |  6.903 ms | 0.0308 ms | 0.0273 ms |  6.869 ms |  6.971 ms |           - |           - |           - |              1984 B |
|           Scale2x_Unmanaged |  3.262 ms | 0.0060 ms | 0.0050 ms |  3.257 ms |  3.275 ms |           - |           - |           - |                64 B |
| Scale2x_Unmanaged_Reference |  3.668 ms | 0.0066 ms | 0.0059 ms |  3.659 ms |  3.679 ms |           - |           - |           - |                64 B |
|          Scale3x_FastBitmap | 13.869 ms | 0.0458 ms | 0.0429 ms | 13.798 ms | 13.964 ms |           - |           - |           - |              2048 B |
|           Scale3x_Unmanaged |  7.383 ms | 0.0119 ms | 0.0105 ms |  7.366 ms |  7.403 ms |    500.0000 |     39.0625 |           - |              4096 B |

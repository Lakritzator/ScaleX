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

The Scale2x_FastBitmap and Scale3x_FastBitmap is the old and "current Greenshot code.
The Scale2x_Unmanaged & Scale3x_Unmanaged is the new code, whereas Scale2x_Unmanaged_Reference is just a copy of Scale2x_Unmanaged for reference so we can see if there is a different after changing.
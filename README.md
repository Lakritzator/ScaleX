This is a dotnet core implementation of the ScaleX algorithm
=============================================================

This repository is used to see what implementation of the ScaleX algorithm is the fastest, and uses the least memory.
This code will Scale a bitmap 2 or 3 times with the algorithm described here: https://www.scale2x.it/algorithm

This implementation is used by Greenshot, to scale the icons for HighDPI screens so they are not so small.
It should only solve a temporary issue, until Greenshot can use vector graphics.

The reason to place it in this repository, is to see & discus what tricks can be used to optimize the code.

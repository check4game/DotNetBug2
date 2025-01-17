# Performance slows down on "OLD" processors

```
BenchmarkDotNet v0.14.0, Windows 10 (10.0.20348.2849)
Intel Core i7-10700K CPU 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  Job-OVPQJT : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=5  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=2  
```
| Method         | Load | Mean       | Error     | StdDev   | Ratio | RatioSD |
|--------------- |----- |-----------:|----------:|---------:|------:|--------:|
| AddOptimalFix3 | 0.51 |   485.2 ms |  13.99 ms |  3.63 ms |  0.34 |    0.00 |
| AddOptimalFix2 | 0.51 |   594.0 ms |  15.14 ms |  3.93 ms |  0.42 |    0.00 |
| AddOptimalFix1 | 0.51 |   603.1 ms |  12.96 ms |  3.37 ms |  0.43 |    0.00 |
| AddOptimalCPU  | 0.51 |   909.5 ms |   4.67 ms |  1.21 ms |  0.65 |    0.00 |
| AddOptimal     | 0.51 | 1,407.8 ms |  19.56 ms |  5.08 ms |  1.00 |    0.00 |
|                |      |            |           |          |       |         |
| AddOptimalFix3 | 0.61 |   614.0 ms |   2.90 ms |  0.75 ms |  0.37 |    0.00 |
| AddOptimalFix2 | 0.61 |   776.7 ms |   6.40 ms |  1.66 ms |  0.46 |    0.00 |
| AddOptimalFix1 | 0.61 |   791.5 ms |   6.98 ms |  1.81 ms |  0.47 |    0.00 |
| AddOptimalCPU  | 0.61 | 1,141.0 ms |   5.80 ms |  1.51 ms |  0.68 |    0.00 |
| AddOptimal     | 0.61 | 1,680.9 ms |  15.94 ms |  4.14 ms |  1.00 |    0.00 |
|                |      |            |           |          |       |         |
| AddOptimalFix3 | 0.71 |   778.8 ms |   9.58 ms |  2.49 ms |  0.40 |    0.00 |
| AddOptimalFix2 | 0.71 |   994.9 ms |   7.30 ms |  1.90 ms |  0.51 |    0.00 |
| AddOptimalFix1 | 0.71 |   996.5 ms |   7.04 ms |  1.83 ms |  0.51 |    0.00 |
| AddOptimalCPU  | 0.71 | 1,323.5 ms |   8.39 ms |  2.18 ms |  0.67 |    0.00 |
| AddOptimal     | 0.71 | 1,964.0 ms |  12.15 ms |  3.16 ms |  1.00 |    0.00 |
|                |      |            |           |          |       |         |
| AddOptimalFix3 | 0.81 |   961.1 ms |   7.62 ms |  1.98 ms |  0.43 |    0.00 |
| AddOptimalFix1 | 0.81 | 1,240.0 ms |  10.01 ms |  2.60 ms |  0.55 |    0.00 |
| AddOptimalFix2 | 0.81 | 1,251.1 ms |  53.74 ms | 13.96 ms |  0.56 |    0.01 |
| AddOptimalCPU  | 0.81 | 1,561.7 ms |  24.17 ms |  6.28 ms |  0.70 |    0.00 |
| AddOptimal     | 0.81 | 2,239.8 ms |   7.46 ms |  1.94 ms |  1.00 |    0.00 |
|                |      |            |           |          |       |         |
| AddOptimalFix3 | 0.91 | 1,183.8 ms |  20.14 ms |  5.23 ms |  0.45 |    0.01 |
| AddOptimalFix2 | 0.91 | 1,514.4 ms |  22.67 ms |  5.89 ms |  0.58 |    0.01 |
| AddOptimalFix1 | 0.91 | 1,557.6 ms |  93.55 ms | 24.29 ms |  0.60 |    0.01 |
| AddOptimalCPU  | 0.91 | 1,819.0 ms |  17.08 ms |  4.44 ms |  0.70 |    0.01 |
| AddOptimal     | 0.91 | 2,603.1 ms | 122.42 ms | 31.79 ms |  1.00 |    0.02 |

AddOptimal, it's problem! On modern CPUs AddOptimal is faster than  AddOptimalFix1 or  AddOptimalFix2
https://github.com/check4game/DotNetBug2/blob/d5062aadf60fbefe4351b4ae48acad3e5de88448/DotNetBug2.cs#L159-L172

AddOptimalFix1
https://github.com/check4game/DotNetBug2/blob/d5062aadf60fbefe4351b4ae48acad3e5de88448/DotNetBug2.cs#L217-L233

AddOptimalFix2
https://github.com/check4game/DotNetBug2/blob/d5062aadf60fbefe4351b4ae48acad3e5de88448/DotNetBug2.cs#L278-L291

# on modern processor 11th Gen Intel Core i5-11500

```
BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4894/22H2/2022Update)
11th Gen Intel Core i5-11500 2.70GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-QHEQIL : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

InvocationCount=1  IterationCount=5  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=2  
```
| Method         | Load | Mean       | Error     | StdDev   | Ratio | RatioSD |
|--------------- |----- |-----------:|----------:|---------:|------:|--------:|
| AddOptimalFix1 | 0.51 |   347.3 ms |  15.13 ms |  3.93 ms |  0.98 |    0.05 |
| AddOptimal     | 0.51 |   356.2 ms |  69.92 ms | 18.16 ms |  1.00 |    0.07 |
| AddOptimalFix2 | 0.51 |   356.5 ms |  53.73 ms | 13.95 ms |  1.00 |    0.06 |
| AddOptimalFix3 | 0.51 |   372.5 ms |   2.14 ms |  0.56 ms |  1.05 |    0.05 |
| AddOptimalCPU  | 0.51 |   869.3 ms | 139.07 ms | 36.12 ms |  2.45 |    0.15 |
|                |      |            |           |          |       |         |
| AddOptimal     | 0.61 |   424.3 ms |  89.80 ms | 23.32 ms |  1.00 |    0.07 |
| AddOptimalFix2 | 0.61 |   470.9 ms | 108.94 ms | 28.29 ms |  1.11 |    0.08 |
| AddOptimalFix1 | 0.61 |   491.4 ms |  46.48 ms | 12.07 ms |  1.16 |    0.06 |
| AddOptimalFix3 | 0.61 |   531.7 ms | 198.61 ms | 51.58 ms |  1.26 |    0.13 |
| AddOptimalCPU  | 0.61 | 1,089.7 ms | 165.77 ms | 43.05 ms |  2.57 |    0.16 |
|                |      |            |           |          |       |         |
| AddOptimal     | 0.71 |   479.7 ms |  71.12 ms | 18.47 ms |  1.00 |    0.05 |
| AddOptimalFix1 | 0.71 |   565.3 ms |  56.08 ms | 14.56 ms |  1.18 |    0.05 |
| AddOptimalFix2 | 0.71 |   566.0 ms | 211.72 ms | 54.98 ms |  1.18 |    0.11 |
| AddOptimalFix3 | 0.71 |   635.8 ms |  19.19 ms |  4.98 ms |  1.33 |    0.05 |
| AddOptimalCPU  | 0.71 | 1,285.0 ms |  24.36 ms |  6.33 ms |  2.68 |    0.09 |
|                |      |            |           |          |       |         |
| AddOptimal     | 0.81 |   571.1 ms |  66.42 ms | 17.25 ms |  1.00 |    0.04 |
| AddOptimalFix2 | 0.81 |   595.2 ms |  12.31 ms |  3.20 ms |  1.04 |    0.03 |
| AddOptimalFix1 | 0.81 |   604.0 ms |  25.76 ms |  6.69 ms |  1.06 |    0.03 |
| AddOptimalFix3 | 0.81 |   794.0 ms |  24.04 ms |  6.24 ms |  1.39 |    0.04 |
| AddOptimalCPU  | 0.81 | 1,503.3 ms |  11.11 ms |  2.89 ms |  2.63 |    0.07 |
|                |      |            |           |          |       |         |
| AddOptimal     | 0.91 |   668.5 ms |  22.80 ms |  5.92 ms |  1.00 |    0.01 |
| AddOptimalFix2 | 0.91 |   714.4 ms |  30.02 ms |  7.80 ms |  1.07 |    0.01 |
| AddOptimalFix1 | 0.91 |   715.3 ms |  13.30 ms |  3.46 ms |  1.07 |    0.01 |
| AddOptimalFix3 | 0.91 |   994.5 ms |  23.54 ms |  6.11 ms |  1.49 |    0.01 |
| AddOptimalCPU  | 0.91 | 1,771.8 ms | 139.63 ms | 36.26 ms |  2.65 |    0.05 |

# on modern processor 12th Gen Intel Core i5-12500H

```
BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i5-12500H, 1 CPU, 16 logical and 12 physical cores
.NET SDK 9.0.100-rc.2.24474.11
[Host] : .NET 9.0.0 (9.0.24.47305), X64 RyuJIT AVX2
Job-UIXUPV : .NET 9.0.0 (9.0.24.47305), X64 RyuJIT AVX2

InvocationCount=1 IterationCount=5 LaunchCount=1
RunStrategy=Monitoring UnrollFactor=1 WarmupCount=2
```

| Method         | Load | Mean     | Error    | StdDev   | Ratio | RatioSD |
|--------------- |----- |---------:|---------:|---------:|------:|--------:|
| AddOptimal     | 0.5  | 377.1 ms | 42.50 ms | 11.04 ms |  1.00 |    0.04 |
| AddOptimalFix1 | 0.5  | 361.1 ms | 17.20 ms |  4.47 ms |  0.96 |    0.03 |
| AddOptimalFix2 | 0.5  | 359.8 ms |  3.69 ms |  0.96 ms |  0.95 |    0.02 |

# some old processors


```
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
AMD Ryzen 5 5500U with Radeon Graphics, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  Job-LSQBKG : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=5  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=2  
```
| Method         | Load | Mean       | Error     | StdDev    | Ratio | 
|--------------- |----- |-----------:|----------:|----------:|------:|
| AddOptimalFix3 | 0.51 |   750.2 ms |  12.93 ms |   3.36 ms |  0.65 | 
| AddOptimalFix1 | 0.51 |   762.2 ms |   0.71 ms |   0.19 ms |  0.66 | 
| AddOptimalFix2 | 0.51 |   762.5 ms |   2.82 ms |   0.73 ms |  0.66 | 
| AddOptimal     | 0.51 | 1,146.9 ms |   1.92 ms |   0.50 ms |  1.00 | 
| AddOptimalCPU  | 0.51 | 1,276.4 ms |   6.03 ms |   1.57 ms |  1.11 | 
|                |      |            |           |           |       | 
| AddOptimalFix3 | 0.61 |   952.7 ms |   7.63 ms |   1.98 ms |  0.70 | 
| AddOptimalFix1 | 0.61 |   964.0 ms |   3.04 ms |   0.79 ms |  0.70 | 
| AddOptimalFix2 | 0.61 |   973.2 ms |  14.45 ms |   3.75 ms |  0.71 | 
| AddOptimal     | 0.61 | 1,370.6 ms |  11.11 ms |   2.89 ms |  1.00 | 
| AddOptimalCPU  | 0.61 | 1,558.1 ms |   7.20 ms |   1.87 ms |  1.14 | 
|                |      |            |           |           |       | 
| AddOptimalFix1 | 0.71 | 1,183.6 ms |   2.75 ms |   0.72 ms |  0.74 | 
| AddOptimalFix2 | 0.71 | 1,194.8 ms |  33.64 ms |   8.74 ms |  0.75 | 
| AddOptimalFix3 | 0.71 | 1,223.6 ms |  12.61 ms |   3.28 ms |  0.76 | 
| AddOptimal     | 0.71 | 1,600.9 ms |   2.67 ms |   0.69 ms |  1.00 | 
| AddOptimalCPU  | 0.71 | 1,956.7 ms | 482.99 ms | 125.43 ms |  1.22 | 
|                |      |            |           |           |       | 
| AddOptimalFix1 | 0.81 | 1,415.4 ms |   3.13 ms |   0.81 ms |  0.77 | 
| AddOptimalFix2 | 0.81 | 1,418.5 ms |   6.58 ms |   1.71 ms |  0.77 | 
| AddOptimalFix3 | 0.81 | 1,500.9 ms |  14.63 ms |   3.80 ms |  0.81 | 
| AddOptimal     | 0.81 | 1,844.2 ms |  51.33 ms |  13.33 ms |  1.00 | 
| AddOptimalCPU  | 0.81 | 2,224.7 ms |  53.81 ms |  13.97 ms |  1.21 | 
|                |      |            |           |           |       | 
| AddOptimalFix1 | 0.91 | 1,698.6 ms |  14.30 ms |   3.71 ms |  0.81 | 
| AddOptimalFix2 | 0.91 | 1,703.3 ms |  10.67 ms |   2.77 ms |  0.81 | 
| AddOptimalFix3 | 0.91 | 1,910.9 ms |  12.78 ms |   3.32 ms |  0.91 | 
| AddOptimal     | 0.91 | 2,102.8 ms |   7.95 ms |   2.07 ms |  1.00 | 
| AddOptimalCPU  | 0.91 | 2,600.5 ms |  11.69 ms |   3.04 ms |  1.24 | 

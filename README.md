# Performance slows down on "OLD" processors

```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.20348.2849)
Intel Core i7-10700K CPU 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  Job-RAMBOK : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=5  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=2  

```
| Method         | Load | Mean       | Error    | StdDev  | Ratio |
|--------------- |----- |-----------:|---------:|--------:|------:|
| AddOptimalFix3 | 0.5  |   483.6 ms | 17.27 ms | 4.48 ms |  0.35 |
| AddOptimalFix2 | 0.5  |   575.9 ms | 18.66 ms | 4.85 ms |  0.41 |
| AddOptimalFix1 | 0.5  |   583.3 ms | 19.47 ms | 5.06 ms |  0.42 |
| AddOptimalCPU  | 0.5  |   907.6 ms |  8.92 ms | 2.32 ms |  0.65 |
| AddOptimal     | 0.5  | 1,398.7 ms | 16.65 ms | 4.32 ms |  1.00 |

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
  Job-NGCJQD : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

InvocationCount=1  IterationCount=5  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=2  

```
| Method         | Load | Mean     | Error    | StdDev   | Ratio | RatioSD |
|--------------- |----- |---------:|---------:|---------:|------:|--------:|
| AddOptimal     | 0.5  | 334.7 ms | 26.79 ms |  6.96 ms |  1.00 |    0.03 |
| AddOptimalFix2 | 0.5  | 346.8 ms | 36.15 ms |  9.39 ms |  1.04 |    0.03 |
| AddOptimalFix1 | 0.5  | 354.1 ms | 31.57 ms |  8.20 ms |  1.06 |    0.03 |
| AddOptimalFix3 | 0.5  | 370.8 ms | 47.85 ms | 12.43 ms |  1.11 |    0.04 |
| AddOptimalCPU  | 0.5  | 852.9 ms | 63.71 ms | 16.55 ms |  2.55 |    0.07 |

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
  Job-MZSBIF : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=5  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=2  

```
| Method         | Load | Mean       | Error    | StdDev  | Ratio |
|--------------- |----- |-----------:|---------:|--------:|------:|
| AddOptimalFix3 | 0.5  |   729.7 ms | 11.21 ms | 2.91 ms |  0.65 |
| AddOptimalFix2 | 0.5  |   748.9 ms | 10.08 ms | 2.62 ms |  0.67 |
| AddOptimalFix1 | 0.5  |   749.6 ms | 11.64 ms | 3.02 ms |  0.67 |
| AddOptimal     | 0.5  | 1,117.0 ms |  8.20 ms | 2.13 ms |  1.00 |
| AddOptimalCPU  | 0.5  | 1,277.3 ms |  6.47 ms | 1.68 ms |  1.14 |

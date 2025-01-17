# Performance slows down on "OLD" processors

```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.20348.2849)
Intel Core i7-10700K CPU 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  Job-DWHPNO : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=10  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=5  

```
| Method         | Load | Mean       | Error   | StdDev  | Ratio |
|--------------- |----- |-----------:|--------:|--------:|------:|
| AddOptimalFix3 | 0.5  |   487.7 ms | 5.36 ms | 3.55 ms |  0.35 |
| AddOptimalFix1 | 0.5  |   583.5 ms | 4.32 ms | 2.86 ms |  0.42 |
| AddOptimalFix2 | 0.5  |   596.0 ms | 4.75 ms | 3.14 ms |  0.43 |
| AddOptimalCPU  | 0.5  |   917.3 ms | 7.06 ms | 4.67 ms |  0.66 |
| AddOptimal     | 0.5  | 1,383.8 ms | 9.15 ms | 6.05 ms |  1.00 |

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
  Job-XHQWJR : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

InvocationCount=1  IterationCount=10  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=5  

```
| Method         | Load | Mean     | Error    | StdDev  | Ratio | RatioSD |
|--------------- |----- |---------:|---------:|--------:|------:|--------:|
| AddOptimal     | 0.5  | 325.0 ms |  1.67 ms | 1.10 ms |  1.00 |    0.00 |
| AddOptimalFix2 | 0.5  | 334.9 ms |  0.89 ms | 0.59 ms |  1.03 |    0.00 |
| AddOptimalFix1 | 0.5  | 335.1 ms |  4.39 ms | 2.90 ms |  1.03 |    0.01 |
| AddOptimalFix3 | 0.5  | 358.9 ms |  1.37 ms | 0.91 ms |  1.10 |    0.00 |
| AddOptimalCPU  | 0.5  | 843.5 ms | 11.60 ms | 7.67 ms |  2.60 |    0.02 |

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
  Job-ROTBUB : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=10  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=5  

```
| Method         | Load | Mean       | Error   | StdDev  | Ratio |
|--------------- |----- |-----------:|--------:|--------:|------:|
| AddOptimalFix3 | 0.5  |   738.8 ms | 1.11 ms | 0.74 ms |  0.66 |
| AddOptimalFix1 | 0.5  |   740.0 ms | 2.04 ms | 1.35 ms |  0.66 |
| AddOptimalFix2 | 0.5  |   740.2 ms | 1.20 ms | 0.79 ms |  0.66 |
| AddOptimal     | 0.5  | 1,122.4 ms | 2.30 ms | 1.52 ms |  1.00 |
| AddOptimalCPU  | 0.5  | 1,265.1 ms | 1.94 ms | 1.29 ms |  1.13 |

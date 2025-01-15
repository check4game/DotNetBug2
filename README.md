# Performance slows down on "OLD" processors

```
BenchmarkDotNet v0.14.0, Windows 10 (10.0.20348.2849)
Intel Core i7-10700K CPU 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  Job-PNDHCV : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=5  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=2  

```
| Method         | Load | Mean       | Error    | StdDev  | Ratio |
|--------------- |----- |-----------:|---------:|--------:|------:|
| AddOptimal     | 0.5  | 1,413.3 ms | 33.91 ms | 8.81 ms |  1.00 |
| AddOptimalFix1 | 0.5  |   567.7 ms | 13.66 ms | 3.55 ms |  0.40 |
| AddOptimalFix2 | 0.5  |   570.7 ms |  9.24 ms | 2.40 ms |  0.40 |

AddOptimal 1413ms, it's problem! On modern CPUs AddOptimal is faster than  AddOptimalFix1 or  AddOptimalFix2
https://github.com/check4game/DotNetBug2/blob/d5062aadf60fbefe4351b4ae48acad3e5de88448/DotNetBug2.cs#L159-L172

AddOptimalFix1 567ms
https://github.com/check4game/DotNetBug2/blob/d5062aadf60fbefe4351b4ae48acad3e5de88448/DotNetBug2.cs#L217-L233

AddOptimalFix2 570ms
https://github.com/check4game/DotNetBug2/blob/d5062aadf60fbefe4351b4ae48acad3e5de88448/DotNetBug2.cs#L278-L291

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
BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4170/22H2/2022Update)
Intel Core i7-2700K CPU 3.50GHz (Sandy Bridge), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX
  Job-ECNBEE : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX

InvocationCount=1  IterationCount=5  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=2  
```
| Method         | Load | Mean       | Error    | StdDev  | Ratio |
|--------------- |----- |-----------:|---------:|--------:|------:|
| AddOptimal     | 0.5  | 2,018.7 ms |  8.89 ms | 2.31 ms |  1.00 |
| AddOptimalFix1 | 0.5  |   962.1 ms | 36.18 ms | 9.40 ms |  0.48 |
| AddOptimalFix2 | 0.5  |   963.6 ms | 12.87 ms | 3.34 ms |  0.48 |

```
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
AMD Ryzen 5 5500U with Radeon Graphics, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  Job-YUKPSM : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=5  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=2  
```

| Method         | Load | Mean       | Error    | StdDev  | Ratio |
|--------------- |----- |-----------:|---------:|--------:|------:|
| AddOptimal     | 0.5  | 1,071.2 ms |  6.36 ms | 1.65 ms |  1.00 |
| AddOptimalFix1 | 0.5  |   728.7 ms | 27.20 ms | 7.06 ms |  0.68 |
| AddOptimalFix2 | 0.5  |   718.5 ms | 16.18 ms | 4.20 ms |  0.67 |

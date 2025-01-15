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

```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.20348.2849)
Intel Core i7-10700K CPU 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.101
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  Job-UGBIFC : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=5  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=2  

```
| Method         | Load | Mean       | Error    | StdDev  | Ratio |
|--------------- |----- |-----------:|---------:|--------:|------:|
| AddOptimal     | 0.5  | 1,376.6 ms |  7.05 ms | 1.83 ms |  1.00 |
| AddOptimalFix1 | 0.5  |   567.9 ms | 12.99 ms | 3.37 ms |  0.41 |
| AddOptimalFix2 | 0.5  |   564.8 ms | 14.80 ms | 3.84 ms |  0.41 |


```
BenchmarkDotNet v0.14.0, Ubuntu 24.04.1 LTS (Noble Numbat)
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  Job-XBQCYZ : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=5  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=2
```

| Method         | Load | Mean     | Error    | StdDev   | Ratio | RatioSD |
|--------------- |----- |---------:|---------:|---------:|------:|--------:|
| AddOptimal     | 0.5  | 570.5 ms | 53.28 ms | 13.84 ms |  1.00 |    0.03 |
| AddOptimalFix1 | 0.5  | 387.5 ms | 32.90 ms |  8.54 ms |  0.68 |    0.02 |
| AddOptimalFix2 | 0.5  | 411.9 ms | 24.78 ms |  6.44 ms |  0.72 |    0.02 |

```
BenchmarkDotNet v0.14.0, Windows 10 (10.0.20348.2966) (Hyper-V)
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  Job-VJTGBD : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
InvocationCount=1  IterationCount=5  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=2
```

| Method         | Load | Mean     | Error    | StdDev   | Ratio | RatioSD |
|--------------- |----- |---------:|---------:|---------:|------:|--------:|
| AddOptimal     | 0.5  | 753.3 ms | 43.02 ms | 11.17 ms |  1.00 |    0.02 |
| AddOptimalFix1 | 0.5  | 466.4 ms | 33.00 ms |  8.57 ms |  0.62 |    0.01 |
| AddOptimalFix2 | 0.5  | 444.7 ms | 11.33 ms |  2.94 ms |  0.59 |    0.01 |

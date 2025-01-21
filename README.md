# Performance slows down on "OLD" processors

```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.20348.2849)
Intel Core i7-10700K CPU 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  Job-LYGMKV : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=10  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=5  

```
| Method        | Load | Mean       | Error   | StdDev  | Ratio | RatioSD |
|-------------- |----- |-----------:|--------:|--------:|------:|--------:|
| AddFindNoSimd | 0.5  |   462.8 ms | 5.69 ms | 3.77 ms |  1.00 |    0.01 |
| AddNoSimd     | 0.5  |   871.5 ms | 1.19 ms | 0.79 ms |  1.88 |    0.01 |
| AddFindSimd   | 0.5  | 1,375.8 ms | 6.08 ms | 4.02 ms |  2.97 |    0.02 |

```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4894/22H2/2022Update)
11th Gen Intel Core i5-11500 2.70GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-CRDHOJ : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

InvocationCount=1  IterationCount=10  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=5  

```
| Method        | Load | Mean     | Error    | StdDev   | Ratio | RatioSD |
|-------------- |----- |---------:|---------:|---------:|------:|--------:|
| AddFindSimd   | 0.5  | 362.4 ms | 47.03 ms | 31.11 ms |  0.97 |    0.09 |
| AddFindNoSimd | 0.5  | 372.8 ms | 26.00 ms | 17.20 ms |  1.00 |    0.06 |
| AddNoSimd     | 0.5  | 887.3 ms | 24.56 ms | 16.24 ms |  2.38 |    0.10 |

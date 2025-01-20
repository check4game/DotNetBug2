```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.20348.2849)
Intel Core i7-10700K CPU 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  Job-CBQARX : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=10  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=5  

```
| Method              | Load | Mean       | Error   | StdDev  | Ratio | RatioSD |
|-------------------- |----- |-----------:|--------:|--------:|------:|--------:|
| AddVectorCPUIndexOf | 0.5  |   445.1 ms | 4.54 ms | 3.00 ms |  1.00 |    0.01 |
| AddVectorCPUFor     | 0.5  |   469.5 ms | 5.19 ms | 3.43 ms |  1.05 |    0.01 |
| AddVectorWhile      | 0.5  |   478.3 ms | 4.41 ms | 2.92 ms |  1.07 |    0.01 |
| AddVectorIF         | 0.5  |   562.4 ms | 4.35 ms | 2.88 ms |  1.26 |    0.01 |
| AddCPU              | 0.5  |   857.9 ms | 3.70 ms | 2.45 ms |  1.93 |    0.01 |
| AddVector           | 0.5  | 1,386.3 ms | 7.12 ms | 4.71 ms |  3.11 |    0.02 |

```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4894/22H2/2022Update)
11th Gen Intel Core i5-11500 2.70GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-CAMLFO : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

InvocationCount=1  IterationCount=10  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=5  

```
| Method              | Load | Mean     | Error   | StdDev  | Ratio |
|-------------------- |----- |---------:|--------:|--------:|------:|
| AddVector           | 0.5  | 326.8 ms | 1.04 ms | 0.69 ms |  0.95 |
| AddVectorIF         | 0.5  | 338.0 ms | 1.10 ms | 0.73 ms |  0.98 |
| AddVectorCPUIndexOf | 0.5  | 344.9 ms | 2.12 ms | 1.41 ms |  1.00 |
| AddVectorCPUFor     | 0.5  | 362.4 ms | 1.90 ms | 1.26 ms |  1.05 |
| AddVectorWhile      | 0.5  | 376.5 ms | 2.46 ms | 1.63 ms |  1.09 |
| AddCPU              | 0.5  | 780.7 ms | 3.07 ms | 2.03 ms |  2.26 |
```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
AMD Ryzen 5 5500U with Radeon Graphics, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  Job-HEBRPA : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=10  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=5  

```
| Method              | Load | Mean       | Error   | StdDev  | Ratio |
|-------------------- |----- |-----------:|--------:|--------:|------:|
| AddVectorIF         | 0.5  |   739.2 ms | 2.36 ms | 1.56 ms |  1.00 |
| AddVectorWhile      | 0.5  |   740.7 ms | 5.28 ms | 3.49 ms |  1.00 |
| AddVectorCPUIndexOf | 0.5  |   742.1 ms | 5.89 ms | 3.89 ms |  1.00 |
| AddVectorCPUFor     | 0.5  |   743.1 ms | 1.08 ms | 0.72 ms |  1.00 |
| AddVector           | 0.5  | 1,119.0 ms | 1.78 ms | 1.18 ms |  1.51 |
| AddCPU              | 0.5  | 1,217.7 ms | 1.47 ms | 0.97 ms |  1.64 |
```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2605)
AMD Ryzen 7 9700X, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.101
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-SLFBIJ : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

InvocationCount=1  IterationCount=10  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=5  

```
| Method              | Load | Mean     | Error    | StdDev   | Ratio | RatioSD |
|-------------------- |----- |---------:|---------:|---------:|------:|--------:|
| AddVectorIF         | 0.5  | 249.3 ms |  1.77 ms |  1.17 ms |  0.92 |    0.03 |
| AddVectorWhile      | 0.5  | 262.4 ms |  3.65 ms |  2.42 ms |  0.97 |    0.04 |
| AddVector           | 0.5  | 264.8 ms |  5.14 ms |  3.40 ms |  0.98 |    0.04 |
| AddVectorCPUIndexOf | 0.5  | 271.5 ms | 16.03 ms | 10.60 ms |  1.00 |    0.05 |
| AddVectorCPUFor     | 0.5  | 283.0 ms |  3.11 ms |  2.06 ms |  1.04 |    0.04 |
| AddCPU              | 0.5  | 648.0 ms | 21.11 ms | 13.96 ms |  2.39 |    0.10 |

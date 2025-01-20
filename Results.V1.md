```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.20348.2849)
Intel Core i7-10700K CPU 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  Job-GFMRBW : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=10  LaunchCount=1  
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=5  

```
| Method              | Load | Mean       | Error     | StdDev    | Ratio | RatioSD |
|-------------------- |----- |-----------:|----------:|----------:|------:|--------:|
| AddVectorCPUIndexOf | 0.5  |   441.4 ms |   8.01 ms |   5.30 ms |  1.00 |    0.02 |
| AddVectorCPUFor     | 0.5  |   465.3 ms |   0.63 ms |   0.42 ms |  1.05 |    0.01 |
| AddVectorWhile      | 0.5  |   466.2 ms |   2.17 ms |   1.44 ms |  1.06 |    0.01 |
| AddVectorIF         | 0.5  |   557.3 ms |   1.39 ms |   0.92 ms |  1.26 |    0.01 |
| AddCPU              | 0.5  |   902.2 ms | 219.41 ms | 145.12 ms |  2.04 |    0.31 |
| AddVector           | 0.5  | 1,372.0 ms |   7.31 ms |   4.83 ms |  3.11 |    0.04 |

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
11th Gen Intel Core i5-11300H 3.10GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.400-preview.22330.6
  [Host]   : .NET 6.0.7 (6.0.722.32202), X64 RyuJIT
  .NET 6.0 : .NET 6.0.7 (6.0.722.32202), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|             Method | DataCount |        Mean |     Error |    StdDev | Ratio |     Gen 0 | Allocated |
|------------------- |---------- |------------:|----------:|----------:|------:|----------:|----------:|
| TestSecurityFilter |    100000 |    682.5 μs |   3.51 μs |   3.28 μs |  1.00 | 1147.4609 |      5 MB |
|                    |           |             |           |           |       |           |           |
|     TestTimeFilter |    100000 |  8,004.1 μs |  51.66 μs |  45.79 μs |  1.00 | 6015.6250 |     24 MB |
|                    |           |             |           |           |       |           |           |
|    TestPriceFilter |    100000 | 10,105.8 μs |  83.15 μs |  73.71 μs |  1.00 | 6187.5000 |     25 MB |
|                    |           |             |           |           |       |           |           |
|          Calculate |    100000 | 10,990.0 μs |  69.98 μs |  62.04 μs |  1.00 | 6921.8750 |     28 MB |
|                    |           |             |           |           |       |           |           |
|        TestExecute |    100000 | 11,498.0 μs | 107.17 μs | 100.25 μs |  1.00 | 7312.5000 |     29 MB |

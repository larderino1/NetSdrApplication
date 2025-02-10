# NetSdrApplication

- **TCP command control**: Send and receive control messages using a `TcpConnectionController`.  
- **UDP I/Q data streaming**: Receive raw I/Q samples via UDP and store them in a local file (`udp_data.bin`).  
- **Dependency Injection**: Manage service lifetimes with `Microsoft.Extensions.DependencyInjection`.  
- **Benchmarking**: Leverage BenchmarkDotNet for serialization performance testing.

## Table of Contents

- [Features](#features)
- [Benchmarks](#benchmarks)

---

## Features

1. **TCP Command Interface**  
   - Connect/Disconnect from a remote SDR (Software-Defined Radio).
   - Send commands to start/stop I/Q streaming or set frequency.

2. **UDP I/Q Data Reception**  
   - Listen on a UDP port for raw I/Q data.
   - Write received data packets into a binary file (`udp_data.bin`).

3. **Dependency Injection**  
   - Clear separation of concerns with `IConnectionController` and `INetSdrClient` abstractions.
   - `ServiceCollection` for instantiating and injecting dependencies.

4. **Robust Error Handling**  
   - Throw meaningful exceptions (`ConnectionException`, `NAKException`, etc.).
   - Gracefully handle cancellation and cleanup for UDP streaming.

5. **Benchmarking with BenchmarkDotNet**  
   - Serialization performance benchmarks for control messages.

---

## Benchmarks


| Method                                   | Mean      | Error     | StdDev    |
|----------------------------------------- |---------: |---------: |---------: |
| **BenchmarkControlItemMessageSerialization** | 43.879 ns | 0.8125 ns | 0.7600 ns |
| **BenchmarkAckMessageSerialization**         | 22.127 ns | 0.3814 ns | 0.3567 ns |
| **BenchmarkHeaderSerialization**             |  7.572 ns | 0.2041 ns | 0.2992 ns |

- **ControlItemMessage Serialization**: ~44 ns  
- **AckMessage Serialization**: ~22 ns  
- **Header Serialization**: ~8 ns  


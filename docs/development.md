# Development

Requirements: Windows 10 1809+ / Windows 11, Visual Studio 2022 17.8+ with ".NET desktop development" and "Windows application development" workloads (or .NET 8 SDK + Windows App SDK), .NET 8 SDK.

```
dotnet restore LiveLock.sln -p:Platform=x64
dotnet build LiveLock.sln -c Debug -p:Platform=x64
dotnet test LiveLock.sln -c Debug -p:Platform=x64
dotnet run --project src/LiveLock.App/LiveLock.App.csproj -p:Platform=x64
```
Settings: `%LOCALAPPDATA%\LiveLock\settings.json`. Log: `%LOCALAPPDATA%\LiveLock\logs\livelock.log`.

dotnet publish -r win-x64 -c Release -f net8.0 --self-contained false
dotnet publish -r linux-x64 -c Release -f net8.0 --self-contained false
dotnet publish -r linux-arm64 -c Release -f net8.0 --self-contained false
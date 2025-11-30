# https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
dotnet publish -c Release -r win-x86 --self-contained -p:PublishSingleFile=true
dotnet publish -c Release -r win-arm64 --self-contained -p:PublishSingleFile=true

# https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
dotnet publish -c Release -r linux-musl-x64 --self-contained -p:PublishSingleFile=true
dotnet publish -c Release -r linux-musl-arm64 --self-contained -p:PublishSingleFile=true
dotnet publish -c Release -r linux-arm --self-contained -p:PublishSingleFile=true
dotnet publish -c Release -r linux-arm64 --self-contained -p:PublishSingleFile=true

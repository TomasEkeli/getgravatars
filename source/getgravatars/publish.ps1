
dotnet publish -c Release -r win10-x64 /p:PublishSingleFile=true /p:PublishTrimmed=true
dotnet publish -c Release -r linux-x64 /p:PublishSingleFile=true /p:PublishTrimmed=true
dotnet publish -c Release -r osx.10.13-x64 /p:PublishSingleFile=true /p:PublishTrimmed=true
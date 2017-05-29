cd ../server/
dotnet restore ./library/PDFSharpCore/PDFSharpCore.csproj --no-cache
dotnet restore ./library/taskbuilder-netcore-interfaces/taskbuilder-netcore-interfaces.csproj --no-cache
dotnet restore ./library/taskbuilder-netcore-data/taskbuilder-netcore-data.csproj --no-cache
dotnet restore ./library/taskbuilder-netcore-data-functionality/taskbuilder-netcore-data-functionality.csproj --no-cache
dotnet restore ./library/taskbuilder-netcore-common/taskbuilder-netcore-common.csproj --no-cache
dotnet restore ./account-manager/account-manager.csproj --no-cache
dotnet restore ./provisioning-server/provisioning-server.csproj --no-cache
dotnet restore ./menu-gate/menu-gate.csproj --no-cache
dotnet restore ./data-service/data-service.csproj --no-cache
dotnet restore ./database-service/database-service.csproj --no-cache
dotnet restore ./rs-web/rs-web.csproj --no-cache
dotnet restore ./tbloader-gate/tbloader-gate.csproj --no-cache
dotnet restore ./web-server/web-server.csproj --no-cache
dotnet restore ./widgets-service/widgets-service.csproj --no-cache
dotnet restore ./manufacturing-service/manufacturing-service.csproj --no-cache
dotnet restore ./licensing-manager/licensing-manager.csproj --no-cache

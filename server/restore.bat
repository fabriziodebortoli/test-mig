dotnet restore ./rs-web/project.json --no-cache
dotnet restore ./tbloader-gate/project.json --no-cache
dotnet restore ./login-manager/project.json --no-cache
dotnet restore ./PDFSharpCore/project.json --no-cache
dotnet restore ./TaskBuilderNet.Common/project.json --no-cache
dotnet restore ./TaskBuilderNetCore.Data/project.json --no-cache
dotnet restore ./TaskBuilderNetCore.Interface/project.json --no-cache
dotnet restore ./menu-gate/project.json --no-cache
dotnet restore ./DataService/project.json --no-cache
dotnet restore ./web-server/project.json --no-cache

dotnet build ./web-server/project.json
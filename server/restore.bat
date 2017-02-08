dotnet restore ./rs-web/project.json --no-cache
dotnet restore ./tbloader-gate/project.json --no-cache
dotnet restore ./login-manager/project.json --no-cache
dotnet restore ./library/PDFSharpCore/project.json --no-cache
dotnet restore ./library/TaskBuilderNet.Common/project.json --no-cache
dotnet restore ./library/task-builder-net-core-data/src/TaskBuilderNetCore.Data/project.json --no-cache
dotnet restore ./library/task-builder-net-core-interfaces/src/TaskBuilderNetCore.Interfaces/project.json --no-cache
dotnet restore ./menu-gate/project.json --no-cache
dotnet restore ./DataService/project.json --no-cache
dotnet restore ./web-server/project.json --no-cache

dotnet build ./web-server/project.json
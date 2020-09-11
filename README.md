# Jekyll blog converted to WebAssembly, written in F# & hosted on github pages.

![DeployToGitHubPages](https://github.com/wilsoncg/wilsoncg.github.io/workflows/DeployToGitHubPages/badge.svg)

Repository for [wilsoncg.net](https://www.wilsoncg.net).

Uses Bolero - F# Tools for Blazor, see [website](https://fsbolero.io/) and [repository](https://github.com/fsbolero/Bolero).

## To build & Run
```powershell
dotnet build -c Debug; dotnet run -p .\src\BoleroGitHub.Server\ -c Debug
```
`BoleroGitHub.Server` project uses dotnet Kestrel web server to assist development (template hot reloading). The static site served is contained in `BoleroGitHub.Client` project.

To run as staging environment:
```powershell
dotnet publish -c Release;dotnet run -p .\src\BoleroGitHub.Server\ -c Release --launch-profile "KestrelStaging"
```
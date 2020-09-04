namespace BoleroGitHub.Client

open System.Net.Http
open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Bolero.Remoting.Client

module Program =

    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
        printfn "HostEnvironment %s BaseAddress %s" builder.HostEnvironment.Environment builder.HostEnvironment.BaseAddress
        let http = new HttpClient(BaseAddress = System.Uri(builder.HostEnvironment.BaseAddress))
        builder.Services.AddSingleton<HttpClient>(fun _ -> http) |> ignore

        builder.RootComponents.Add<Main.MyApp>("#main")
#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Debug) |> ignore
        builder.Services.AddRemoting(builder.HostEnvironment) |> ignore
#endif        
        builder.Build().RunAsync() |> ignore
        0

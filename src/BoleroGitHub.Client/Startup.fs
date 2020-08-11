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
        builder.RootComponents.Add<Main.MyApp>("#main")
#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Debug) |> ignore
        builder.Services.AddRemoting(builder.HostEnvironment) |> ignore
#endif
        let baseAddress = builder.HostEnvironment.BaseAddress
        printfn "HostEnvironment.BaseAddress %s" baseAddress
        let http = new HttpClient(BaseAddress = System.Uri(baseAddress))
        builder.Services.AddTransient<HttpClient>(fun _ -> http) |> ignore
        builder.Build().RunAsync() |> ignore
        0

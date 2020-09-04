namespace BoleroGitHub.Client

open System
open System.Net.Http
open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Bolero.Remoting.Client

module Program =

    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
        let env = builder.HostEnvironment
        printfn "HostEnvironment %s BaseAddress %s" env.Environment env.BaseAddress
        let http = new HttpClient(BaseAddress = Uri(env.BaseAddress))
        builder.Services.AddSingleton<HttpClient>(http) |> ignore

        builder.RootComponents.Add<Main.MyApp>("#main")
#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Debug) |> ignore
        builder.Services.AddRemoting(env) |> ignore
#endif        
        builder.Build().RunAsync() |> ignore
        0

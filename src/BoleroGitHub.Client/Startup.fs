namespace BoleroGitHub.Client

open Microsoft.AspNetCore.Components.WebAssembly.Hosting
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
        builder.Build().RunAsync() |> ignore
        0

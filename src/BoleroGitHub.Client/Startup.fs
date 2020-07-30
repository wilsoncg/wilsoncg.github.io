namespace BoleroGitHub.Client

open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Bolero.Remoting.Client

module Program =

    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
        builder.RootComponents.Add<Main.MyApp>("#main")
#if DEBUG
        builder.Services.AddRemoting(builder.HostEnvironment) |> ignore
#endif
        builder.Build().RunAsync() |> ignore
        0

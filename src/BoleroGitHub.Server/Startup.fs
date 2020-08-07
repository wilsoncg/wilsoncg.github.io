namespace BoleroGitHub.Server

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.DependencyInjection
open Bolero
open Bolero.Remoting.Server
open Bolero.Server.RazorHost
open Bolero.Templating.Server
open System.IO
open Microsoft.Extensions.FileProviders

// Simplified startup with html loader
// Refer to bolero tryfsharp sample
// https://github.com/fsbolero/TryFSharpOnWasm/blob/master/src/WebFsc.Server/Startup.fs

type Startup() =
    let clientProjPath = Path.Combine(__SOURCE_DIRECTORY__, "..", "BoleroGitHub.Client")

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
        services.AddControllers() |> ignore         
        services.AddHotReload(
            templateDir = clientProjPath, 
            delay = System.TimeSpan.FromMilliseconds 1000.)
        |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        app
            .UseStaticFiles(
                StaticFileOptions(
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(clientProjPath, "wwwroot")
                    )
                )
            )            
            .UseRouting()       
            .UseBlazorFrameworkFiles()
            .UseEndpoints(fun endpoints ->
                endpoints.UseHotReload() |> ignore
                endpoints.MapControllers() |> ignore
                endpoints.MapFallbackToFile("index.html") |> ignore)
        |> ignore

module Program =

    [<EntryPoint>]
    let main args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStaticWebAssets()
            .UseStartup<Startup>()
            .Build()
            .Run()
        0

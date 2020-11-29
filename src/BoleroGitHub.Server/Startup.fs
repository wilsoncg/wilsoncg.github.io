namespace BoleroGitHub.Server

open System
open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.StaticFiles
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.FileProviders
open Bolero.Templating.Server
open FSharp.Control.Tasks.V2
open Microsoft.Extensions.FileProviders.Physical

// Simplified startup with html loader
// Refer to bolero tryfsharp sample
// https://github.com/fsbolero/TryFSharpOnWasm/blob/master/src/WebFsc.Server/Startup.fs

type Startup() =    
    let clientProjPath = Path.Combine(__SOURCE_DIRECTORY__, "..", "BoleroGitHub.Client")
    let serverProjPath = Path.Combine(__SOURCE_DIRECTORY__, "..", "BoleroGitHub.Server")
    
    // align with GitHub pages which serves .md as text/markdown
    let contentTypeProvider = FileExtensionContentTypeProvider()
    do  contentTypeProvider.Mappings.[".md"] <- "text/markdown"

    member this.ConfigureDevelopmentServices(services: IServiceCollection) =
        services.AddControllers() |> ignore
        services.AddHotReload(
            templateDir = clientProjPath, 
            delay = System.TimeSpan.FromMilliseconds 1000.)
        |> ignore

    member this.ConfigureStagingServices(services: IServiceCollection) =
        services.AddControllers() |> ignore            

    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        app
         .UseDefaultFiles() |> ignore

        let folderPath =
            match env.EnvironmentName with
            | "Staging" -> Path.Combine [| serverProjPath; "bin"; "release"; "net5.0"; "publish"; "wwwroot" |]
            | _ -> Path.Combine(clientProjPath, "wwwroot")

        app
         .UseStaticFiles(
                StaticFileOptions(
                    FileProvider = new PhysicalFileProvider(folderPath),
                    ContentTypeProvider = contentTypeProvider
                ))
         .UseRouting()
         .UseBlazorFrameworkFiles()         
         .UseStatusCodePagesWithReExecute("/error/{0}")
         .Use(fun ctx (next:Func<Task>) ->
            task { 
                do! next.Invoke()
                let path = ctx.Request.Path.Value
                if path.StartsWith("/error/404") then
                    let filePath = Path.Combine [| clientProjPath; "wwwroot"; "404.html" |]
                    let fi = FileInfo(filePath)
                    if not ctx.Response.HasStarted then
                        ctx.Response.ContentType <- "text/html"
                        do! ctx.Response.SendFileAsync(new PhysicalFileInfo(fi))                   
                        do! ctx.Response.CompleteAsync()
                } :> Task
         )
         .UseEndpoints(fun endpoints ->
            if env.EnvironmentName = "Development" then
                endpoints.UseHotReload() |> ignore
            endpoints.MapControllers() |> ignore
         )         
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

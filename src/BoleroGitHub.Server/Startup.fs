namespace BoleroGitHub.Server

open System
open System.IO
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.StaticFiles
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.FileProviders
open Microsoft.Extensions.Logging
open Bolero
open Bolero.Remoting.Server
open Bolero.Server.RazorHost
open Bolero.Templating.Server
open FSharp.Control.Tasks.V2

type GitHubPagesMiddleware (next : RequestDelegate,
                            loggerFactory : ILoggerFactory ) =
    member this.Invoke (context: HttpContext) =
        do if isNull next then raise (ArgumentNullException("next"))
        
        let logger = loggerFactory.CreateLogger<GitHubPagesMiddleware>()        

        task {
            do! next.Invoke(context)
            if context.Request.Path = PathString "/" then
                //let stream = new StreamReader()
                context.Request.Path <- PathString "/index.html"
                context.SetEndpoint(null)
                return! next.Invoke(context)
            else
                logger.LogWarning("{Code} {Path}", 
                    context.Response.StatusCode, 
                    context.Request.Path.ToString())            
                //context.Request.Path <- PathString "/404.html"
            //else
                // context.Request.Path <- PathString "/index.html"
                // context.SetEndpoint null
                return! next.Invoke(context)
        }

// Simplified startup with html loader
// Refer to bolero tryfsharp sample
// https://github.com/fsbolero/TryFSharpOnWasm/blob/master/src/WebFsc.Server/Startup.fs

type Startup() =
    let clientProjPath = Path.Combine(__SOURCE_DIRECTORY__, "..", "BoleroGitHub.Client")
    let serverProjPath = Path.Combine(__SOURCE_DIRECTORY__, "..", "BoleroGitHub.Server")
    // align with GitHub pages which serves .md as text/markdown
    let contentTypeProvider = FileExtensionContentTypeProvider()
    do  contentTypeProvider.Mappings.[".md"] <- "text/markdown"

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
        services.AddControllers() |> ignore
//#if DEBUG       
        services.AddHotReload(
            templateDir = clientProjPath, 
            delay = System.TimeSpan.FromMilliseconds 1000.)
        |> ignore
//#endif

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        //app
         //.UseStatusCodePagesWithReExecute("/404.html")
         //.UseMiddleware<GitHubPagesMiddleware>()
         //|> ignore

        match env.EnvironmentName with
        | "Staging" -> 
            app.UseStaticFiles(
                StaticFileOptions(
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine [| serverProjPath; "bin"; "release"; "netcoreapp3.1"; "publish"; "wwwroot" |]
                    ),
                    ContentTypeProvider = contentTypeProvider
                )
            ) |> ignore
        | _ ->
            app.UseStaticFiles(
                    StaticFileOptions(
                        FileProvider = new PhysicalFileProvider(
                            Path.Combine(clientProjPath, "wwwroot")
                        ),
                        ContentTypeProvider = contentTypeProvider
                    )
            ) |> ignore
        
        app
         .UseRouting()
         .UseBlazorFrameworkFiles()         
         //.UseStatusCodePagesWithReExecute("/404.html")
         .UseEndpoints(fun endpoints ->
            endpoints.UseHotReload() |> ignore
            endpoints.MapControllers() |> ignore
            //endpoints.MapFallbackToFile("index.html") |> ignore
         )         
         .UseMiddleware<GitHubPagesMiddleware>()
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

module Tests

open System
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.StaticFiles
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.FileProviders
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Xunit
open FSharp.Control.Tasks.V2
open BoleroGitHub.Server
open System.Net

[<Fact>]
let ``Test middleware`` () =
    async {
        let! h =
            HostBuilder()
                .ConfigureWebHost(fun webBuilder ->
                    webBuilder
                     .UseTestServer()
                     .ConfigureTestServices(fun services ->
                        services.AddControllers() |> ignore)
                     .Configure(fun app ->
                        app                         
                         .UseRouting()
                         .UseBlazorFrameworkFiles()
                         .UseMiddleware<GitHubPagesMiddleware>() |> ignore)
                    |> ignore)
                .StartAsync() |> Async.AwaitTask
        let! response = h.GetTestClient().GetAsync("/") |> Async.AwaitTask

        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
    }

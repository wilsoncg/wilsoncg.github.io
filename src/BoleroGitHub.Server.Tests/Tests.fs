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
open System.IO

[<Fact>]
let ``Middleware test fetch index.html should be 200`` () =
    async {
        let projectRootPath = 
         Path.Combine(
            [| Directory.GetCurrentDirectory(); ".."; ".."; ".."; ".."; "BoleroGitHub.Client"; "wwwroot" |]
         )
        let h =
            WebHostBuilder()
                .UseStaticWebAssets()
                .UseEnvironment("Development")
                .UseStartup<Startup>()
        use testServer = new TestServer(h)
        use client = testServer.CreateClient()
        let! response = client.GetAsync("/") |> Async.AwaitTask
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        Assert.True(content.Contains("meta name=\"keywords\" content=\"index\""))
        Assert.True(content.Contains("_framework/blazor.webassembly.js"), content.Substring(0,50) |> sprintf "found %s")
    }

[<Fact>]
let ``Middleware test fetch /blah should be 404`` () =
    async {
        let projectRootPath = 
         Path.Combine(
            [| Directory.GetCurrentDirectory(); ".."; ".."; ".."; ".."; "BoleroGitHub.Client"; "wwwroot" |]
         )
        let h =
            WebHostBuilder()
                .UseStaticWebAssets()
                .UseEnvironment("Development")
                .UseStartup<Startup>()
        use testServer = new TestServer(h)
        use client = testServer.CreateClient()
        let! response = client.GetAsync("/blah") |> Async.AwaitTask
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        Assert.False(String.IsNullOrEmpty(content))
        Assert.True(content.Contains("meta name=\"keywords\" content=\"404\""))
        Assert.False(content.Contains("meta name=\"keywords\" content=\"index\""), "index.html page served, should have been 404.html")
        
    }

module ModelTests

open System
open System.Text.Json
open System.Text.Json.Serialization
open Xunit
open BoleroGitHub.Client.Main
open BoleroGitHub.Client.Markdown
open BoleroGitHub.Client.PostPage

[<Fact>]
let ``Check can find post in model`` () =
    let pfm = FrontMatter.Post { date = DateTime.UtcNow; title = "title" }
    let r : Rendered = { FrontMatter = Some pfm; Body = ""; Summary = "" }
    let testPost title = (title, r )
    let model, cmd = initModel()
    let updatedModel = { model with posts = [ testPost "title-to-find"; testPost "another-post" ] |> Map.ofList }
    let matches title =
        findPost updatedModel title 
    
    Assert.False(matches "another-post" |> Seq.isEmpty, "could not find another-post")
    Assert.False(matches "title-to-find" |> Seq.isEmpty, "could not find title-to-find")
    Assert.True(matches "title-which-doesnt-exist" |> Seq.isEmpty)

[<JsonFSharpConverter>]
type JsonPosts = { posts : string []}

[<Fact>]
let ``Can deserialize anonymous type`` () =   
    let options = JsonSerializerOptions()
    options.Converters.Add(JsonFSharpConverter())
    let actual = JsonSerializer.Deserialize<{| posts : string[] |}>("""{"posts":["post1","post2"]}""", options)
    Assert.Equal({| posts = [|"post1";"post2"|] |}, actual)

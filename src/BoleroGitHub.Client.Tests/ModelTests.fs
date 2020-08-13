module ModelTests

open System
open Xunit
open BoleroGitHub.Client.Main
open BoleroGitHub.Client.Markdown

[<Fact>]
let ``Check can find post in model`` () =
    let p = Page.Post "title-to-find"
    let r : Rendered = { FrontMatter = None; Body = ""; Summary = "" }
    let model : Model = { 
      initModel with
        page = p;
        posts = [ (p, r); (Page.Post "another-post", r ) ] }
    let matches title = 
        model.posts 
        |> List.where (fun (p,r) -> 
            match p with 
            | Page.Post t -> t = title 
            | _ -> false)

    Assert.False(matches "title-to-find" |> Seq.isEmpty)
    Assert.False(matches "another-post" |> Seq.isEmpty)
    Assert.True(matches "title-which-doesnt-exist" |> Seq.isEmpty)
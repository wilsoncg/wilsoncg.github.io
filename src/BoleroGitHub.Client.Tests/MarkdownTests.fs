module Tests

open System
open Legivel.Serialization
open Xunit
open BoleroGitHub.Client.Markdown

[<Literal>]
let FrontMatter = 
    """layout: splash
title: "Projects"
permalink: /projects/
date: 2018-11-27 23:23:00
intro: 
  - title: "Projects" 
    excerpt: "In my free time I'm actively developing with F# and the Microsoft Azure cloud platform. I'm currently working on a web assembly application which uses Bolero, an extension to Blazor in F#."
feature_row:
  - image_path: /assets/003-webassembly.svg
    alt: "web assembly"
    title: "F# Bolero using WebAssembly & Blazor"
    excerpt: "Options trading tool written in F#. Delivered to the browser using Bolero, an F# extension of Blazor & WebAssembly, which follows the Elmish MVU (Model-View-Update) render pattern."
    url: "https://fsbolero.io/"
    btn_label: "fsbolero.io"
    btn_class: "btn--primary"
  - image_path: /assets/004-fsharp.png
    alt: "F#"
    title: "F# project contribution"
    excerpt: "Contribution towards maintenance of F# project FSharp.Control.AsyncSeq, an asynchronous extension package for the F# sequence type."
    url: "https://github.com/fsprojects/FSharp.Control.AsyncSeq"
    btn_label: "github"
    btn_class: "btn--primary" """

let ProjectsMarkdown =
    sprintf """--- 
%s 
---
some text""" FrontMatter

let dataFromDeserializeResult (r: DeserializeResult<_> list) =
    r
    |> List.head
    |> function
        | Success s -> s.Data
        | Error e -> e.ToString() |> sprintf "Deserialize failure: %s" |> failwith

[<Fact>]
let ``Check front matter is found`` () =
    let actual = parse ProjectsMarkdown
    Assert.True (Option.isSome actual.FrontMatter) 

[<Fact>]
let ``Check front matter can be parsed`` () =
    let actual = parseFrontMatter FrontMatter

    Assert.NotEmpty actual.Intro
    Assert.Equal ("Projects", actual.Intro.[0].title)

    Assert.NotEmpty (actual.FeatureRow)
    Assert.Equal ("web assembly", actual.FeatureRow.[0].alt) 


type introcontent = {| title: string; excerpt: string |}
type intro = {| intro: introcontent list |}
[<Fact>]
let ``Parse with anonymous record`` () =
    let s = """
intro: 
  - title: "Projects" 
    excerpt: "excerpt"
"""
    let trimmed = (s.Trim([| '\r'; '\n' |]))    
    let actual = Deserialize<intro> trimmed |> dataFromDeserializeResult
    
    Assert.NotEmpty actual.intro
    Assert.Equal ("Projects", actual.intro.[0].title)
    Assert.Equal ("excerpt", actual.intro.[0].excerpt)
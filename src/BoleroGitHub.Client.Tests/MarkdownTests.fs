module MarkdownTests

open System
open Legivel.Serialization
open Xunit
open BoleroGitHub.Client.Markdown

[<Literal>]
let ProjectsFrontMatter = 
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
some text""" ProjectsFrontMatter

let dataFromDeserializeResult (r: DeserializeResult<_> list) =
    r
    |> List.head
    |> function
        | Success s -> s.Data
        | Error e -> e.ToString() |> sprintf "Deserialize failure: %s" |> failwith

[<Fact>]
let ``Check front matter is found`` () =
    let actual = parse ProjectsMarkdown PageType.Projects ""
    Assert.True (Option.isSome actual.FrontMatter) 

[<Fact>]
let ``Check front matter can be parsed`` () =
    let actual = 
      parseFrontMatter ProjectsFrontMatter PageType.Projects ""
      |> function 
         | FrontMatter.Projects fm -> fm 

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

type testDate = {| date: DateTime |}
[<Fact>]
let ``Parse date`` () =
  let s = """date:  2017-07-09 16:36:51 +01:00"""
  let actual = Deserialize<testDate> s |> dataFromDeserializeResult

  Assert.Equal(2017, actual.date.Year)
  Assert.Equal(7, actual.date.Month)
  Assert.Equal(9, actual.date.Day)

[<Literal>]
let PostFrontMatter =
  """title:  "Don't fear the monad"
date:   2018-03-24 12:35:00 +01:00
categories: monads
tags: 
  - monads
  - monoids
  - category theory
  - functional programming
  - fsharp
  - linq """

[<Literal>]
let Post = """
A bunch of useful links covering monads in F# and C#.

## Videos

What is a Monad?
<a href="https://www.youtube.com/watch?v=t1e8gqXLbsU">
![What is a monad?](/assets/monads-01-what-is-a-monad.png)
</a>

Brian Beckman - Don't fear the Monad
<a href="https://www.youtube.com/watch?v=ZhuHCtR3xq8">
![Don't fear the Monad](/assets/monads-02-dont-fear-the-monad.png)
</a>
  """
let PostWithFrontmatter = 
    sprintf """--- 
%s 
---
%s""" PostFrontMatter Post

[<Fact>]
let ``Parse post``() = 
  let actual = parse PostWithFrontmatter PageType.Post ""

  Assert.True (Option.isSome actual.FrontMatter)

[<Fact>]
let ``Parse post for first 10 characters``() =
  let actual = parse PostWithFrontmatter PageType.Post ""

  Assert.Equal("A bunch of", actual.Summary.Substring (0,10))
module Tests

open System
open Xunit
open BoleroGitHub.Client.Markdown

[<Literal>]
let ProjectsMarkdown =
    """---
    layout: splash
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
        btn_class: "btn--primary
    ---"""

[<Fact>]
let ``Check markdown parsing`` () =
    
    let actual = parseFrontMatter ProjectsMarkdown
    
    Assert.False (Seq.isEmpty actual.FeatureRows) 

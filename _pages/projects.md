---
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
    btn_class: "btn--primary"
  - image_path: /assets/004-fsharp.png
    alt: "F#"
    title: "F# project contribution"
    excerpt: "Contribution towards maintenance of F# project FSharp.Control.AsyncSeq, an asynchronous extension package for the F# sequence type."
    url: "https://github.com/fsprojects/FSharp.Control.AsyncSeq"
    btn_label: "github"
    btn_class: "btn--primary"    
  - image_path: /assets/001-bot.svg
    alt: "bot"
    title: "C# Azure serverless bot"
    excerpt: "Auto deployment from github repository, running on a consumption model as an Azure serverless function. Integrated with the Microsoft LUIS service to provide responses based on language understanding."
    url: "https://github.com/wilsoncg/bot"
    btn_label: "github"
    btn_class: "btn--primary"
  - image_path: /assets/002-graphic.svg
    alt: "chart"
    title: "F# XPlot Payment chart"
    excerpt: "Combined an F# SQL type provider, with a production database snapshot and Xplot/ploty javascript library to create an interactive web dashboard displaying payment transactions over the last 6 months."
    url: "https://github.com/wilsoncg/PaymentCharts"
    btn_label: "github"
    btn_class: "btn--primary"
---

{% include feature_row id="intro" type="center" %}

{% include feature_row %}

###### Images courtesy of [https://www.flaticon.com/authors/freepik](https://www.flaticon.com/authors/freepik)
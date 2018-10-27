---
layout: splash
title: "Projects"
permalink: /projects/
date: 2018-11-27 23:23:00
intro: 
  - excerpt: "In my free time I'm actively developing with F# and the Microsoft Azure cloud platform. I'm developing a bot which runs as a serverless function and uses language understanding (LUIS) as a means to enhance my knowledge in this area."
feature_row:
  - image_path: /assets/001-bot.svg
    alt: "bot"
    title: "C# Azure serverless bot"
    excerpt: "Auto deployment from github repository, running on a consumption model as an Azure function. Integrated with the Microsoft LUIS service to provide responses based on language understanding."
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

###### Images courtesy of (https://www.flaticon.com/authors/freepik)[https://www.flaticon.com/authors/freepik]
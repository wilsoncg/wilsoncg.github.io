module BoleroGitHub.Client.Markdown

open Legivel.Serialization
open Legivel.Attributes
open Markdig
open Markdig.Extensions.Yaml
open Markdig.Renderers
open Markdig.Syntax
open System.IO
open System.Linq
open System.Text.RegularExpressions

type IntroContent =
    {
        title: string
        excerpt: string
    }
type Intro = IntroContent list

type FeatureRowContent =
    {
        title: string
        excerpt: string
        [<YamlField("image_path")>]
        imagePath: string
        alt: string
        url: string
        [<YamlField("btn_label")>]
        buttonLabel: string
        [<YamlField("btn_class")>]
        buttonClass: string
    }
type FeatureRow = FeatureRowContent list

type ProjectsFrontMatter = 
    {
        layout: string        
        [<YamlField("intro")>]
        Intro: Intro
        [<YamlField("feature_row")>]
        FeatureRow: FeatureRow
    }

type Rendered =
    {
        FrontMatter: Option<string>
        Body: string
    }

let parse markdown = 
    let pipeline = MarkdownPipelineBuilder().UseYamlFrontMatter().Build()
    use sw = new StringWriter()
    let renderer = HtmlRenderer(sw)
    pipeline.Setup(renderer) |> ignore
    let doc = Markdown.Parse(markdown, pipeline)
    let frontmatter =
        match doc.Descendants<YamlFrontMatterBlock>().FirstOrDefault() with
        | null -> None
        | yaml -> 
            let y = markdown.Substring(yaml.Span.Start, yaml.Span.Length) 
            y |> Some
    {
        FrontMatter = frontmatter
        Body = ""
    }

let parseFrontMatter (fm:string) =
    let r = Deserialize<ProjectsFrontMatter> fm
    r 
    |> List.head
    |> function
        | Success s -> s.Data
        | Error e -> e.ToString() |> sprintf "Deserialize failure: %s" |> failwith
    //DeserializerBuilder()
     //.WithAttributeOverride(fun (p:ProjectsFrontMatter) -> p.layout = "", new YamlIgnoreAttribute())
     //.Build()
     //.Deserialize<ProjectsFrontMatter>(md)
    //{ FeatureRows = [] }
module BoleroGitHub.Client.Markdown

open Legivel.Serialization
open Legivel.Attributes
open Markdig
open Markdig.Extensions.Yaml
open Markdig.Renderers
open Markdig.Syntax
open System
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

type PostFrontMatter =
    {
        title: string
        date: DateTime
    }

type PageType =
    | Projects
    | Post

type FrontMatter = 
    | Projects of ProjectsFrontMatter 
    | Post of PostFrontMatter

type Rendered =
    {
        FrontMatter: Option<FrontMatter>
        Body: string
    }

let private deserializeAndExtract<'t> s =
    let r = Deserialize<'t> s
    r 
    |> List.head
    |> function
        | Success s -> s.Data
        | Error e -> e.ToString() |> sprintf "Deserialize failure: %s" |> failwith

let parseFrontMatter (fm:string) (pt: PageType) : FrontMatter =
    match pt with
    | PageType.Projects -> deserializeAndExtract<ProjectsFrontMatter> fm |> FrontMatter.Projects
    | PageType.Post -> deserializeAndExtract<PostFrontMatter> fm |> FrontMatter.Post

let parse markdown t = 
    let pipeline = MarkdownPipelineBuilder().UseYamlFrontMatter().Build()
    use sw = new StringWriter()
    let renderer = HtmlRenderer(sw)
    pipeline.Setup(renderer) |> ignore
    let doc = Markdown.Parse(markdown, pipeline)
    renderer.Render(doc) |> ignore
    sw.Flush() |> ignore
    let frontmatter =
        match doc.Descendants<YamlFrontMatterBlock>().FirstOrDefault() with
        | null -> None
        | yaml -> 
            let y = markdown.Substring(yaml.Span.Start, yaml.Span.Length)
            let parsed = parseFrontMatter y t
            parsed |> Some
    {
        FrontMatter = frontmatter
        Body = sw.ToString()
    }


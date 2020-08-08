module BoleroGitHub.Client.Markdown

open YamlDotNet.Serialization
open Markdig

type FeatureRow() =
        [<YamlMember(Alias="title")>]
        member val Title = "" with get, set
        [<YamlMember(Alias="excerpt")>]
        member val Excerpt = "" with get, set
        [<YamlMember(Alias="image_path")>]
        member val ImagePath = "" with get, set
        [<YamlMember(Alias="alt")>]
        member val Alt = "" with get, set
        [<YamlMember(Alias="url")>]
        member val Url = "" with get, set
        [<YamlMember(Alias="btn_label")>]
        member val ButtonLabel = "" with get, set
        [<YamlMember(Alias="btn_class")>]
        member val ButtonClass = "" with get, set

type ProjectsFrontMatter() = 
    member val layout = "" with get, set
    [<YamlMember(Alias="intro")>]
    member val Intro = FeatureRow with get, set
    [<YamlMember(Alias="feature_row")>]
    member val FeatureRows = Array.empty<FeatureRow> with get, set

let parseFrontMatter (md:string) =
    DeserializerBuilder()
     //.WithAttributeOverride(fun (p:ProjectsFrontMatter) -> p.layout = "", new YamlIgnoreAttribute())
     .Build()
     .Deserialize<ProjectsFrontMatter>(md)
    //{ FeatureRows = [] }
module BoleroGitHub.Client.PostPage

open System
open System.Globalization
open System.Net.Http
open System.Threading.Tasks
open System.Text.Json
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting.Client
open Bolero.Templating.Client
open Microsoft.JSInterop
open Microsoft.AspNetCore.Components
open BoleroGitHub.Client.Markdown
open System.Text.Json.Serialization

type LoadState =
    | Loading
    | Loaded

type PostPageModel =
    { postIndex: string list
      posts: Map<string, Rendered>
      loadState: LoadState
      error: string option }

type PostPageMsg =
    | InitPostPageMsg
    | LoadPostIndex
    | LoadSinglePost of string
    | GotPost of (string * Rendered)
    | GotPostIndex of string list
    | GotPosts of (string * Rendered) list
    | Error of exn

let initModel() =
    { postIndex = list.Empty
      posts = Map.empty
      loadState = Loaded
      error = None }, Cmd.ofMsg InitPostPageMsg

let getAsync (client:HttpClient) (url:string) =
    async {
        let! response = client.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return (content, url)
    }

let private titleFromFileLocation (location:string) =
        location.Replace("posts/","").Replace(".md","")

let getPostsParallel (fileLocation, hc) =
    async {
      let! markdowns =
        fileLocation 
        |> List.map (fun p -> getAsync hc p)
        |> Async.Parallel

      let p = 
        markdowns 
        |> Array.toList
        |> List.map (fun (m, url) -> 
            let parsed = Markdown.parse m PageType.Post url
            let title = titleFromFileLocation url
            (title, parsed))
      return p
    }

let asyncLoadPostIndex httpClient = 
        Cmd.OfAsync.either (fun hc -> 
            async { 
                let! (s, _) = getAsync hc "posts/list.json"
                let options = JsonSerializerOptions()
                options.Converters.Add(JsonFSharpConverter())
                let data = JsonSerializer.Deserialize<{| posts : string[] |}>(s, options)
                let posts = 
                    match data.posts with
                    | null -> list.Empty
                    | p -> p |> Array.toList
                return posts
            }) httpClient GotPostIndex Error

let preLoadPosts httpClient postIndex =
    Cmd.OfAsync.either getPostsParallel (postIndex, httpClient) GotPosts Error

let loadSinglePost httpClient title =
    Cmd.OfAsync.either (fun hc ->
        async {
            let! (doc, _) = sprintf "posts/%s.md" title |> getAsync hc
            let parsed = Markdown.parse doc PageType.Post title
            return (title, parsed)
        }
    ) httpClient GotPost Error

let update httpClient jsRuntime message model =
    
    match message with
    | InitPostPageMsg ->
        model, Cmd.none
    | LoadPostIndex ->
        match model.postIndex.IsEmpty with
        | false -> { model with loadState = Loaded }, Cmd.none
        | true -> { model with loadState = Loading }, asyncLoadPostIndex httpClient    
    | GotPostIndex index ->
        { model with postIndex = index; loadState = Loaded }, Cmd.none
    | GotPosts posts ->
        let m = posts |> Map.ofList
        { model with posts = m; loadState = Loaded }, Cmd.none
    | LoadSinglePost p ->
        { model with loadState = Loading }, Cmd.batch [ loadSinglePost httpClient p ]
    | GotPost (title, parsed) ->
        let newPosts =
            match model.posts.TryFind title with
            | Some _ -> 
                let m = model.posts.Remove(title)
                m.Add(title, parsed)
            | None -> model.posts.Add(title, parsed)
        { model with loadState = Loaded; posts = newPosts }, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none

type PostBodyComponentModel =
    {
        RawHtml: string
    }
type PostBodyComponent() =
    inherit ElmishComponent<PostBodyComponentModel, PostPageMsg>()    

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.ShouldRender() = true

    override this.View model dispatch =
        RawHtml model.RawHtml

    override this.OnAfterRenderAsync firstRender =
        match firstRender with
        | true -> this.JSRuntime.InvokeVoidAsync("syntaxHighlight").AsTask()
        | false -> ValueTask().AsTask()

type Main = Template<"wwwroot/templateMainMinimal.html">

let renderPostSummary (url, rendered) =
    let extract (pfm:FrontMatter) =
        match pfm with
        | FrontMatter.Post p -> Some p
        | _ -> None
    let (title, date) = 
        Option.bind extract rendered.FrontMatter
        |> function
            | Some fm -> (fm.title, fm.date.ToString("dd/MM/yyyy"))
            | None -> ("","")

    Main.PostSummary()
        .date(date)
        .posturl(url)
        .headline(title)
        .description(rendered.Summary)
        .Elt()

let showSimplePostList (model:PostPageModel) dispatch =
    let byDescending = 
        model.postIndex
        |> List.sortByDescending id
        |> List.map (fun pi ->
            let url = titleFromFileLocation pi
            let partial (xs : string seq) = String.Join('-', xs)
            let title = 
                (titleFromFileLocation pi).Split('-')
                |> Seq.skip 3
                |> partial
            
            let date = 
                (titleFromFileLocation pi).Split('-')
                |> Seq.take 3
                |> partial
                |> fun s -> DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None)
                |> function 
                    | (true, d) -> d
                    | (false, _) -> DateTime.UtcNow 

            let pfm = { title = title; date = date }
            let rendered = { 
                FrontMatter = Some(FrontMatter.Post pfm); 
                Body = ""; 
                Summary = "" }
            (url, rendered)
        )
    Main
        .Posts()
        .PostsList(forEach byDescending renderPostSummary)
        .Elt()

let showFullPostList (model:PostPageModel) dispatch =
    let byDescending = 
        model.posts
        |> Map.toList
        |> List.sortByDescending (fun (key, _) -> key)
    Main
        .Posts()
        .PostsList(forEach byDescending renderPostSummary)
        .Elt()

let showPost post title dispatch =
    let rendered = post
    let extract (pfm:FrontMatter) =
        match pfm with
        | FrontMatter.Post p -> Some p
        | _ -> None
    let (title, date) = 
        Option.bind extract rendered.FrontMatter
        |> function
            | Some fm -> (fm.title, fm.date)
            | None -> ("",DateTime.UtcNow)
    let postBody =
        ecomp<PostBodyComponent,_,_> [] { RawHtml = rendered.Body } dispatch
    
    Main
        .Post()
        .title(title)
        .Body(postBody)
        .datetime(date.ToString("yyyy-MM-ddTHH:mm:ssZ"))
        .shortdate(date.ToString("d MMM, yyyy"))
        .Elt()

let findPost model title =
        model.posts 
        |> Map.toList
        |> List.where (fun (p, r) -> p = title)
        |> List.choose (fun (p,r) -> 
            match r.FrontMatter with
            | Some f -> Some r
            | _ -> None)

let textInMain t =
    div [ attr.id "main-content" ; "role" => "main" ] [
        div [ attr.``class`` "archive" ] [
            text t
        ]
    ]

let postPage (model:PostPageModel) title dispatch =
    cond model.loadState <| function
    | Loading -> textInMain "Loading..."
    | Loaded ->
        let tryFind = Map.tryFind title model.posts     
        cond tryFind <| function
        | None -> Main.ErrorNotification().ErrorText("No post found").Elt()
        | Some _ -> showPost model.posts.[title] title dispatch
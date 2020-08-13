module BoleroGitHub.Client.Main

open System
open System.IO
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting.Client
open Bolero.Templating.Client
open BoleroGitHub.Client.Markdown
open System.Net.Http
open System.Collections.Generic

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/posts">] Posts
    | [<EndPoint "/post/{postTitle}">] Post of postTitle: string
    | [<EndPoint "/projects">] Projects

type Toggle = On | Off
type LoadState = 
 | Loading 
 | Loaded
 
/// The Elmish application's model.
type Model =
    {
        page: Page
        markdowns: Map<PageType, Rendered>
        postIndex: string list
        posts: (Page * Rendered) list
        loadState: LoadState
        searchToggle: Toggle
        searchTerm: string
        error: string option
    }

let initModel =
    {
        page = Home
        markdowns = Map.empty
        postIndex = list.Empty
        posts = list.Empty
        loadState = Loaded
        searchToggle = Off
        searchTerm = ""
        error = None
    }

/// The Elmish application's update messages.
type Message =
    | LoadingPage of Page
    | GotProjects of Page * Rendered
    | GotPostIndex of string list
    | GotPosts of (Page * Rendered) list
    | SetPage of Page
    | SearchToggle of Toggle
    | SearchTerm of string
    | Error of exn
    | ClearError

let getAsync (client:HttpClient) (url:string) =
    async {
        let! response = client.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }
let getProjectsMd hc = 
    async {
        let! markdown = getAsync hc "/pages/projects.md" 
        let parsed = Markdown.parse markdown PageType.Projects
        return (Projects, parsed)
    }
let getPosts (posts, hc) =
    async {
      let! markdowns =
        posts 
        |> List.map (fun p -> getAsync hc p)
        |> Async.Parallel
      let p = 
        markdowns 
        |> Array.toList
        |> List.map (fun m -> 
            let parse = Markdown.parse m PageType.Post
            (Posts, parse))
      return p
    }

let update httpClient message model =
    let preLoadProjects = Cmd.ofAsync getProjectsMd httpClient GotProjects Error
    let getPostIndex = 
        Cmd.ofAsync (fun hc -> 
            async { 
                let! s = getAsync hc "posts/index.txt" 
                return s.Split Environment.NewLine |> Array.toList
            }) httpClient GotPostIndex Error
    let preLoadPosts posts =
        Cmd.ofAsync getPosts (posts, httpClient) GotPosts Error
    match message with
    | SetPage page ->
        { model with page = page },
        match page with
        | Home -> Cmd.none
        | Projects -> Cmd.ofMsg (LoadingPage Projects)
        | Posts -> Cmd.ofMsg (LoadingPage Posts)
        | _ -> Cmd.none
    | LoadingPage page ->
        let loading = { model with loadState = Loading} 
        match page with 
        | Projects -> loading, preLoadProjects
        | Posts -> loading, getPostIndex
        | _ -> model, Cmd.none
    | GotProjects (p,md) ->
        let m = Map.add PageType.Projects md model.markdowns
        { model with markdowns = m; loadState = Loaded }, Cmd.none
    | GotPostIndex index ->
        { model with postIndex = index }, preLoadPosts index
    | GotPosts posts ->
        { model with posts = posts; loadState = Loaded }, Cmd.none
    | SearchToggle toggle ->
        { model with searchToggle = if toggle = On then Off else On }, Cmd.none
    | SearchTerm term ->
        { model with searchTerm = term}, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/templateMainMinimal.html">

let homePage model dispatch =
  let splashImages = [ "splash-murcia.jpg"; "splash-blue-min.png"; "splash-retro-car.jpg" ]
  let r = Random().Next(splashImages.Length)
  Main
    .Home()
    .SplashImage(splashImages.[r])
    .Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let private showProject (project: FeatureRowContent) =
    Main.ProjectsItem()
        .imagePath(project.imagePath)
        .alt(project.alt)
        .title(project.title)
        .excerpt(project.excerpt)
        .url(project.url)
        .buttonClass(project.buttonClass)
        .buttonLabel(project.buttonLabel)
        .Elt()

let private listProjects (projects: ProjectsFrontMatter) (body:string) =
    Main
        .Projects()
        .title(projects.Intro.Head.title)
        .excerpt(projects.Intro.Head.excerpt)
        .ProjectsList(forEach projects.FeatureRow showProject)
        .Body(RawHtml body)
        .Elt()

let projectsPage (model:Model) =
    let exists = Map.containsKey PageType.Projects model.markdowns
    cond (exists) <| function  
     | true ->
        let md = Map.find PageType.Projects model.markdowns
        match md.FrontMatter with
        | Some fm ->
            match fm with
            | FrontMatter.Projects p -> listProjects p md.Body
            | _ -> empty
        | None -> empty
     | false -> empty

let textInMain t =
    div [ attr.id "main-content" ; "role" => "main" ] [
        div [ attr.``class`` "archive" ] [
            text t
        ]
    ]

let showPostSummary (post:PostFrontMatter) =
    Main.PostSummary()
        .headline(post.title)
        .Elt()

let postsPage (model:Model) =
    let p =  
        model.posts 
        |> List.map (fun (p,r) -> r.FrontMatter)
        |> List.choose id
        |> List.map (fun fm -> 
            match fm with
            | FrontMatter.Post p -> Some p
            | _ -> None)
        |> List.choose id 
        // |> List.map (Option.bind (fun fm ->
        //     match fm with
        //     | FrontMatter.Post p -> showPostSummary p
        //     | _ -> empty))
        // |> function
        //     | Some fm ->
        //         match fm with
        //         | FrontMatter.Post p -> [p]
        //         | _ -> []
        //     | None -> [])
    Main
        .Posts()
        .PostsList(forEach p showPostSummary)
        .Elt()

let view model dispatch =
    Main()
        .Menu(concat [
            menuItem model Home "Home";
            menuItem model Projects "Projects";
            menuItem model Posts "Posts";
        ])
        .SearchToggle(fun _ -> dispatch (SearchToggle model.searchToggle))
        .SearchTerm(model.searchTerm, fun s -> dispatch(SearchTerm s))
        .ContentIsVisible(if model.searchToggle = On then "is--hidden" else "")
        .SearchIsVisible(if model.searchToggle = On then "is--visible" else "")
        .Body(
            cond (model.loadState, model.page) <| function
            | Loading, _ -> textInMain "Loading..."
            | _, Home -> homePage model dispatch
            | _, Projects -> projectsPage model
            | _, Posts -> postsPage model      
            | _,_ -> textInMain "Not Implemented"
        )
        .Year(DateTime.UtcNow.Year |> string |> text)
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let hc = this.Services.GetService(typeof<HttpClient>) :?> HttpClient
        Program.mkProgram (fun _ -> 
            initModel, 
            Cmd.ofMsg(SetPage Home)) (update hc) view
        |> Program.withRouter router
#if DEBUG
        |> Program.withConsoleTrace
        |> Program.withHotReload
#endif

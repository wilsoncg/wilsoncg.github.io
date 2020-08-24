module BoleroGitHub.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting.Client
open Bolero.Templating.Client
open System.Net.Http
open System.Collections.Generic
open Microsoft.JSInterop
open Microsoft.AspNetCore.Components
open BoleroGitHub.Client.Markdown

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
        posts: PostPage.PostPageModel
        projects: Map<PageType, Rendered>      
        searchToggle: Toggle
        loadState: LoadState
        searchTerm: string
        error: string option
    }

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | PostPageMsg of PostPage.PostPageMsg
    | LoadProjects
    | GotProjects of Page * Rendered
    | SearchToggle of Toggle
    | SearchTerm of string
    | Error of exn
    | ClearError

let initModel =
    let initPostsPage, initPostsPageCmd = PostPage.initModel()
    let initState = {
        page = Home
        posts = initPostsPage
        projects = Map.empty
        loadState = Loaded
        searchToggle = Off
        searchTerm = ""
        error = None
    }
    let initCmd = Cmd.batch [
        Cmd.map PostPageMsg initPostsPageCmd
    ]
    initState, initCmd


let getAsync (client:HttpClient) (url:string) =
    async {
        let! response = client.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return (content, url)
    }
let getProjectsMd hc = 
    async {
        let url = "/pages/projects.md" 
        let! (markdown, _) = getAsync hc url
        let parsed = Markdown.parse markdown PageType.Projects url
        return (Projects, parsed)
    }

let update httpClient jsRuntime message model =
    let asyncLoadProjects = Cmd.ofAsync getProjectsMd httpClient GotProjects Error
    
    match message with
    | SetPage page ->
        { model with page = page },
        match page with
        | Projects -> Cmd.ofMsg LoadProjects
        | _ -> Cmd.none
    | PostPageMsg msg ->
        let nextState, nextCmd = PostPage.update httpClient jsRuntime msg model.posts
        let appState = { model with posts = nextState; error = nextState.error }
        appState, Cmd.map PostPageMsg nextCmd
    | LoadProjects ->
        match model.projects.IsEmpty with
        | false -> { model with loadState = Loaded }, Cmd.none
        | true ->  { model with loadState = Loading}, asyncLoadProjects
    | GotProjects (p,md) ->
        let m = Map.add PageType.Projects md model.projects
        { model with projects = m; loadState = Loaded }, Cmd.none    
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
    let exists = Map.containsKey PageType.Projects model.projects
    cond (exists) <| function  
     | true ->
        let md = Map.find PageType.Projects model.projects
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
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err ->
                Main.ErrorNotification()
                    .ErrorText(err)
                    .Elt()
        )
        .Body(
            cond model.loadState <| function
            | Loading -> textInMain "Loading..."
            | Loaded -> 
            cond model.page <| function
            | Home -> homePage model dispatch
            | Projects -> projectsPage model
            | Posts -> PostPage.showPostList model.posts (PostPageMsg >> dispatch)
            | Post t -> PostPage.postPage model.posts t (PostPageMsg >> dispatch)
        )
        .Year(DateTime.UtcNow.Year |> string |> text)
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let hc = this.Services.GetService(typeof<HttpClient>) :?> HttpClient
        let jsRuntime = this.JSRuntime
        let update = update hc jsRuntime
        Program.mkProgram (fun _ -> initModel) update view
        |> Program.withRouter router        
#if DEBUG
        |> Program.withConsoleTrace
        |> Program.withHotReload
#endif

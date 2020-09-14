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

type Size(h:int, w:int) =
    let mutable h = h
    let mutable w = w
    member this.Height with get() = h and set(value) = h <- value
    member this.Width with get() = w and set(value) = w <- value
    new() = Size(0,0)

type Callback =
    static member OfSize(f) =
        DotNetObjectReference.Create(SizeCallback(f))

and SizeCallback(f: Size -> unit) =
    [<JSInvokable>]
    member this.Invoke(arg1, arg2) =
        f (Size(arg1, arg2))

/// The Elmish application's model.
type Model =
    {
        page: Page
        posts: PostPage.PostPageModel
        projects: Map<PageType, Rendered>      
        searchToggle: Toggle
        hamburgerVisible: bool
        hamburgerMenuButtonToggle: Toggle
        splashImage: string
        loadState: LoadState
        searchTerm: string
        error: string option
    }

/// The Elmish application's update messages.
type Message =
    | Initialize
    | SetPage of Page
    | PostPage of PostPage.PostPageMsg
    | LoadProjects
    | GotProjects of Page * Rendered
    | SearchToggle of Toggle
    | SearchTerm of string
    | WindowResize of Size
    | HamburgerMenuToggle of Toggle
    | Error of exn
    | ClearError

let initModel =
    let splash =
        let images = [ "splash-murcia.jpg"; "splash-blue-min.png"; "splash-retro-car.jpg" ]
        let r = Random().Next(images.Length)
        images.[r]
    let initPostsPage, initPostsPageCmd = PostPage.initModel()
    let initState = {
        page = Home
        posts = initPostsPage
        projects = Map.empty
        loadState = Loaded
        searchToggle = Off
        hamburgerVisible = false
        hamburgerMenuButtonToggle = Off
        splashImage = splash
        searchTerm = ""
        error = None
    }
    let initCmd = Cmd.batch [        
        Cmd.ofMsg Initialize;
        Cmd.map PostPage initPostsPageCmd
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

let update httpClient (jsRuntime:IJSRuntime) message model =
    let asyncLoadProjects = Cmd.ofAsync getProjectsMd httpClient GotProjects Error
    let setupJSCallback = 
        Cmd.ofSub (fun dispatch -> 
            // given a size, dispatch a message
            let onResize = dispatch << WindowResize
            jsRuntime.InvokeVoidAsync("generalFunctions.initResizeCallback", Callback.OfSize onResize).AsTask() |> ignore
        )
    let fireInitialWindowSize =
        Cmd.ofJS jsRuntime "generalFunctions.getSize" [||] WindowResize Error 

    match message with
    | Initialize ->
        model, Cmd.batch [
            setupJSCallback;
            fireInitialWindowSize;
        ] 
    | SetPage page ->
        { model with page = page },
        match page with
        | Projects -> Cmd.ofMsg LoadProjects
        | Posts -> Cmd.map PostPage (Cmd.ofMsg PostPage.LoadPostIndex)
        | Post p -> Cmd.map PostPage (Cmd.ofMsg (PostPage.LoadSinglePost p))
        | _ -> Cmd.none
    | PostPage msg ->
        let nextState, nextCmd = PostPage.update httpClient jsRuntime msg model.posts
        let appState = { model with posts = nextState; error = nextState.error }
        appState, Cmd.map PostPage nextCmd
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
    | WindowResize size ->
        let visible = size.Width < 450
        { model with hamburgerVisible = visible }, Cmd.none
    | HamburgerMenuToggle toggle ->
        { model with hamburgerMenuButtonToggle = if toggle = On then Off else On}, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/templateMainMinimal.html">

let homePage model dispatch =  
  Main
    .Home()
    .SplashImage(model.splashImage)
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

type CollapsibleMenuComponentModel =
    {
        MenuItems: Node list
        HamburgerVisible: bool
        HamburgerMenuButtonToggle: Toggle
    }
type CollapsibleMenuComponent() =
    inherit ElmishComponent<CollapsibleMenuComponentModel, Message>()    

    [<Inject>]
    member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

    override this.View model dispatch =
        let (buttonVisible, standardLinks) = 
            match model.HamburgerVisible with
            | false -> ("hidden", "")
            | true -> ("visible", "hidden")
        let hamburgerMenuVisible =
            match model.HamburgerMenuButtonToggle with
            | On -> "visible"
            | Off -> "hidden"
        
        let partial f x y = f(x,y)
        let cssClass (s: string list) = s |> partial String.Join " "

        concat [ 
            ul [ attr.``class`` (cssClass ["visible-links"; standardLinks])] [
                concat model.MenuItems
            ];
            button [ 
                attr.``class`` (cssClass ["greedy-nav__toggle"; buttonVisible]); 
                attr.``type`` "button";
                on.click (fun _ -> 
                    dispatch (HamburgerMenuToggle model.HamburgerMenuButtonToggle)) ] [
                    span [ attr.``class`` "visually-hidden" ] [ text "Toggle menu"]
                    div [ attr.``class`` "navicon" ] []
            ];
            ul [ attr.``class`` (cssClass ["hidden-links"; hamburgerMenuVisible])] [
                concat model.MenuItems
            ];
        ]

let view model dispatch =
    let menuItems = [
            menuItem model Home "Home";
            menuItem model Projects "Projects";
            menuItem model Posts "Posts";
        ]
    let collapsibleMenu = 
        ecomp<CollapsibleMenuComponent,_,_> [] 
         { 
             MenuItems = menuItems
             HamburgerVisible = model.hamburgerVisible
             HamburgerMenuButtonToggle = model.hamburgerMenuButtonToggle
         } 
         dispatch

    Main()
        .Menu(collapsibleMenu)
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
            | Posts -> PostPage.showSimplePostList model.posts (PostPage >> dispatch)
            | Post t -> PostPage.postPage model.posts t (PostPage >> dispatch)
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

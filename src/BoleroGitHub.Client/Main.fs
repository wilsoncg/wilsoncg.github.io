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
    | [<EndPoint "/projects">] Projects    

type Toggle = On | Off
/// The Elmish application's model.
type Model =
    {
        page: Page
        markdowns: Dictionary<string, Rendered>
        searchToggle: Toggle
        searchTerm: string
        error: string option
    }

let initModel =
    {
        page = Home
        markdowns = Dictionary<string, Rendered>() 
        searchToggle = Off
        searchTerm = ""
        error = None
    }

/// The Elmish application's update messages.
type Message =
    | Initialize
    | GotMarkdown of Dictionary<string, Rendered>
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
        let parsed = Markdown.parse markdown
        let d = Dictionary<string, Rendered>()
        d.Add("projects", parsed)
        return d
    }

let update httpClient message model =
    match message with
    | Initialize ->
        model, 
        Cmd.ofAsync getProjectsMd httpClient GotMarkdown Error
    | GotMarkdown dictionary ->
        { model with markdowns = dictionary}, Cmd.ofMsg (SetPage Home)
    | SetPage page ->
        { model with page = page }, Cmd.none
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

let private listProjects (projects: FeatureRowContent list) =
    Main
        .Projects()
        .ProjectsList(forEach projects showProject)
        .Elt()

let projectsPage (model:Model) dispatch =
    let s = model.markdowns.["projects"]
    // let p = 
    //     s.FrontMatter
    //     |> function 
    //      | Some fm -> listProjects fm.FeatureRow
    //      | None _ -> div [] []
    // p
    cond s.FrontMatter.IsSome <| function 
        | true -> listProjects s.FrontMatter.Value.FeatureRow
        | false -> empty

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
            cond model.page <| function
            | Home -> homePage model dispatch
            | Posts -> Text("Not Implemented")
            | Projects -> projectsPage model dispatch
        )
        .Year(DateTime.UtcNow.Year |> string |> text)
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let hc = this.Services.GetService(typeof<HttpClient>) :?> HttpClient
        Program.mkProgram (fun _ -> 
            initModel, 
            Cmd.ofMsg(Initialize)) (update hc) view
        |> Program.withRouter router
#if DEBUG
        |> Program.withConsoleTrace
        |> Program.withHotReload
#endif

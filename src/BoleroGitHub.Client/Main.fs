module BoleroGitHub.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting.Client
open Bolero.Templating.Client

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
        searchToggle: Toggle
        searchTerm: string
        error: string option
    }

let initModel =
    {
        page = Home
        searchToggle = Off
        searchTerm = ""
        error = None
    }

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | SearchToggle of Toggle
    | SearchTerm of string
    | Error of exn
    | ClearError

let update message model =
    match message with
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
            | Projects -> Text("Not Implemented")
        )
        .Year(DateTime.UtcNow.Year |> string |> text)
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg(SetPage Home)) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif

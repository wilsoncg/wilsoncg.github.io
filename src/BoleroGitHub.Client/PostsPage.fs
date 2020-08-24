module BoleroGitHub.Client.PostPage

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
    | LoadPost of string
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

let getPosts (fileLocation, hc) =
    let titleFromFileLocation (location:string) =
        location.Replace("posts/","").Replace(".md","")
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
        Cmd.ofAsync (fun hc -> 
            async { 
                let! (s, _) = getAsync hc "posts/index.txt" 
                let split = 
                    s.Trim([| '\r'; '\n' |])
                    |> fun s -> s.Split Environment.NewLine
                    |> Array.toList
                    |> List.sortByDescending id
                return split
            }) httpClient GotPostIndex Error

let preLoadPosts httpClient postIndex =
    Cmd.ofAsync getPosts (postIndex, httpClient) GotPosts Error

let update httpClient jsRuntime message model =
    
    match message with
    | InitPostPageMsg ->
        model, Cmd.ofMsg LoadPostIndex
    | LoadPostIndex ->
        match model.postIndex.IsEmpty with
        | false -> { model with loadState = Loaded }, Cmd.none
        | true -> { model with loadState = Loading }, asyncLoadPostIndex httpClient
    | LoadPost p ->
        if not model.postIndex.IsEmpty && not model.posts.IsEmpty then
            { model with loadState = Loaded }, Cmd.none
        else
            { model with loadState = Loading }, Cmd.batch [ asyncLoadPostIndex httpClient ]
    | GotPostIndex index ->
        { model with postIndex = index }, preLoadPosts httpClient index
    | GotPosts posts ->
        // let partial f x y = f(x,y)
        // let keys = posts |> List.map fst |> partial String.Join ","
        // printfn "keys retrieved: %s" keys
        let m = posts |> Map.ofList
        { model with posts = m; loadState = Loaded }, Cmd.none
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

    override this.OnAfterRenderAsync _ =
        this.JSRuntime.InvokeVoidAsync(
            "hljs.initHighlighting")
            .AsTask()

type Main = Template<"wwwroot/templateMainMinimal.html">

let renderPostSummary (post:KeyValuePair<string, Rendered>) =
    let rendered = post.Value
    let url = post.Key
    let extract (pfm:FrontMatter) =
        match pfm with
        | FrontMatter.Post p -> Some p
        | _ -> None
    let (title, date) = 
        Option.bind extract rendered.FrontMatter
        |> function
            | Some fm -> (fm.title, fm.date.ToString("MM/dd/yyyy"))
            | None -> ("","")

    Main.PostSummary()
        .date(date)
        .posturl(url)
        .headline(title)
        .description(rendered.Summary)
        .Elt()

let showPostList (model:PostPageModel) dispatch =
    Main
        .Posts()
        .PostsList(forEach model.posts renderPostSummary)
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

let compareWith (comparer:'T -> 'T -> int) (list1: 'T list) (list2: 'T list) =
    let common: 'T list = List.empty
    let rec loop list1 list2 c =
         match list1, list2 with
         | head1 :: tail1, head2 :: tail2 ->
               let r = comparer head1 head2
               if r = 0 then loop tail1 tail2 (List.append c [head1]) else (r, common)
         | [], [] -> 0, c
         | _, [] -> 1, c
         | [], _ -> -1, c
    loop list1 list2 common

let rec lcs list1 list2 =
  if List.isEmpty list1 || List.isEmpty list2 then
    List.Empty
  else
    let tail1 = List.tail list1
    let tail2 = List.tail list2
    if List.head list1 = List.head list2 then      
      List.head list1 :: lcs tail1 tail2
    else
      let candidate1 = lcs list1 tail2
      let candidate2 = lcs tail1 list2
      if List.length candidate1 > List.length candidate2 then
        candidate1
      else
        candidate2

let lcs' list1 list2 =
    let rec loop list1 list2 =
        // match list1, list2 with
        // | [], [] -> List.empty
        // | head1 :: tail1, head2 :: tail2
        //     if head1 = head2 then                 
      if List.isEmpty list1 || List.isEmpty list2 then
        List.Empty
      else
        let tail1 = List.tail list1
        let tail2 = List.tail list2
        if List.head list1 = List.head list2 then      
          List.head list1 :: loop tail1 tail2
        else
          let candidate1 = loop list1 tail2
          let candidate2 = loop tail1 list2
          if List.length candidate1 > List.length candidate2 then
            candidate1
          else
            candidate2
    loop list1 list2

let postPage (model:PostPageModel) title dispatch =
    //let matches = findPost model title
    let comparison (s1:string) (s2:string) = 
        let s1' = s1.ToCharArray() |> Array.toList
        let s2' = s2.ToCharArray() |> Array.toList
        compareWith (fun x y -> if x = y then 0 else 1) s1' s2'
        //lcs s1' s2'
        |> function (i, r) -> r 
        |> List.toArray 
        |> String

    printfn "containsKey %s %b" title (model.posts.ContainsKey title)
    let partial f x y = f(x,y)
    let keys = model.posts |> Map.toList |> List.map fst |> partial String.Join "," //List.reduce (fun x y -> String.Join
    printfn "keys %s" keys
    let matches = 
        model.posts 
        |> Map.toList 
        |> List.map (fun (t,_) -> sprintf "compare %s=%s %s" t title (comparison t title))//comparison t title)
        //|> List.choose (fun (n, c) -> c)
        //|> string
        |> partial String.Join ","
    printfn "matches %s" matches
    
    cond (model.posts.ContainsKey title) <| function
    | false -> Main.ErrorNotification().ErrorText("No post found").Elt()
    | true -> showPost model.posts.[title] title dispatch
    // | true -> textInMain "No post found"
    // | false -> showPost matches.Head title dispatch
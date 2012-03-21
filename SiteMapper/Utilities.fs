namespace SiteMapper

open System
open System.Collections.Concurrent
open System.IO
open System.Text.RegularExpressions
open System.Windows
open System.Windows.Controls
open System.Xml.Linq
open SEOLib.Links
open SEOLib.Types
open Types

module Utilities =

    /// Finds a control in a window by its name.
    let findControl (window : Window) name = window.FindName name

    /// Spawns an agent that appends strings to the contents of a textbox.
    let progressReporter context (textbox : TextBox) =
        MailboxProcessor.Start(fun z ->
            let rec loop() =
                async {
                    let! msg = z.Receive()
                    match msg with
                        | Progress str ->
                            do! Async.SwitchToContext context
                            textbox.AppendText str
                            textbox.ScrollToEnd()
                            return! loop()
                        | _ -> 
                            do! Async.SwitchToContext context
                            textbox.AppendText "Done."
                            (z :> IDisposable).Dispose()
                }
            loop())

    /// Sends progress messages to an agent.
    let reportProgress (agent : Message'Agent) msg = agent.Post <| Progress msg

    /// Displays a message box in front of the specified window.
    let displayMsgBox (window : Window) msg = MessageBox.Show(window, msg) |> ignore

    /// Attempts to construct a Uri from a string.
    let tryCreateUri url =
        let uri = Uri.TryCreate(url, UriKind.Absolute)
        match uri with
            | true, uri' -> Some uri'
            | _          ->
                let uri'' = Uri.TryCreate("http://" + url, UriKind.Absolute)
                match uri'' with
                    | true, x -> Some x
                    | _       -> None

    let domainNamePattern = "[^\.]+\.\w{2,3}(\.\w{2})?"

    let tryCreateUri' (x : Uri option) =
        match x with
            | Some x' ->
                let host = x'.Host
                let path = x'.AbsolutePath
                let host' = Regex(domainNamePattern, RegexOptions.RightToLeft).Match(host).Value
                let pattern = "(?i)^https?://((www\.)|([^\.]+\.))" + Regex.Escape(host') + "[^\"]*"
                let isMatch = Regex(pattern).IsMatch(string x')
                match isMatch with
                    | true  -> "http://" + host + path
                    | false -> "http://www." + host + path
            | None -> ""

    let tryCreateUri'' = tryCreateUri >> tryCreateUri'

    /// Canonicalizes a URL: example.com -> http://www.example.com/
    let canonicalize url = tryCreateUri'' url

    /// Returns the host of a URL.
    let hostFromUrl url =
        let uri = Uri url
        uri.Host

    let xname str = XName.Get str
    let sitemapsNamespace = XNamespace.Get "http://www.sitemaps.org/schemas/sitemap/0.9"
    let createElement (ns : XNamespace) name (value : string) = XElement(ns + name, value)
    let createElement' = createElement sitemapsNamespace
    let rand = Random()

    let addElement (xelem : XElement) (xelem' : XElement option) =
        match xelem' with
            | Some xelem'' -> xelem.Add xelem''
            | None         -> ()

    let desktopPath = Environment.GetFolderPath Environment.SpecialFolder.Desktop
    let desktopPath' = Path.Combine(desktopPath, "Sitemap")

    let createXdocument() =
        let doc = XDocument()
        doc.Declaration <- XDeclaration("1.0", "UTF-8", "true")
        doc

    let collectLinks onlyInternal (agent : Message'Agent) (bag : ConcurrentBag<WebPage>) url host =
            let webPage = fetchUrl url
            let msg = sprintf "Crawling: %s\n" url |> Progress
            agent.Post msg
            match webPage with
                | Some webPage' ->
                    let isNoIndex = webPage'.ResponseUri.IsNone
                    match isNoIndex with
                        | true  -> ()
                        | false -> bag.Add webPage'
                    let isNoFollow = webPage'.NoFollow
                    match isNoFollow with
                        | true  -> []
                        | false ->
                            let html = webPage'.HTML
                            scrapeLinks host html onlyInternal
                | None -> []

    let ghostAgent = new MessageAgent(fun _ -> async { do () })
    
    // A reference cell for holding the cancling agent responsible for
    // instructing the crawler to cancel exploring the Web site.    
    let agent = ref ghostAgent
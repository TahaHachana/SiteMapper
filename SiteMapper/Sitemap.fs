namespace SiteMapper

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions
open System.Xml.Linq
open Settings
open Types
open GUI
open Utilities
//open SEOLib.Links
open SEOLib.Types
open SEOLib.Crawler

module Sitemap =

//    let xname str = XName.Get str
//    let sitemapsNamespace = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9")
//    let createElement (ns : XNamespace) name (value : string) = XElement(ns + name, value)
//    let createElement' = createElement sitemapsNamespace
//    let rand = Random()


//    let processSettings' (settings : Settings) =
//        match settings.Frequency with
//            | Some x ->
//                let x' = x.ToString()
//                let elem = createElement' "changefreq" x'
//                Some elem
//            | None -> None
//
//    let processSettings'' (settings : Settings) =
//        match settings.Priority with
//            | Some p ->
//                match p with
//                | Auto ->
//                    let value = rand.Next(1, 9) |> float
//                    let value' = value / 10. |> string
//                    let elem = createElement' "priority" value'
//                    Some elem
//                | Value value ->
//                    let elem = createElement' "priority" value
//                    Some elem
//            | None -> None

//    let addElement (xelem : XElement) (xelem' : XElement option) =
//        match xelem' with
//            | Some xelem'' -> xelem.Add xelem''
//            | None -> ()

    /// Creates a Sitemap entry.
//        let genEntry (url : string) (settings : Settings) lm =
//            let urlElement = XElement(sitemapsNamespace + "url")
//            let addElement' = addElement urlElement
//            createElement' "loc" url |> Some |> addElement'
//            processSettings   settings lm |> addElement'
//            processSettings'  settings    |> addElement'
//            processSettings'' settings    |> addElement'
//            urlElement

    let genEntry (webPage : WebPage) (settings : Settings) =
        let lm =
            webPage.Headers
            |> List.tryFind (fun x -> fst x = "Last-Modified")
            |> function Some x -> snd x | None -> ""
        let url = webPage.ResponseUri.Value.ToString()
        let urlElement = XElement(sitemapsNamespace + "url")
        let addElement' = addElement urlElement
        createElement' "loc" url |> Some |> addElement'
        processSettings   settings lm |> addElement'
        processSettings'  settings    |> addElement'
        processSettings'' settings    |> addElement'
        urlElement

//    let desktopPath = Environment.GetFolderPath Environment.SpecialFolder.Desktop
//    let desktopPath' = Path.Combine(desktopPath, "Sitemap")
//    do Directory.CreateDirectory desktopPath' |> ignore

//    let createXdocument() =
//        let doc = XDocument()
//        doc.Declaration <- XDeclaration("1.0", "UTF-8", "true")
//        doc

    let rec save (elements : XElement seq) skip x =
        let xdocument = createXdocument()
        let root = XElement(sitemapsNamespace + "urlset")
        xdocument.Add root
        let urls = elements |> Seq.skip skip 
        let len = urls |> Seq.length
        match len >= 50000 with
            | true ->
                urls |> Seq.take 50000 |> Seq.iter (fun x -> root.Add x)
                let fileName = sprintf "Sitemap%d.xml" x
                let path = Path.Combine(desktopPath', fileName)
                xdocument.Save path
                save elements (skip + 50000) (x + 1)
            | false ->
                match len with
                    | 0 -> ()
                    | _ ->
                        urls |> Seq.iter (fun x -> root.Add x)
                        let name = sprintf "sitemap%d.xml" x
                        let path = Path.Combine(desktopPath', name)
                        xdocument.Save path

    let generateSitemap (url : string) context =
        cancelButton.IsEnabled <- true
        let settings = settings()
        async {
            let progressReporter' = progressReporter context progressTextbox
            let robotsMsg = Progress "Fetching the robots.txt file...\n"
            progressReporter'.Post robotsMsg
            let q = ConcurrentQueue<string>()
            let set = HashSet<string>()
            let bag = ConcurrentBag<WebPage>()
            let f =
                async {
                        let elements = bag |> Seq.map (fun x-> genEntry x settings)
                        save elements 0 0
                        do! Async.SwitchToContext context
                        cancelButton.IsEnabled <- false
                        progressReporter'.Post Message'.Done
                        showMsg "Sitemap generation was successfully completed."
                    }
            let collectLinks' = collectLinks true progressReporter' bag
            let canceler = crawl url None f collectLinks'
            agent := canceler
        }
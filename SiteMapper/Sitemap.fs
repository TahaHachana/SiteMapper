namespace SiteMapper

open System.Collections.Concurrent
open System.Collections.Generic
open System.IO
open System.Xml.Linq
open SEOLib.Crawler
open SEOLib.Robots
open SEOLib.Links
open SEOLib.Types
open GUI
open Settings
open Types
open Utilities
open System.Net.Http

module Sitemap =

    /// Generates a Sitemap entry.
    let genEntry webPage settings =
        let lm =
            webPage.Headers
            |> Seq.tryFind (fun x -> x.Key = "Last-Modified")
            |> function Some x -> x.Value |> Seq.nth 0 | None -> ""
        let url = webPage.Url //.ResponseUri.Value.ToString()
        let urlElement = XElement(sitemapsNamespace + "url")
        let addElement' = addElement urlElement
        createElement' "loc" url |> Some |> addElement'
        processSettings   settings lm |> addElement'
        processSettings'  settings    |> addElement'
        processSettings'' settings    |> addElement'
        urlElement

    /// Saves Sitemap file(s) in the Sitemap folder on the desktop.
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
            progressReporter'.Post robotsMsg
            let bag = ConcurrentBag<WebPage>()
            let fWebPage (webPage : WebPage) =
                let msg = sprintf "Crawling: %s\n" webPage.Url |> Progress
                progressReporter'.Post msg
                bag.Add webPage
            let f =
                async {
                        let elements = bag |> Seq.map (fun x -> genEntry x settings)
                        save elements 0 0
                        do! Async.SwitchToContext context
                        cancelButton.IsEnabled <- false
                        progressReporter'.Post Message'.Done
                        showMsg "Sitemap generation was successfully completed."
                    }
            let canceler = crawl url None f fWebPage true OFF
            agent := canceler
            }
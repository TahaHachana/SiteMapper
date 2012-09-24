namespace SiteMapper

open System
open System.IO
open System.Threading
open System.Windows
open Spidy.Types
open GUI
open Sitemap
open Utilities

module main =

    [<AutoOpenAttribute>]
    module EventHandling =
        let checkUrl() =
            let url = urlTextbox.Text
            let isUrl str = domainNameRegex.IsMatch str
            match url with
                | ""   -> None
                | url' ->
                    let isValidUrl = isUrl url'
                    match isValidUrl with
                        | false -> None
                        | true  -> Some <| canonicalize url'

        let checkUtcValue() =
            let utcValue = utcTextbox.Text
            try
                let dt = DateTime.TryParse(utcValue)
                match dt with
                    | true, x -> Some <| x.ToUniversalTime()
                    | _       -> None
            with
                | _ -> None

        let checkPriority() =
            let priorityValue = priorityTextbox.Text
            try
                let priorityValue'' = float priorityValue
                if   priorityValue'' > 1. then None
                elif priorityValue'' < 0. then None
                else Some priorityValue''
            with _ -> None

        let generateSitemap' url context =
            try generateSitemap url context |> Async.Start
            with _ -> showMsg "Failed to generate the Sitemap."

        let processUrl url utcValue priority context =
            match url with
                | None      -> showMsg "Enter a valid URL."
                | Some url' ->
                    let checkPriority' (priority : float option) =
                        match priorityExact.IsChecked.Value with
                            | true ->
                                match priority with
                                    | None   -> showMsg "Enter a valid priority value (0.0 -> 1.0)."
                                    | Some _ -> generateSitemap' url' context
                            | false -> generateSitemap' url' context
                    match lastModUtc.IsChecked.Value with
                        | true ->
                            match utcValue with
                                | None   -> showMsg "Enter a valid change frequency value."
                                | Some _ -> checkPriority' priority
                        | false -> checkPriority' priority

        let handleClick _ =
            progressTextbox.Clear()
            let url      = checkUrl()
            let utcValue = checkUtcValue()
            let priority = checkPriority()
            let context  = SynchronizationContext.Current
            processUrl url utcValue priority context

        startButton.Click |> Event.add handleClick

        cancelButton.Click |> Event.add (fun _ ->
            cancelButton.IsEnabled <- false
            let cancelingAgent = !agent
            cancelingAgent.Post Cancel
            progressTextbox.AppendText "Cancelling...\n"
            showMsg "Sitemap generation was canceled."
            )

        do Directory.CreateDirectory desktopPath' |> ignore

    module App =
        let app = Application()

        [<STAThreadAttribute>]
        window |> app.Run |> ignore
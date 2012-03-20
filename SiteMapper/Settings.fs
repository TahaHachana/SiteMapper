namespace SiteMapper

open Types
open GUI
open Utilities

module Settings =

    /// Returns the change frequency setting.
    let changeFreq() : ChangeFreq option = 
        let idx = changeFreqCombo.SelectedIndex
        match idx with
            | 0 -> None
            | _ -> Some <| enum idx

    /// Returns the last modification setting.
    let lastMod() =
        match lastModNone.IsChecked.Value with
            | true  -> None
            | false ->
                match lastModWeb.IsChecked.Value with
                    | true  -> Some WebResp 
                    | false -> Some <| Utc utcTextbox.Text

    /// Returns the page priority setting.
    let priority() =
        match priorityNone.IsChecked.Value with
            | true  -> None
            | false ->
                match priorityRand.IsChecked.Value with
                    | true  -> Some Auto
                    | false -> Some <| Value priorityTextbox.Text 

    /// Returns the change frequency, last modification and priority settings.
    let settings() =
        {
            Frequency    = changeFreq()
            LastModified = lastMod()
            Priority     = priority()
        }

    let processSettings (settings : Settings) lm =
        match settings.LastModified with
            | Some x ->
                match x with
                    | WebResp ->
                        match lm with
                            | "" -> None
                            | _  ->
                                let elem = createElement' "lastmod" lm
                                Some elem
                    | Utc utc ->
                        let elem = createElement' "lastmod" utc
                        Some elem
            | None -> None

    let processSettings' (settings : Settings) =
        match settings.Frequency with
            | Some x ->
                let x' = x.ToString()
                let elem = createElement' "changefreq" x'
                Some elem
            | None -> None

    let processSettings'' (settings : Settings) =
        match settings.Priority with
            | Some p ->
                match p with
                | Auto ->
                    let value = rand.Next(1, 9) |> float
                    let value' = value / 10. |> string
                    let elem = createElement' "priority" value'
                    Some elem
                | Value value ->
                    let elem = createElement' "priority" value
                    Some elem
            | None -> None
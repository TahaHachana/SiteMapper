namespace SiteMapper

open System
open Types
open Utilities
open FSharpx

module GUI =

    type MainWindow = XAML<"Window1.xaml">
    
    let mainWindow      = MainWindow()
    let window          = mainWindow.Root
    let cancelButton    = mainWindow.Cancel
    let changeFreqCombo = mainWindow.ChangeFreq
    let lastModNone     = mainWindow.LmNone
    let lastModUtc      = mainWindow.LmUtc
    let lastModWeb      = mainWindow.LmWebResp
    let priorityExact   = mainWindow.PrioExact
    let priorityNone    = mainWindow.PrioNone
    let priorityRand    = mainWindow.PrioRand
    let priorityTextbox = mainWindow.PrioVal
    let progressTextbox = mainWindow.Progress
    let startButton     = mainWindow.Start
    let urlTextbox      = mainWindow.Url
    let utcTextbox      = mainWindow.Utc

    // Set the focus on the URL textbox.
    urlTextbox.Focus() |> ignore

    let showMsg msg = displayMsgBox window msg
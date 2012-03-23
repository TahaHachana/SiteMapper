namespace SiteMapper

open System
open System.Windows
open System.Windows.Controls
open Types
open Utilities
open FSharpx

module GUI =

    type MainWindow = XamlFile<"Window1.xaml">
    
    let mainWindow      = MainWindow()
    let window          = mainWindow.Control
    let cancelButton    = mainWindow.Cancel.Control
    let changeFreqCombo = mainWindow.ChangeFreq.Control
    let lastModNone     = mainWindow.LmNone.Control
    let lastModUtc      = mainWindow.LmUtc.Control
    let lastModWeb      = mainWindow.LmWebResp.Control
    let priorityExact   = mainWindow.PrioExact.Control
    let priorityNone    = mainWindow.PrioNone.Control
    let priorityRand    = mainWindow.PrioRand.Control
    let priorityTextbox = mainWindow.PrioVal.Control 
    let progressTextbox = mainWindow.Progress.Control
    let startButton     = mainWindow.Start.Control
    let urlTextbox      = mainWindow.Url.Control
    let utcTextbox      = mainWindow.Utc.Control

    // Set the focus on the URL textbox.
    urlTextbox.Focus() |> ignore

    let showMsg msg = displayMsgBox window msg
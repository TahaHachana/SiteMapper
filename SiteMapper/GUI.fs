namespace SiteMapper

open System
open System.Windows
open System.Windows.Controls
open Types
open Utilities

module GUI =

    let window =
        let uri = Uri("/SiteMapper;component/Window1.xaml", UriKind.Relative)
        Application.LoadComponent uri :?> Window

    let findControl' = findControl window

    let cancelButton    = findControl' "cancel"     :?> Button
    let changeFreqCombo = findControl' "changeFreq" :?> ComboBox
    let lastModNone     = findControl' "lmNone"     :?> RadioButton
    let lastModUtc      = findControl' "lmUtc"      :?> RadioButton
    let lastModWeb      = findControl' "lmWebResp"  :?> RadioButton
    let priorityExact   = findControl' "prioExact"  :?> RadioButton
    let priorityNone    = findControl' "prioNone"   :?> RadioButton
    let priorityRand    = findControl' "prioRand"   :?> RadioButton
    let priorityTextbox = findControl' "prioVal"    :?> TextBox 
    let progressTextbox = findControl' "progress"   :?> TextBox
    let startButton     = findControl' "start"      :?> Button
    let urlTextbox      = findControl' "url"        :?> TextBox
    let utcTextbox      = findControl' "utc"        :?> TextBox

    // Set the focus on the URL textbox.
    urlTextbox.Focus() |> ignore

    let showMsg msg = displayMsgBox window msg
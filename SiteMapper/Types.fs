namespace SiteMapper

open SEOLib.Types

module Types =

    type ChangeFreq =
        | Always  = 1
        | Hourly  = 2
        | Daily   = 3
        | Weekly  = 4
        | Monthly = 5
        | Yearly  = 6
        | Never   = 7

    type LastMod =
        | WebResp
        | Utc of string

    type Priority =
        | Auto
        | Value of string

    type Settings =
        {
            Frequency    : ChangeFreq option
            LastModified : LastMod    option
            Priority     : Priority   option
        }

    type Message' =
        | Done
        | Progress of string

    type Message'Agent = Agent<Message'>
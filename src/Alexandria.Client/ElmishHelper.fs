//from https://github.com/SAFE-Stack/SAFE-ConfPlanner
module Elmish.Helper


open Elmish

let withAdditionalCommand cmd (model, cmds) =
    model, (Cmd.batch [cmds ; cmd])

let withCommand (cmds: Cmd<'a>) model =
    model, cmds

let withMsg (msg: 'a) model =
    model, Cmd.ofMsg msg

let withCommands (cmds: Cmd<'a> list) model =
    model, Cmd.batch cmds

let withMsgs (msgs: 'a list) model =
    model, (Cmd.batch (msgs |> List.map Cmd.ofMsg ))

let withoutCommands model =
    model, Cmd.none

namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction


module rec Cell =
    let cellIdentifier (taskId: TaskId) (dateId: DateId) =
        [
            taskId |> TaskId.Value |> string
            dateId |> DateId.Value |> FlukeDate.Stringify
        ]

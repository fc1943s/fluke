namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open FsCore.BaseModel


module rec Cell =
    let collection = Collection (nameof Cell)

    let inline cellIdentifier (taskId: TaskId) (dateId: DateId) =
        [
            taskId |> TaskId.Value |> string
            dateId
            |> DateId.ValueOrDefault
            |> FlukeDate.Stringify
        ]

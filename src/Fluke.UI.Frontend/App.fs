namespace Fluke.UI.Frontend

open Fable.React
open Feliz
open Feliz.Recoil
open Browser.Dom
open Fluke.Shared


module App =
    let appMain = React.memo (fun () ->
        Recoil.root [
            root.localStorage (fun hydrater -> hydrater.setAtom Recoil.Atoms.view)

            root.init (fun init ->
                let now = Recoil.OldData.tempState.GetNow ()
                let dayStart = Recoil.OldData.tempState.DayStart

//                let a =
//                    Recoil.FakeBackend.getTree
//                        {| User = None
//                           TreeId = TreeId ""
//                           View = view
//                           Position = None |}

                init.set (Recoil.Atoms.getNow, Recoil.OldData.tempState.GetNow)
                init.set (Recoil.Atoms.now, now)
                init.set (Recoil.Atoms.dayStart, dayStart)
                init.set (Recoil.Atoms.taskOrderList, Recoil.OldData.tempState.TaskOrderList)
                init.set (Recoil.Atoms.informationList, Recoil.OldData.tempState.InformationList)
                init.set (Recoil.Atoms.lastSessions, Recoil.OldData.tempState.LastSessions)
                init.set (Recoil.Atoms.taskStateList, Recoil.OldData.tempState.TaskStateList)

                Recoil.OldData.tempState.InformationCommentsMap
                |> Map.iter (fun information comments ->
                    let recoilInformation = Recoil.Atoms.RecoilInformation.RecoilInformation.Create information
                    init.set (recoilInformation.WrappedInformation, information)
                    init.set (recoilInformation.Comments, comments)
                )

                let dateSequence =
                    [ now.Date ]
                    |> Rendering.getDateSequence (45, 20)

                Recoil.OldData.tempState.TaskStateList
                |> List.iter (fun taskState ->
                    let taskId = Recoil.Atoms.RecoilTask.taskId taskState.Task
                    let task = Recoil.Atoms.RecoilTask.RecoilTask.Create taskId
                    init.set (task.Id, taskId)
                    init.set (task.Comments, taskState.Comments)
                    init.set (task.PriorityValue, taskState.PriorityValue)

                    let cellMap =
                        let (Model.Lane (_, cells)) =
                            Rendering.renderLane dayStart now dateSequence taskState.Task taskState.StatusEntries

                        cells
                        |> List.map (fun (Model.Cell (address, status)) ->
                            address.Date, status
                        )
                        |> Map.ofList

                    dateSequence
                    |> List.iter (fun date ->
                        let cellId = Recoil.Atoms.RecoilCell.cellId taskId date
                        let cell = Recoil.Atoms.RecoilCell.RecoilCell.Create cellId
                        let cellComments = taskState.CellCommentsMap |> Map.tryFind date |> Option.defaultValue []
                        let sessions =
                            taskState.Sessions
                            |> List.filter (fun (Model.TaskSession start) -> Model.isToday Recoil.OldData.tempState.DayStart start date)

                        let status = cellMap |> Map.tryFind date |> Option.defaultValue Model.Missed

                        init.set (cell.Id, cellId)
                        init.set (cell.Task, task)
                        init.set (cell.Date, date)
                        init.set (cell.Status, status)
                        init.set (cell.Comments, cellComments)
                        init.set (cell.Sessions, sessions)
                    )
                )
            )

            root.children [
                ReactBindings.React.createElement
                    (Ext.recoilLogger, (), [])

                Components.MainComponent.render ()
            ]
        ]
    )

    ReactDOM.render (appMain (), document.getElementById "app")


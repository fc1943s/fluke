namespace Fluke.UI.Frontend

open Fable.React
open Feliz
open Feliz.Recoil
open Browser.Dom
open Fluke.Shared


module App =
    let appMain = React.memo (fun () ->
        Recoil.root [
            RecoilRoot.root.init (fun init ->
                init.set (Recoil.Atoms.getNow, Recoil.Temp.tempState.GetNow)
                init.set (Recoil.Atoms.now, Recoil.Temp.tempState.GetNow ())
                init.set (Recoil.Atoms.dayStart, Recoil.Temp.tempState.DayStart)
                init.set (Recoil.Atoms.view, Recoil.Temp.view)
                init.set (Recoil.Atoms.taskOrderList, Recoil.Temp.tempState.TaskOrderList)
                init.set (Recoil.Atoms.informationList, Recoil.Temp.tempState.InformationList)
                init.set (Recoil.Atoms.lastSessions, Recoil.Temp.tempState.LastSessions)
                init.set (Recoil.Atoms.taskStateList, Recoil.Temp.tempState.TaskStateList)

                let dateSequence =
                    [ Recoil.Temp.tempState.GetNow().Date ]
                    |> Rendering.getDateSequence (45, 20)

                Recoil.Temp.tempState.TaskStateList
                |> List.iter (fun taskState ->
                    let taskId = Recoil.Atoms.RecoilTask.taskId taskState.Task
                    let task = Recoil.Atoms.RecoilTask.RecoilTask.Create taskId
                    init.set (task.Id, taskId)
                    init.set (task.Comments, taskState.Comments)
                    init.set (task.PriorityValue, taskState.PriorityValue)

                    dateSequence
                    |> List.iter (fun date ->
                        let cellId = Recoil.Atoms.RecoilCell.cellId taskId date
                        let cell = Recoil.Atoms.RecoilCell.RecoilCell.Create cellId
                        let cellComments = taskState.CellCommentsMap |> Map.tryFind date |> Option.defaultValue []
                        let sessions =
                            taskState.Sessions
                            |> List.filter (fun (Model.TaskSession start) -> Model.isToday Recoil.Temp.tempState.DayStart start date)
//                        let laneMap = laneMapFamily task
//                        let status = laneMap.[date]
                        let status = Model.Missed

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


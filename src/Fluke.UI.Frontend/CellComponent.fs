namespace Fluke.UI.Frontend

open System
open Fable.React
open Fable.React.Props
open Feliz
open Feliz.Recoil
open Fluke.Shared
open Fluke.UI.Frontend

module CellComponent =
    open Model

    module Atoms =

        type RecoilTask =
            { Name: RecoilValue<string, ReadWrite>
              Information: RecoilValue<Information, ReadWrite>
              Scheduling: RecoilValue<TaskScheduling, ReadWrite>
              PendingAfter: RecoilValue<FlukeTime option, ReadWrite>
              MissedAfter: RecoilValue<FlukeTime option, ReadWrite>
              Duration: RecoilValue<int option, ReadWrite> }

            static member NameFamily = atomFamily {
                key "fluke/task/nameFamily"
                def (fun (_information: Information, taskName: string) -> taskName)
            }
            static member InformationFamily = atomFamily {
                key "fluke/task/informationFamily"
                def (fun (information: Information, _taskName: string) -> information)
            }
            static member SchedulingFamily = atomFamily {
                key "fluke/task/schedulingFamily"
                def (fun (_information: Information, _taskName: string) -> Manual WithoutSuggestion)
            }
            static member PendingAfterFamily = atomFamily {
                key "fluke/task/pendingAfterFamily"
                def (fun (_information: Information, _taskName: string) -> None)
            }
            static member MissedAfterFamily = atomFamily {
                key "fluke/task/missedAfterFamily"
                def (fun (_information: Information, _taskName: string) -> None)
            }
            static member DurationFamily = atomFamily {
                key "fluke/task/durationFamily"
                def (fun (_information: Information, _taskName: string) -> None)
            }
            static member Create information taskName =
                { Name = RecoilTask.NameFamily (information, taskName)
                  Information = RecoilTask.InformationFamily (information, taskName)
                  Scheduling = RecoilTask.SchedulingFamily (information, taskName)
                  PendingAfter = RecoilTask.PendingAfterFamily (information, taskName)
                  MissedAfter = RecoilTask.MissedAfterFamily (information, taskName)
                  Duration = RecoilTask.DurationFamily (information, taskName) }

        let taskFamily = atomFamily {
            key "fluke/task"
            def (fun (information: Information, taskName: string) -> RecoilTask.Create information taskName)
        }

//        let date = atom {
//            key "fluke/date"
//            def (flukeDate 0000 Month.January 01)
//        }

//        let cellAddress = atom {
//            key "fluke/cellAddress"
//        }

        type RecoilCell =
            { Address: RecoilValue<CellAddress, ReadWrite>
              Comments: RecoilValue<Comment list, ReadWrite>
              Sessions: RecoilValue<TaskSession list, ReadWrite>
              Status: RecoilValue<CellStatus, ReadWrite>
              Selected: RecoilValue<bool, ReadWrite>
              IsSelected: RecoilValue<bool, ReadWrite>
              IsToday: RecoilValue<bool, ReadWrite> }


    // TODO: take this out of here
    let tooltipPopup = React.memo (fun (input: {| Comments: Comment list |}) ->
        Html.div [
            prop.className Css.tooltipPopup
            prop.children [
                input.Comments
                |> List.map (fun (Comment (_user, comment)) -> comment.Trim ())
                |> List.map ((+) Environment.NewLine)
                |> String.concat (Environment.NewLine + Environment.NewLine)
                |> fun text ->
                    ReactBindings.React.createElement
                        (Ext.reactMarkdown,
                            {| source = text |}, [])
            ]
        ]
    )

    let cell = React.memo (fun (input: {| CellAddress: CellAddress
                                          Comments: Comment list
                                          Sessions: TaskSession list
                                          Status: CellStatus
                                          IsSelected: bool
                                          IsToday: bool |}) ->
        let hasComments = not input.Comments.IsEmpty

        Html.div [
            prop.classes [
                input.Status.CellClass
                if hasComments then
                    Css.tooltipContainer
                if input.IsSelected then
                    Css.cellSelected
                if input.IsToday then
                    Css.cellToday
            ]
            prop.children [
                Html.div [
                    prop.style [
                        match Functions.getCellSeparatorBorderLeft2 input.CellAddress.Date with
                        | Some borderLeft -> borderLeft
                        | None -> ()
                    ]
                    prop.children [
                        match input.Sessions.Length with
        //                | x -> str (string x)
                        | x when x > 0 -> str (string x)
                        | _ -> ()
                    ]
                ]

                if hasComments then
                    tooltipPopup {| Comments = input.Comments |}
            ]
        ]
    )


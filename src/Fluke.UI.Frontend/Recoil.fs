namespace Fluke.UI.Frontend

open Browser.Types
open FSharpPlus
open Fable.Core
open Feliz
open Feliz.Recoil
open Fluke.Shared
open Fluke.UI.Frontend
open Fable.React
open Fable.React.Props
open Fable.DateFunctions
open Fulma
open System
open Suigetsu.UI.Frontend.ElmishBridge
open Suigetsu.UI.Frontend.React
open Suigetsu.Core


module Recoil =
    open Model

    module Temp =
        type TempDataType =
            | TempPrivate
            | TempPublic
            | Test
        let view = View.Calendar
//        let view = View.Groups
//        let view = View.Tasks

        let tempDataType = TempPrivate
//        let tempDataType = Test
//        let tempDataType = TempPublic

        let tempState =
            let testData = TempData.tempData.RenderLaneTests

            let getNow =
                match tempDataType with
                | TempPrivate -> TempData.getNow
                | TempPublic  -> TempData.getNow
                | Test        -> testData.GetNow

            let dayStart =
                match tempDataType with
                | TempPrivate -> PrivateData.PrivateData.dayStart
                | TempPublic  -> TempData.dayStart
                | Test        -> TempData.testDayStart

            {| GetNow = getNow
               DayStart = dayStart |}


    module Atoms =
        let getNow = atom {
            key "fluke/getNow"
            def Temp.tempState.GetNow
        }
        let dayStart = atom {
            key "fluke/dayStart"
            def Temp.tempState.DayStart
        }
        let now = atom {
            key "fluke/now"
            def (flukeDateTime 0000 Month.January 01 00 00)
        }
        let view = atom {
            key "fluke/view"
            def View.Calendar
        }
        let selected = atom {
            key "fluke/selected"
            def (Map.empty : Map<Task, Set<FlukeDate>>)
        }
        let hovered = atom {
            key "fluke/hovered"
            def Hover.None
        }
        let activeSessions = atom {
            key "fluke/activeSessions"
            def ([] : ActiveSession list)
        }
        let dingsFamily = atomFamily {
            key "fluke/dingsFamily"
            def (fun (_date: FlukeDateTime) -> false)
        }
        let dateSequence = selector {
            key "fluke/dateSequence"
            get (fun getter ->
                getter.get now
                |> fun x -> [ x.Date ]
                |> Rendering.getDateSequence (45, 20)
            )
        }

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

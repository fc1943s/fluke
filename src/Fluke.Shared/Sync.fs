namespace Fluke.Shared

open Fluke.Shared.Domain

module Sync =
    open UserInteraction
    open State

    type Api =
        {
            currentUser: Async<User>
            databaseStateList: Username -> FlukeDateTime -> Async<DatabaseState list>
        }

    let serverPort = "33921"

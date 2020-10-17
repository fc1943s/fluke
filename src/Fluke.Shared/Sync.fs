namespace Fluke.Shared

open Fluke.Shared.Domain

module Sync =
    open Model
    open UserInteraction
    open State

    type Api =
        {
            currentUser: Async<User>
            treeStateList: Username -> FlukeDateTime -> Async<TreeState list>
        }

    let serverPort = "33921"

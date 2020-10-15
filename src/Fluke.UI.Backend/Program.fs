namespace Fluke.UI.Backend

open Saturn


module Program =

    [<EntryPoint>]
    let main _ =
        run Server.app
        0

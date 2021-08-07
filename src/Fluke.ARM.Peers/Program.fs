namespace Fluke.ARM.GunPeers

open Farmer


module Program =
    [<EntryPoint>]
    let main _ =
        printf "Generating ARM template..."
        Peers.deployment |> Writer.quickWrite "output"
        printfn "all done! Template written to output.json"

        Peers.deployment
        |> Deploy.execute (nameof Peers) []
        |> printfn "%A"

        0

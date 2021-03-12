namespace Fluke.ARM.GunPeers

open Farmer


module Program =
    [<EntryPoint>]
    let main _ =
        //printf "Generating ARM template..."
        //Vm1.deployment |> Writer.quickWrite "output"
        //printfn "all done! Template written to output.json"

        GunPeers.deployment
        |> Deploy.execute (nameof GunPeers) []
        |> printfn "%A"

        0

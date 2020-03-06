namespace Fluke.UI.Frontend

open Fable.Core

module ExtTypes =
    
    type IFlatted =
        abstract stringify: obj -> string

    type IMoment =
        abstract diff: IMoment -> string -> bool -> float
        


        

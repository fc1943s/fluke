namespace Fluke.UI.Frontend

module ExtTypes =
    
    type IFlatted =
        abstract stringify: obj -> string

    type IMoment =
        abstract diff: IMoment -> string -> bool -> float
        


        

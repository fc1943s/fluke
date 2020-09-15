namespace Fluke.UI.Frontend.Components

open Fulma
open Fulma.Extensions.Wikiki
open Feliz
open Feliz.UseListener


module PageLoaderComponent =
    let render =
        React.memo (fun () ->
            PageLoader.pageLoader [
                                      PageLoader.Color IsDark
                                      PageLoader.IsActive true
                                  ] [])

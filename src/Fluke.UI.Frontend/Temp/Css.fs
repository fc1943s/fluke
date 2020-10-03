namespace Fluke.UI.Frontend

open Zanaptak.TypedCssClasses


[<AutoOpen>]
module Css =
    type Css = CssClasses<"../../src/Fluke.UI.Frontend/public/index.scss", Naming.CamelCase>


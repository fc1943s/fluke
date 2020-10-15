namespace Fluke.UI.Backend

open Saturn
open Giraffe.ResponseWriters


module Server =

    module Router =
        let api =
            pipeline {
                plug acceptJson
                set_header "x-pipeline-type" "Api"
            }

        let apiRouter =
            router {
                not_found_handler (text "Api 404")
                pipe_through api
            }

        let appRouter = router { forward "" apiRouter }

    let app =
        application {
            pipe_through
                (pipeline {
                    plug head
                    plug requestId
                 })

            error_handler (fun ex _ -> pipeline { text (ex.ToString ()) })
            use_router Router.appRouter
            url "http://0.0.0.0:8085/"
            memory_cache
            use_gzip
            use_config (fun _ -> {| ConfigKey = "ConfigValue" |})
        }

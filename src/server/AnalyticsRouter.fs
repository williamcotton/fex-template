module AnalyticsRouter

open Fable.Core
open Express

[<Import("default", "router")>]
let router : unit -> ExpressApp = jsNative

let analyticsRouter = router()

analyticsRouter.get("/", fun req res next ->
    res.pageview({| title = "Front Page" |})
)


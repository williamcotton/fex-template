module AnalyticsRouter

open Fable.Core
open Fable.Core.JsInterop
open Express
open Global

[<Import("default", "router")>]
let router : unit -> ExpressApp = jsNative

let analyticsRouter = router()

analyticsRouter.get("/", fun req res next ->
    let greeting = req?dataQuery?data?greeting;
    res.pageview({| title = "Front Page"; payload = greeting |})
)

analyticsRouter.post("/setName", fun req res next ->
    res.event({| title = "Set Name Event"; payload = {| name = req.body?inputName |} |})
)

analyticsRouter.get("/github_status", fun req res next ->
    let url = req?dataQuery?query
    res.pageview({| title = "Github Status"; payload = {| url = url |} |})
)

let analyticsPageviev obj = 
    consoleLog ("Pageview", obj?title, obj?payload)

let analyticsEvent obj =
    consoleLog ("Event", obj?title, obj?payload)

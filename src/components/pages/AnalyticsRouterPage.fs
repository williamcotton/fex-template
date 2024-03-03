module AnalyticsRouterPage

open Express
open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Global
open AppLayout
open CodeBlock

[<ReactComponent>]
let AnalyticsRouterPage() =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 "Analytics Router"

        Html.p "One of the many benefits of using a standardized route handler is the ability to create a secondary router that can listen for the same events but in an isolated context."

        Html.p "This allows us to capture user interactions and page views on the server and the client while keeping the analytics code in a single location and completely separate from the concerns of our business logic, route handlers and user interface components."

        CodeBlock {| lang = "fsharp"; code =
"""[<Import("default", "router")>]
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
"""     |}

        Html.p "Our server-side middleware wires up the router and the analytics event handlers."

        CodeBlock {| lang = "fsharp"; code =
"""useMiddleware(analyticstMiddleware({|
    analyticsRouter = analyticsRouter; 
    app = app; 
    analyticsPageview = analyticsPageviev; 
    analyticsEvent = analyticsEvent 
|}))
"""     |}

        Html.p "The same router is used for our client-side middleware."

        CodeBlock {| lang = "fsharp"; code =
"""useMiddleware(analyticstMiddleware({|
    analyticsRouter = analyticsRouter
|}))""" |}


        req.Link {| href = "/caveats"; children = "Next: Caveats" |}

    ]
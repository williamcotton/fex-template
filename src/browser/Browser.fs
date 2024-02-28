module Browser

open Feliz
open Fable.Core
open App
open Express
open Components
open AnalyticsRouter
open Global

[<Import("default", "browser-express")>]
let express : unit -> ExpressApp = jsNative

[<Import("default", "./middleware/react-renderer.js")>]
let reactRendererMiddleware : {| app: ExpressApp; appLayout: {| content: ReactElement; req: ExpressReq |} -> ReactElement |} -> unit = jsNative

[<Import("default", "./middleware/express-link.js")>]
let expressLinkMiddleware : unit -> unit = jsNative

[<Import("default", "./middleware/graphql-client.js")>]
let graphqlClientMiddleware : {| route : string |} -> unit = jsNative

[<Import("default", "./middleware/fetch-client.js")>]
let fetchClientMiddleware: obj -> unit = jsNative

[<Import("default", "./middleware/analytics.js")>]
let analyticstMiddleware: {| analyticsRouter : obj; fetch : obj |} -> unit = jsNative


[<Emit("app.use($0)")>]
let useMiddleware middleware: unit = jsNative

let app = express()
useMiddleware(expressLinkMiddleware())
useMiddleware(reactRendererMiddleware({| app = app; appLayout = AppLayout |}))
useMiddleware(graphqlClientMiddleware({| route = "/graphql" |}))
useMiddleware(analyticstMiddleware({| analyticsRouter = analyticsRouter; fetch = fetch |}))
useMiddleware(fetchClientMiddleware())
useMiddleware(analyticstMiddleware({| analyticsRouter = analyticsRouter; fetch = fetch |}))


universalApp app

app.listen(3000, fun _ ->
    printfn "Listening on port 3000"
)

module Server

open Feliz
open Fable.Core
open App
open Express
open Fable.Core.JsInterop
open GraphQLSchema
open Components

[<Import("default", "express")>]
let express : unit -> ExpressApp = jsNative

[<Import("default", "csurf")>]
let csurf : obj -> unit = jsNative

[<Import("default", "cookie-session")>]
let cookieSession : {| name: string; sameSite: string; secret: string |} -> unit = jsNative

[<Import("createHandler", "graphql-http/lib/use/express")>]
let createHandler : {| schema: obj; rootValue: obj; graphiql: bool; context : obj -> obj -> obj |} -> unit = jsNative

[<Import("default", "./graphql-schema-builder.js")>]
let graphqlSchemaBuilder : {| schemaString: string |} -> obj = jsNative

[<Import("default", "./middleware/express-link.js")>]
let expressLinkMiddleware : {| defaultTitle: string |} -> unit = jsNative

[<Import("default", "./middleware/react-renderer.js")>]
let reactRendererMiddleware : {| appLayout: {| content: ReactElement; req: ExpressReq |} -> ReactElement |} -> unit = jsNative

[<Import("default", "./middleware/graphql-client.js")>]
let graphqlClientMiddleware : {| schema : obj; rootValue : obj |} -> unit = jsNative

[<Import("default", "./middleware/fetch-client.js")>]
let fetchClientMiddleware: obj -> unit = jsNative

[<Import("default", "body-parser")>]
let bodyParser : {| urlencoded: obj -> obj; json: obj -> obj |} = jsNative

[<Emit("app.use($0)")>]
let useMiddleware middleware: unit = jsNative

[<Emit("app.use($0, $1)")>]
let useMiddlewareRoute route middleware: unit = jsNative

[<Emit("express.static($0)")>]
let expressStatic (path : string): unit = jsNative

[<Emit("process.env[$0]")>]
let env (key: string) : string = jsNative
let defaultTitle = env "DEFAULT_TITLE"
let sessionSecret = env "SESSION_SECRET"
let port = env "PORT"

let schemaObject = graphqlSchemaBuilder {| schemaString = schemaString |}
let schema = schemaObject :?> {| schema: obj; rootValue: obj |}
let rootValue : obj = rootValueInitializer

let customContextFunction ctx args =
    let req =
        if ctx?raw then
            ctx?raw?res?req
        else
            ctx
    req

let app = express()
useMiddleware(expressStatic("build"))
useMiddleware(cookieSession({| name = "session"; sameSite = "lax"; secret = sessionSecret |}))
useMiddleware(bodyParser.urlencoded({| extended = false |}))
useMiddleware(bodyParser.json())
useMiddleware(csurf())
useMiddlewareRoute "/graphql" (createHandler({| schema = schema.schema; rootValue = rootValue; graphiql = true; context = customContextFunction |}))
useMiddleware(graphqlClientMiddleware({| schema = schema.schema; rootValue = rootValue |}));
useMiddleware(fetchClientMiddleware())
useMiddleware(expressLinkMiddleware({| defaultTitle = defaultTitle |}))
useMiddleware(reactRendererMiddleware({| appLayout = AppLayout |}))

universalApp app

app.listen(int port, fun _ ->
    printfn "Listening on port %s" port
)
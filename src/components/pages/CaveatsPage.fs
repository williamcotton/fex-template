module CaveatsPage

open Feliz
open Fable.Core.JsInterop
open Global
open AppLayout
open CodeBlock

[<ReactComponent>]
let CaveatsPage() =
    let req = React.useContext requestContext
    React.fragment [
        Html.h3 "Caveats"
        Html.p "At the core of this architecture is the HTTP request. In the context of the server this request is intiated by a socket listening on a TCP port, converted into data of type ExpressReq, and passed to route handlers. In the context of the browser this request is initiated by onClick and onSubmit DOM events, turned into to type ExpressReq, and passed on to the same route handlers."
        Html.p "The only difference is in how the request is formed and how the response is handled. The rest forms the core of the user's interactions with a web browser."
        Html.p "In a Fex application the core of the applications is written in F#. Like any language that compiles to a host language it is a bit rought around the edges. But in general it is easy to isolate the parts of the application that need to be written in JavaScript and the parts that need to be written in F#."
        Html.p "And the ugly parts that glue the pieces together:"
        CodeBlock {| lang = "fsharp"; code =
"""module Server

open Feliz
open Fable.Core
open App
open Express
open Fable.Core.JsInterop
open GraphQLSchema
open Components

[<Emit("process.env[$0]")>]
let env (key : string) : string = jsNative

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
)"""    |}

        Html.p "There's a real cost when it comes to compiling to a host language, especially when there are significant formal differences between the host language and the language being compiled to it."

        Html.p "The question is whether or not the cost is worth it. This will be dependent on the specific use case and the specific requirements of the application."
    ]
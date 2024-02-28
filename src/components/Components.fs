module Components

open Feliz
open GraphQLSchema
open Fable.Core.JsInterop
open Fable.Core
open Express
open Global

[<Import("default", "highlight.js")>]
let highlightjs : {| highlight: string -> obj -> {| value : string |} |} = jsNative

let requestContext = React.createContext(name="Request")

type RequestBody =
    { inputName: string
      inputEmail: string }

[<ReactComponent>]
let AppLayout (props: {| content: ReactElement; req: ExpressReq |}) =
    let Link = props.req.Link
    React.contextProvider(requestContext, props.req, React.fragment [
        Html.div [
            prop.className "sitewrapper"
            prop.children [
                Html.header [
                    Html.h1 [
                        Link {| href = "/"; children = "fex-template" |}
                    ]
                    Html.nav [
                        Html.ul [
                            Html.li [
                                Link {| href = "/about"; children = "About" |}
                            ]
                            Html.li [
                                Link {| href = "/github_status"; children = "GitHub Status" |}
                            ]
                        ]
                    ]
                ]
                Html.div [
                    prop.className "content"
                    prop.children props.content
                ]
                Html.footer [
                    prop.children [
                        Html.p [
                            prop.text "Fex Template"
                        ]
                    ]
                ]
            ]
        ]
    ])

[<ReactComponent>]
let CodeBlock (props: {| code: string; lang: string |}) =
    let code = highlightjs.highlight props.code {| language = props.lang |}
    React.fragment [ 
        Html.pre [ 
            Html.code [ 
                prop.dangerouslySetInnerHTML code.value
            ]
        ]
    ]

[<ReactComponent>]
let Counter() =
    let (count, setCount) = React.useState(0)
    Html.div [
        prop.style [ style.marginBottom 20 ]
        prop.children [ 
            Html.button [
                prop.style [ style.marginRight 5 ]
                prop.onClick (fun _ -> setCount(count + 1))
                prop.text "Increment"
            ]

            Html.button [
                prop.style [ style.marginLeft 5; style.marginRight 15 ]
                prop.onClick (fun _ -> setCount(count - 1))
                prop.text "Decrement"
            ]

            Html.span count
        ]
    ]


[<ReactComponent>]
let FrontPage(props: {| greeting : Greeting |}) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 props.greeting.heading

        Html.p props.greeting.content

        Html.h3 "Universal Route Handlers"

        Html.p "Fex enables efficient server-side rendering of static HTML through simple route handlers, data queries, and React components. This initial rendering delivers fast load times."
        
        Html.p "Meanwhile, the same route handlers and components power client-side interactions without full page reloads allowing for all of the benefits of interactive single-page applications."

        Html.p "Here's an example of a universal route handler:"

        CodeBlock {| 
            lang = "fsharp"; code =
"""app.get("/", fun req res next ->
    promise {
        let! response = 
            req 
            |> gql "query { greeting { heading content } }" {||}
            
        match response with
        | Ok response -> 
            let greeting : Greeting = response?greeting
            ShowGreeting ({| greeting = greeting |})
            |> res.renderComponent
        | Error message -> next()

    } |> ignore
)
"""     |}

        Html.p "What underlies this functionality is that `res.renderComponent` and `req.gql` are attached to the request and response objects by parallel middleware. These middleware have separate implementations for the server and the browser but offer the same typed interface."

        Html.h3 "Feliz React Components"

        Html.p "React components can be crafted in F# using Feliz, allowing for server-rendered HTML and client-side interactivity. Here's an example of a counter component, which should look very familiar to a React developer:"

        Counter()

        CodeBlock {| 
            lang = "fsharp"; code =
"""[<ReactComponent>]
let Counter() =
    let (count, setCount) = React.useState(0)
    Html.div [
        Html.button [
            prop.style [ style.marginRight 5 ]
            prop.onClick (fun _ -> setCount(count + 1))
            prop.text "Increment"
        ]

        Html.button [
            prop.style [ style.marginLeft 5 ]
            prop.onClick (fun _ -> setCount(count - 1))
            prop.text "Decrement"
        ]

        Html.h1 count
    ]
"""     |}

        Html.h3 "Link Elements"

        Html.p "Navigation is managed by React components, facilitating client-side routing that operates as a single-page application with dynamic data fetching. At the same time the Fex architectural pattern ensures the server-side rendering of the components."

        Html.p "Example navigation bar component:"

        CodeBlock {| 
            lang = "javascript"; code =
"""[<ReactComponent>]
let NavigationBar(props: {| greeting : Greeting |}) =
    let req = React.useContext requestContext
    Html.nav [
        req.Link {| href = "/"; children = "Home" |}
        req.Link {| href = "/about"; children = "About" |}
    ]
"""
        |}

        req.Link {| href = "/form-elements"; children = "Next: Form Elements" |}
    ]

[<ReactComponent>]
let FormElementPage(props: {| inputName : string option |}) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 "Form Elements"

        Html.p "Forms integrate seamlessly, functioning on both client and server sides, enabling interactions without page reloads. The server also handles these form submissions, ensuring functionality even without JavaScript."

        Html.p "Example form component and handler:"

        CodeBlock {| lang = "fsharp"; code = 
"""[<ReactComponent>]
let FormElementPage(props: {| inputName : string option |}) =
    let req = React.useContext requestContext
    React.fragment [
        match props.inputName with
        | Some name -> (Html.p ("Hello, " + name + "!"))
        | None -> Html.p "Please enter your name:"

        req.Form {| action = "/setName"; method = "post"; children = [
            Html.input [ prop.type' "text"; prop.name "inputName"; prop.placeholder "Name" ]
            Html.input [ prop.type' "submit"; prop.value "Submit" ]
        ] |}
    ]
"""     |}

        CodeBlock {| lang = "fsharp"; code =
"""app.post("/setName", fun req res next ->
    promise {     
        let inputName = req.body?inputName
            let! response =
                req |> gql "mutation ($inputName: String) 
                           { setName(inputName: $inputName) { success } }" 
                           {| inputName = inputName |} {||}
        res.redirect("back")
    } |> ignore
)""" 
        |}

        Html.p "This is an example of the above form element components in action and shows how requests are handled without a page reload while still being structured around the foundational element of interactive web applications, the form post."
        
        match props.inputName with
        | Some name -> (Html.p ("Hello, " + name + "!"))
        | None -> Html.p "Please enter your name:"

        req.Form {| action = "/setName"; method = "post"; children = [
            Html.input [ prop.type' "text"; prop.key "inputName"; prop.name "inputName"; prop.placeholder "Name" ]
            Html.input [ prop.type' "submit"; prop.key "submit"; prop.value "Submit" ]
        ] |}

        Html.p "If you disable JavaScript in your browser, form submissions will continue to work and update the state of the page, albeit with the obvious need for a page reload."

        Html.p "In some instances it might make sense to use a hook like useReducer to manage the state of some part of the page, but in most cases a simple stateless component will suffice, following the general approach of working with a functional programming language like F#."

        req.Link {| href = "/middleware"; children = "Next: Parallel Middleware" |}
    ]

[<ReactComponent>]
let MiddlewarePage() =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 "Parallel Middleware"

        Html.p "Middleware is a powerful tool for managing requests and responses. It can be used to handle errors, parse requests, and manage sessions. Middleware can also be used to manage the flow of requests and responses, allowing for a wide range of customizations."

        Html.p "With the Fex architectural pattern, parallel middleware is used to allow for environment agnostic route handlers and components."

        Html.p "For example, our GraphQL server-side middleware directly makes a call to the GraphQL service."

        CodeBlock {| lang = "javascript"; code =
"""export default ({ schema, rootValue }) => (req, res, next) => {
    req.gql = async (query, variables) => {

      // ... some code here to handle caching and other options

      const response = await graphql(schema, query, rootValue, req, variables);
        
      const { data, errors } = response;

      if (errors) {
        const statusCode = errors[0].message === "NotFound" ? 404 : 500;
        throw new HTTPError(statusCode, errors[0].message);
      }

      req.dataQuery = {
        data,
        errors,
        query,
        variables,
      };

      return data;
    };
    next();
  };
""" 
        |}

        Html.p "Whereas the client-side middleware fetches from the server, which in turn makes a call to the GraphQL service."

        CodeBlock {| lang = "javascript"; code =
"""
export default ({ route }) => (req, res, next) => {
  req.gql = async (query, variables, options = {}) => {
    
    // ... some code here to handle caching and other options

    const fetchResponse = async () => {
      const response = await fetch(route, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Accept: 'application/json',
          'X-CSRF-Token': req.csrf,
        },
        body: JSON.stringify({ query, variables }),
      });
      return response.json()
    };

    const response = await fetchResponse()

    // ... some code here to handle caching and other options

    req.dataQuery = {
      data,
      errors,
      query,
      variables,
    };

    return data;
  };
""" 
        |}

        Html.p "Together this allows for a call to `req.gql` to offer the same interface for both the server-side and client-side middleware. In our example this allows for a consistent approach to handling GraphQL queries and mutations."

        req.Link {| href = "/form-validation"; children = "Next: Form Validation" |}
    ]

[<ReactComponent>]
let FormValidationPage(props: {| errors : Map<string, string list>; requestBody : RequestBody |})  =
    let req = React.useContext requestContext
    let errors = props.errors // Assuming errors is a Map with the field name as key and error message as value
    let requestBody = props.requestBody // Cast requestBody to the RequestBody type
    let inputName = requestBody.inputName
    let inputEmail = requestBody.inputEmail

    React.fragment [
        Html.h2 "Form Validation"

        Html.p "Form validation is a critical part of any web application. It ensures that the data submitted by the user is accurate and secure. Fex provides a simple and flexible approach to form validation based on the built-in Result types in F#."

        req.Form {| action = "/form-validation"; method = "post"; children = [
            Html.div [
                prop.className "form-group"
                prop.children [
                    Html.label [ prop.htmlFor "inputName"; prop.text "Name" ]
                    textInputFieldWithError "inputName" "Name" inputName errors
                ]
            ]
            Html.div [
                prop.className "form-group"
                prop.children [
                    Html.label [ prop.htmlFor "inputEmail"; prop.text "Email" ]
                    textInputFieldWithError "inputEmail" "Email" inputEmail errors
                ]
            ]
            Html.input [ prop.type' "submit"; prop.key "submit"; prop.value "Submit" ]
        ] |}

        let isFormSuccessfullySubmitted =
            not (isObjEmpty requestBody) && Map.isEmpty errors

        match isFormSuccessfullySubmitted with
        | true -> Html.p "Form submitted successfully"
        | _ -> null

        Html.p "F# is an excellent choice when it comes to validating form data. It's a functional-first language that provides a powerful type system and a rich set of libraries for working with data. Below is an example using the Validus NuGet package."

        CodeBlock {| lang = "fsharp"; code =
"""app.post("/form-validation", fun req res next ->
    let requestBody = req.body :?> RequestBody
    let inputName = requestBody.inputName
    let inputEmail = requestBody.inputEmail
    
    let nameValidator fieldName =
        let msg = fun _ -> $"{fieldName} must be between 3 and 64 characters"
        Check.WithMessage.String.betweenLen 3 64 msg

    let validatedInput =  
        validate {
            let! inputName = nameValidator "Name" "inputName" inputName
            and! inputEmail = nameValidator "Email address" "inputEmail" inputEmail
            return {
                inputName = inputName
                inputEmail = inputEmail
            }
        }

    let errors = 
        match validatedInput with
        | Ok validInput -> Map.empty
        | Error e -> e |> ValidationErrors.toMap

    FormValidationPage ({| errors = errors; requestBody = requestBody |})
    |> res.renderComponent
)"""    |}

        Html.p "Again, since we're using the Fex architectural pattern we can turn off JavaScript in our web browsers and have the exact same experience."

        Html.p "Take a look in w3m, links, or lynx and you'll see that the form still works and the page still updates, allowing for the easy creation of a TUI web application!"
    ]

[<ReactComponent>]
let WeatherPage(props: {| forecast : obj array|}) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 "Weather"
        props.forecast
        |> Array.map(fun (period: obj) ->
            let detailedForecast : string = period?detailedForecast
            Html.p detailedForecast
        )
        |> React.fragment
    ]

[<ReactComponent>]
let GithubStatusPage(props: {| status : obj array |}) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 "GitHub Status"
        props.status
        |> Array.map(fun (status: obj) ->
            let statusMessage : string = status?status
            let name : string = status?name
            let description : string = status?description
            Html.div [
                Html.h3 name
                Html.p statusMessage
                Html.p description
            ]
        )
        |> React.fragment
    ]

[<ReactComponent>]
let RequestContextPage() =
    let req = React.useContext requestContext
    React.fragment [
        Html.h2 "Request Context"
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
[<ReactComponent>]
let RequestResponseCyclePage() =
    let req = React.useContext requestContext
    React.fragment [
        let (requestStep, setRequestStep) = React.useState(0)
        let steps = 9 // Steps including server and client middleware

        let cycleRequestStep = fun _ -> setRequestStep((requestStep + 1) % steps)

        // Updated function signatures to include a background color parameter
        let createParallelStep (title: string, stepIndex: int, offsetX: int, offsetY: int, backgroundColor: string) =
            React.fragment [
                Svg.rect [
                    svg.fill (match stepIndex with 
                                    | step when step = requestStep -> color.green
                                    | _ -> backgroundColor) // Use backgroundColor parameter
                    svg.x(offsetX)
                    svg.y(offsetY)
                    svg.width(160)
                    svg.height(30)
                ]
                Svg.text [
                    svg.x(offsetX + 5)
                    svg.y(offsetY + 20)
                    svg.fontSize 18
                    svg.fill color.white
                    svg.text title
                ]
            ]

        let createCombinedStep ( title: string, stepIndex: int, offsetX: int, offsetY: int, backgroundColor: string) =
            React.fragment [
                Svg.rect [
                    svg.fill (match stepIndex with
                                    | step when step = requestStep -> color.green
                                    | _ -> backgroundColor) // Use backgroundColor parameter
                    svg.x(offsetX)
                    svg.y(offsetY)
                    svg.width(360)
                    svg.height(30)
                ]
                Svg.text [
                    svg.x(offsetX + 5)
                    svg.y(offsetY + 20)
                    svg.fontSize 18
                    svg.fill color.white
                    svg.text title
                ]
            ]

        // Button to cycle through the request/response steps
        Html.p [
            Html.button [
                prop.onClick (fun _ -> cycleRequestStep())
                prop.text "Cycle Through Request and Response"
            ]
        ]

        // SVG container for the visual representation
        Svg.svg [
            svg.width (694) // Width to accommodate the elements
            svg.height (300) // Adjusted height to fit the additional rows
            svg.children [ 
                // Dynamically create SVG blocks for each step with specific background colors
                createParallelStep("HTTP Request", 0, 0, 0, "darkred")
                createParallelStep ("Click", 0, 400, 0, "darkblue")

                createParallelStep ("Event Listener", 1, 400, 30, "darkblue")
                createParallelStep( "Socket Listener", 1, 0, 30, "darkred")

                createParallelStep("Express", 2, 0, 60, "darkred")
                createParallelStep ( "Browser-Express", 2, 400, 60, "darkblue")

                createParallelStep("Server Middleware", 3, 0, 90, "darkred")
                createParallelStep("Client Middleware", 3, 400, 90, "darkblue")

                createCombinedStep("Universal Request Handlers", 4, 100, 120, "purple")
                createCombinedStep("Universal Data Fetchers", 5, 100, 150, "purple")
                createCombinedStep("Universal React Components", 6, 100, 180, "purple")

                createParallelStep("Rendered HTML", 7, 0, 210, "darkred")
                createParallelStep("Diffed DOM", 7, 400, 210, "darkblue")    

                createParallelStep("HTTP Response", 8, 0, 240, "darkred")
                createParallelStep("DOM Update", 8, 400, 240, "darkblue")
            ]
        ]
    ]


module Components

open Feliz
open GraphQLSchema
open Fable.Core
open Express

[<Import("default", "highlight.js")>]
let highlightjs : {| highlight: string -> obj -> {| value : string |} |} = jsNative

let requestContext = React.createContext(name="Request")

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

        Html.p "Fex enables efficient server-side rendering of static HTML through simple route handlers, GraphQL queries, and React components. This initial rendering delivers fast load times. The same code also powers client-side interactions without full page reloads, akin to single-page applications, but with route handlers serving as the controller."

        Html.p "Example of a universal route handler:"

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

        Html.h3 "Feliz React Components"

        Html.p "Components are crafted in F# using Feliz, allowing for server-rendered HTML and client-side interactivity. Here's an example of a counter component:"

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

        Html.p "Navigation is managed by React components, facilitating client-side routing that mimics a single-page application while ensuring server-side handling of initial requests for SEO and interactivity before JavaScript loads."

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

        Html.p "Forms integrate seamlessly, functioning on both client and server sides, enabling interactions without page reloads. The server also handles form submissions, ensuring functionality even without JavaScript."

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

        Html.p [req.Form {| action = "/setName"; method = "post"; children = [
            Html.input [ prop.type' "text"; prop.name "inputName"; prop.placeholder "Name" ]
            Html.input [ prop.type' "submit"; prop.value "Submit" ]
        ] |} ]

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

        Html.p "With the Fex architectural pattern parallel middleware is used to allow for higher-order route handlers and components."

        Html.p "For example, our GraphQL server-side middleware makes a direct call to the GraphQL schema and root value."

        CodeBlock {| lang = "javascript"; code =
"""export default ({ schema, rootValue }) =>
  (req, res, next) => {
    req.gql = async (query, variables) => {
      const isMutation = /^mutation/.test(query);
      const key = cacheKey(query, variables);
      const response = await graphql(schema, query, rootValue, req, variables);
      if (!isMutation) res.cacheQuery(key, response);
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

        Html.p "Whereas the client-side middleware makes a call to the server-side middleware, which in turn makes a call to the GraphQL server."

        CodeBlock {| lang = "javascript"; code =
"""
export default ({ route }) => (req, res, next) => {
  req.gql = async (query, variables, options = {}) => {
    const cache = 'cache' in options ? options.cache : true;
    const refresh = 'refresh' in options ? options.refresh : false;
    const isMutation = /^mutation/.test(query);
    const key = cacheKey(query, variables);

    // if it's the initial page request or we're caching the query after further requests, check the server side query cache and the local query cache
    const cachedResponse =
      initialRequest || (cache && !initialRequest)
        ? Object.assign(req.queryCache, localQueryCache)[key]
        : false;

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

    // if we don't have a cached response, fetch from the server
    const response = cachedResponse && !refresh ? cachedResponse : (await fetchResponse());

    // if we're caching and it's not a mutation and not the intial request, then store the response in the local query cache
    if (cache && !isMutation && !initialRequest) {
      localQueryCache[key] = response;
    }

    const { data, errors } = response;

    if (errors) {
      const statusCode = errors[0].message === 'NotFound' ? 404 : 500;
      throw new HTTPError(statusCode, errors[0].message);
    }

    // store the data, errors, query and variables on the request for other interested middleware, eg, event tracking for analytics
    req.dataQuery = {
      data,
      errors,
      query,
      variables,
    };

    initialRequest = false;

    return data;
  };
""" 
        |}

        Html.p "Together this allows for a call to req.gql to be agnostic to the server-side and client-side middleware, allowing for a consistent approach to handling GraphQL queries and mutations."

        req.Link {| href = "/about"; children = "Back: About" |}
    ]
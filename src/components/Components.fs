module Components

open Feliz
open GraphQLSchema
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


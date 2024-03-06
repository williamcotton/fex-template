module SinglePageApplicationAdvancedDemoPage

open Express
open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Global
open AppLayout
open Validus
open SPACodeBlocks

[<Import("default", "router")>]
let router : unit -> ExpressApp = jsNative
let SinglePageApplicationAdvancedDemoRouter = router()
let private spa = SinglePageApplicationAdvancedDemoRouter

type SinglePageApplicationAdvancedDemoPageProps = {
    color : string
    colorError : string
    name : string
    nameError : string
    inputName : string
    inputEmail : string
    inputNameErrors : string array
    inputEmailErrors : string array
}

type InputNameAndEmail = {
    inputName : string
    inputEmail : string
    inputNameErrors : string array
    inputEmailErrors : string array
}

type Color = {
    color : string
    colorError : string
}

type Name = {
    name : string
    nameError : string
}

[<ReactComponent>]
let nameSection (props: Name) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h4 [ prop.text "Name"; prop.id "setName" ]

        Html.p "In this section we're using a form to update the name. The form is submitted to the universal route handler and ultimately to the the server using the GraphQL mutation. This is followed by an update to either the DOM or the HTML response, depending on the context. The anchor ID is used to scroll to the correct section of the page after the page reloads for when JavaScript is disabled."

        let name = props.name
        let nameError = props.nameError

        match nameError with
        | "invalid-name" -> Html.p "Please enter your name:"
        | _ -> Html.p (sprintf "Hello, %s!" name)

        req.Form {| id = "setName"; baseAction = "/set-name#setName"; method = "post"; children = [
            Html.input [ prop.type' "text"; prop.key "name"; prop.name "name"; prop.placeholder "Name" ]
            Html.input [ prop.type' "submit"; prop.key "submit"; prop.value "Change Name" ]
        ] |}

        nameComponentCodeBlock
        nameHandlerCodeBlock
    ]

[<ReactComponent>]
let colorSection (props: Color) = 
    let req = React.useContext requestContext
    React.fragment [
        Html.h4 [ prop.text "Color"; prop.id "setColor" ]

        Html.p "This section follows the same basic pattern but uses FormButton components instead of a Form component. The interaction is still founded on the Form post but with a convenient button component that simplifies the process. Again, notice the use of the query parameters in the URL to drive the error handling. This is a key part of the artchitecture that allows for multiple sections of the page to maintain their state independently."

        let color = props.color
        let colorError = props.colorError

        req.FormButton {| baseAction = "/set-color#setColor"; name = "color"; value = "red"; buttonText = "Red"|}
        req.FormButton {| baseAction = "/set-color#setColor"; name = "color"; value = "green"; buttonText = "Green"|}
        req.FormButton {| baseAction = "/set-color#setColor"; name = "color"; value = "blue"; buttonText = "Blue"|}
        req.FormButton {| baseAction = "/set-color#setColor"; name = "color"; value = "error"; buttonText = "Error"|}

        match colorError with
        | "invalid-color" -> Html.p "Invalid color. Please select red, green, or blue."
        | _ -> null

        Html.div [
            prop.style [ style.color color]
            prop.children [ Html.p "Click the buttons to change the color of this text." ]
        ]

        colorComponentCodeBlock
        colorHandlerCodeBlock
    ]

[<ReactComponent>]
let nameAndEmailSection (props: InputNameAndEmail) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h4 [ prop.text "Name and Email"; prop.id "setNameAndEmail" ]

        Html.p "This section demonstrates how to handle multiple form inputs with multiple validation requirements. The form is again submitted to the universal route handler. For demonstratinon purposes this route handler does nothing beside handle and report validation errors."

        let inputName = props.inputName
        let inputNameErrors = props.inputNameErrors
        let inputEmail = props.inputEmail
        let inputEmailErrors = props.inputEmailErrors

        req.Form {| baseAction = "/set-name-and-email#setNameAndEmail"; method = "post"; children = [
            Html.div [
                prop.className "form-group"
                prop.children [
                    Html.label [ prop.htmlFor "inputName"; prop.text "Name" ]
                    textInputFieldWithStringListError "inputName" "Name" inputName (Some inputNameErrors)
                ]
            ]

            Html.div [
                prop.className "form-group"
                prop.children [
                    Html.label [ prop.htmlFor "inputEmail"; prop.text "Email" ]
                    textInputFieldWithStringListError "inputEmail" "Email" inputEmail (Some inputEmailErrors)
                ]
            ]

            Html.input [ prop.type' "submit"; prop.key "submit"; prop.value "Submit" ]
        ] |}

        nameAndEmailComponentCodeBlock
        nameAndEmailHandlerCodeBlock
    ]

[<ReactComponent>]
let SinglePageApplicationAdvancedDemoPage(props: SinglePageApplicationAdvancedDemoPageProps) =
    let req = React.useContext requestContext
    React.fragment [
        Html.h3 "Advanced Single Page Application"

        Html.p "This section demonstrates how multiple sections of a page can be updated independently of each other. Each section has its own form and handler, and the page is updated without a full page reload. This uses a mixture of approaches but generally follows a pattern of utilizing query params for more ephemeral state like error validation messages along with a mechanism like GraphQL queries and mutations for more persistent storage. This approach allows for these interactions to continue to work without JavaScript enabled."
        
        nameSection { name = props.name; nameError = props.nameError}

        colorSection { color = props.color; colorError = props.colorError }

        nameAndEmailSection { inputName = props.inputName; inputEmail = props.inputEmail; inputNameErrors = props.inputNameErrors; inputEmailErrors = props.inputEmailErrors }

        Html.p "In the full code for the current page you'll see that we've encapsulated the components and route handlers into a single file using a single Express router that is later mounted onto our main application in a pattern that should seem very familiar to developers experienced with ExpressJS."

        Html.p "In the next section we will see the benefits of this architecture and how it can be used to create a separation of concerns between updating a UI based on user actions and then tracking those interactions."

        Html.p [
          req.Link {| href = "/analytics-router"; children = "Next: Analytics Router" |}
        ]

        Html.h4 "Full Code For Current Page"

        SPAFullCodeBlock
    ]

spa.get("/", fun req res next ->
    promise {
        let! response = 
            req 
            |> gql 
              "
              query { 
                  color { color } 
                  name { name }
              }
              " {||} {| cache = false |}
            
        match response with
        | Ok response ->
            let props = {
                color = response?color?color
                colorError = req.query?colorError
                name = response?name?name
                nameError = req.query?nameError
                inputName = req.query?inputName
                inputEmail = req.query?inputEmail
                inputNameErrors = req.query?inputNameErrors
                inputEmailErrors = req.query?inputEmailErrors
            }

            SinglePageApplicationAdvancedDemoPage props
            |> res.renderComponent
        | Error message -> next()
    } |> ignore
)

spa.post("/set-name", fun req res next ->
    let newName : string = req.body?name
    promise {
        let! response = 
            req 
            |> gql "mutation ($name: String) { setName(inputName: $name) { success } }" 
                {| name = newName |} {||}

        let name =
          match response with
          | Ok response -> { nameError = ""; name = newName }
          | Error message -> { nameError = message; name = newName }

        res.redirectBackAndMergeQuery<Name>(name)        
    } |> ignore
)

spa.post("/set-color", fun req res next ->
    let newColor : string = req.body?color
    promise {
        let! response = 
            req 
            |> gql "mutation ($color: String) { setColor(color: $color) { success } }" 
                {| color = newColor |} {||}

        let color = 
          match response with
          | Ok response -> { colorError = ""; color = newColor}
          | Error message -> { colorError = message; color = newColor}

        res.redirectBackAndMergeQuery<Color>(color)
    } |> ignore
)

spa.post("/set-name-and-email", fun req res next ->
    let requestBody = req.body
    let inputName = requestBody?inputName
    let inputEmail = requestBody?inputEmail
    
    let nameValidator fieldName =
        let msg = fun _ -> $"{fieldName} must be between 3 and 64 characters"
        Check.WithMessage.String.betweenLen 3 64 msg

    let emailValidator =
        ValidatorGroup(Check.WithMessage.String.betweenLen 7 256 (fun _ -> "Email must be between 7 and 256 characters"))                                                  
            .And(Check.WithMessage.String.pattern @"[^@]+@[^\.]+\..+" (fun _ -> "Please provide a valid email address"))
            .Build()

    let validatedInput =  
        validate {
            let! inputName = nameValidator "Name" "inputName" inputName
            and! inputEmail = emailValidator "inputEmail" inputEmail

            return {| inputName = inputName; inputEmail = inputEmail |}
        }
    
    let inputNameAndEmail = 
      match validatedInput with
      | Ok _ -> 
          { inputName = inputName; inputEmail = inputEmail; inputNameErrors = [||]; inputEmailErrors = [||] }
      | Error validationErrors ->
          let inputNameErrors = extractErrors validationErrors "inputName"
          let inputEmailErrors = extractErrors validationErrors "inputEmail"

          { inputNameErrors = inputNameErrors; inputEmailErrors = inputEmailErrors; inputName = inputName; inputEmail = inputEmail }
    
    res.redirectBackAndMergeQuery<InputNameAndEmail>(inputNameAndEmail)
)
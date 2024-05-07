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

type InputNameAndEmail = {
    inputName : string
    inputEmail : string
    inputNameErrors : string array option
    inputEmailErrors : string array option
}

type Color = {
    color : string
    colorError : string option
}

type Name = {
    name : string
    nameError : string option
}

type SinglePageApplicationAdvancedDemoPageProps = {
    color : Color
    name : Name
    inputNameAndEmail : InputNameAndEmail
}

[<ReactComponent>]
let nameSection props =
    let req = React.useContext requestContext
    React.fragment [
        Html.h4 [ prop.text "Name"; prop.id "setName" ]

        Html.p "In this section we're using a form to update the name. The form is submitted to the universal route handler and ultimately to the the server using the GraphQL mutation. This is followed by an update to either the DOM or the HTML response, depending on the context. The anchor ID is used to scroll to the correct section of the page after the page reloads for when JavaScript is disabled."

        match props.nameError with
        | Some "invalid-name" -> Html.p "Please enter your name:"
        | _ -> Html.p (sprintf "Hello, %s!" props.name)

        req.Form {| id = "setName"; baseAction = "/set-name#setName"; method = "post"; children = [
            Html.input [ prop.type' "text"; prop.key "name"; prop.name "name"; prop.placeholder "Name" ]
            Html.input [ prop.type' "submit"; prop.key "submit"; prop.value "Change Name" ]
        ] |}

        nameComponentCodeBlock
        nameHandlerCodeBlock
    ]

[<ReactComponent>]
let colorSection props = 
    let req = React.useContext requestContext
    React.fragment [
        Html.h4 [ prop.text "Color"; prop.id "setColor" ]

        Html.p "This section follows the same basic pattern but uses FormButton components instead of a Form component. The interaction is still founded on the Form post but with a convenient button component that simplifies the process. Again, notice the use of the query parameters in the URL to drive the error handling. This is a key part of the artchitecture that allows for multiple sections of the page to maintain their state independently."

        req.FormButton {| baseAction = "/set-color#setColor"; name = "color"; value = "red"; buttonText = "Red"|}
        req.FormButton {| baseAction = "/set-color#setColor"; name = "color"; value = "green"; buttonText = "Green"|}
        req.FormButton {| baseAction = "/set-color#setColor"; name = "color"; value = "blue"; buttonText = "Blue"|}
        req.FormButton {| baseAction = "/set-color#setColor"; name = "color"; value = "error"; buttonText = "Error"|}

        match props.colorError with
        | Some "invalid-color" -> Html.p "Invalid color. Please select red, green, or blue."
        | _ -> React.fragment []

        Html.div [
            prop.style [ style.color props.color ]
            prop.children [ Html.p "Click the buttons to change the color of this text." ]
        ]

        colorComponentCodeBlock
        colorHandlerCodeBlock
    ]

[<ReactComponent>]
let nameAndEmailSection props =
    let req = React.useContext requestContext
    React.fragment [
        Html.h4 [ prop.text "Name and Email"; prop.id "setNameAndEmail" ]

        Html.p "This section demonstrates how to handle multiple form inputs with multiple validation requirements. The form is again submitted to the universal route handler. For demonstratinon purposes this route handler does nothing beside handle and report validation errors."

        req.Form {| baseAction = "/set-name-and-email#setNameAndEmail"; method = "post"; children = [
            Html.div [
                prop.className "form-group"
                prop.children [
                    Html.label [ prop.htmlFor "inputName"; prop.text "Name" ]
                    let messageMapping = function
                        | "invalid-length" -> "Name must be between 3 and 64 characters."
                        | _ -> "Unknown error"
                    textInputFieldWithStringListError "inputName" "Name" props.inputName props.inputNameErrors messageMapping
                ]
            ]

            Html.div [
                prop.className "form-group"
                prop.children [
                    Html.label [ prop.htmlFor "inputEmail"; prop.text "Email" ]
                    let messageMapping = function
                        | "invalid-length" -> "Email must be between 7 and 256 characters."
                        | "invalid-email" -> "Please provide a valid email address."
                        | _ -> "Unknown error"
                    textInputFieldWithStringListError "inputEmail" "Email" props.inputEmail props.inputEmailErrors messageMapping
                ]
            ]

            Html.input [ prop.type' "submit"; prop.key "submit"; prop.value "Submit" ]
        ] |}

        nameAndEmailComponentCodeBlock
        nameAndEmailHandlerCodeBlock
    ]

[<ReactComponent>]
let SinglePageApplicationAdvancedDemoPage props =
    let req = React.useContext requestContext
    React.fragment [
        Html.h3 "Advanced Single Page Application"

        Html.p "This section demonstrates how multiple sections of a page can be updated independently of each other. Each section has its own form and handler, and the page is updated without a full page reload. This uses a mixture of approaches but generally follows a pattern of utilizing query params for more ephemeral state like error validation messages along with a mechanism like GraphQL queries and mutations for more persistent storage. This approach allows for these interactions to continue to work without JavaScript enabled."
        
        nameSection props.name

        colorSection props.color

        nameAndEmailSection props.inputNameAndEmail

        Html.p "In the full code for the current page you'll see that we've encapsulated the components and route handlers into a single file using a single Express router that is later mounted onto our main application in a pattern that should seem very familiar to developers experienced with ExpressJS."

        SPAUseCodeBlock

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
                color = { color = response?color?color; colorError = req.query?colorError }
                name = { name = response?name?name; nameError = req.query?nameError }
                inputNameAndEmail = { inputName = req.query?inputName; inputEmail = req.query?inputEmail; inputNameErrors = req.query?inputNameErrors; inputEmailErrors = req.query?inputEmailErrors }
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

        let nameError =
          match response with
          | Ok response -> None
          | Error message -> Some message

        res.redirectBack<Name>({
            name = newName
            nameError = nameError
        })        
    } |> ignore
)

spa.post("/set-color", fun req res next ->
    let newColor : string = req.body?color
    promise {
        let! response = 
            req 
            |> gql "mutation ($color: String) { setColor(color: $color) { success } }" 
                {| color = newColor |} {||}

        let colorError = 
          match response with
          | Ok response -> None
          | Error message -> Some message

        res.redirectBack<Color>({
            color = newColor
            colorError = colorError
        })
    } |> ignore
)

spa.post("/set-name-and-email", fun req res next ->
    let inputName = req.body?inputName
    let inputEmail = req.body?inputEmail
    
    let nameValidator =
        Check.WithMessage.String.betweenLen 3 64 (fun _ -> "invalid-length")

    let emailValidator =
        ValidatorGroup(Check.WithMessage.String.betweenLen 7 256 (fun _ -> "invalid-length"))                                                  
            .And(Check.WithMessage.String.pattern @"[^@]+@[^\.]+\..+" (fun _ -> "invalid-email"))
            .Build()

    let validatedInput =  
        validate {
            let! inputName = nameValidator "inputName" inputName
            and! inputEmail = emailValidator "inputEmail" inputEmail
            return {| inputName = inputName; inputEmail = inputEmail |}
        }
    
    let (inputNameErrors, inputEmailErrors) = 
      match validatedInput with
      | Ok _ -> None, None
      | Error validationErrors ->
          let inputNameErrors = extractErrors validationErrors "inputName"
          let inputEmailErrors = extractErrors validationErrors "inputEmail"
          (Some inputNameErrors, Some inputEmailErrors)
    
    res.redirectBack<InputNameAndEmail>({
        inputName = inputName
        inputEmail = inputEmail
        inputNameErrors = inputNameErrors
        inputEmailErrors = inputEmailErrors
    })
)
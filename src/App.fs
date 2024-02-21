module App

open Feliz
open Fable.Core.JsInterop
open Express
open Global
open GraphQLSchema
open Components

let universalApp (app: ExpressApp) =
    app.get("/", fun req res next ->
        let inputName =
            match req.query?inputName with
            | string as name -> Some name
            | _ -> None
        promise {
            let! response = 
                req 
                |> gql "query { greeting { heading content } }" {||}
                
            match response with
            | Ok response -> 
                let greeting : Greeting = response?greeting
                FrontPage ({| greeting = greeting; inputName = inputName |})
                |> res.renderComponent
            | Error message -> next()

        } |> ignore
    )

    app.get("/about", fun req res next ->
        Html.div [
            prop.text "This is a simple template for building a website with Fable, Fable.React, and Feliz."
        ]
        |> res.renderComponent
    )

    app.post("/name", fun req res next ->
        let inputName = req.body?inputName
        consoleLog inputName
        res.redirect("back")
    )

    app.``use`` (fun (req: ExpressReq) (res: ExpressRes) next ->
        res.status 404 |> ignore
        Html.div "This page isn't here!"
        |> res.renderComponent
    )

    let errorHandler (err: obj) (req: ExpressReq) (res: ExpressRes) (next: unit -> unit) =
        match err with
        | :? System.Exception as ex ->
            let message = ex.Message
            consoleLog message
            res.status 500 |> ignore
            Html.div message
            |> res.renderComponent
        | _ -> next()

    app.``use`` errorHandler
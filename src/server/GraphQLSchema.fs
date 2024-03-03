module GraphQLSchema

open Fable.Core.JsInterop
open Global

type Greeting = {
    heading: string
    subheading: string
    content: string
}

type Name = {
    name: string
}

type Color = {
    color: string
}

let schemaString = "
  scalar JSON

  type Greeting {
    heading: String
    subheading: String
    content: String
  }

  type Name {
    name: String
  }

  type Color {
    color: String
  }

  type SuccessResponse {
    success: Boolean
  }

  type Query {
    greeting: Greeting
    name: Name
    color: Color
  }
  
  type Mutation {
    setName(inputName: String): SuccessResponse
    setColor(color: String): SuccessResponse
  }
  "

let rootValueInitializer : obj =
    let greeting () =
        promise {
            let greeting = {
                heading = "Fex: F# and Express for Web Development"
                subheading = "A Unified Server-Client Architecture"
                content = "Fex is an architectural pattern for building web applications with Express in JavaScript, utilizing the F# Fable compiler and Feliz for React components. It's not a framework but an approach that emphasizes separation of concerns, simplicity and flexibility. By leveraging both the `express` npm module for server-side and the `browser-express` module for client-side operations, it allows developers to craft route handlers, middleware, and React components that seamlessly work across both environments. The goal is to simplify web development while giving developers the freedom to adapt their architecture as their application evolves to meet user needs."
            }
            return greeting
        }

    let setName (input: string) (req: obj) =
        promise {
            req?session?inputName <- input?inputName
            return {| success = true |}
        }

    let name _ (req: obj) =
        promise {
            let inputName = req?session?inputName
            return 
                match inputName with
                | null -> { name = "Anonymous" }
                | _ -> { name = inputName }    
        }

    let setColor (input: string) (req: obj) =
        promise {
            req?session?color <- input?color
            return match input?color with
                    | "red" | "green" | "blue" -> {| success = true |}
                    | _ -> failwith "invalid-color"
        }

    let color _ (req: obj) =
        promise {
            let color = req?session?color
            return 
                match color with
                | null -> { color = "" }
                | _ -> { color = color }    
        }

    {| greeting = greeting; setName = setName; name = name; setColor = setColor; color = color |}
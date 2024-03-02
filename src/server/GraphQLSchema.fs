module GraphQLSchema

open Fable.Core.JsInterop

type Greeting = {
    heading: string
    content: string
}

type Name = {
    name: string
}

let schemaString = "
  scalar JSON

  type Greeting {
    heading: String
    content: String
  }

  type Name {
    name: String
  }

  type SuccessResponse {
    success: Boolean
  }

  type Query {
    greeting: Greeting
    name: Name
  }
  
  type Mutation {
    setName(inputName: String): SuccessResponse
  }
  "

let rootValueInitializer : obj =
    let greeting () =
        promise {
            let greeting = {
                heading = "Fex: F# and Express for Web Development"
                content = "Fex is an architectural pattern for building web applications with Express in JavaScript, utilizing Fable, Fable.React, and Feliz. It's not a framework but an approach that emphasizes simplicity and flexibility. By leveraging both the `express` npm module for server-side and the `browser-express` module for client-side operations, it allows developers to craft route handlers, middleware, and React components that seamlessly work across both environments. The goal is to simplify web development while giving developers the freedom to adapt their architecture as their application evolves to meet user needs."
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

    {| greeting = greeting; setName = setName; name = name |}
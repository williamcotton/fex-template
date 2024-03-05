module Express

open Feliz
open Fable.Core

type ExpressReq =
  abstract member ``params`` : obj
  abstract member body : obj
  abstract member query : obj
  abstract member Link : obj -> ReactElement
  abstract member Form : obj -> ReactElement
  abstract member FormButton : obj -> ReactElement
  abstract member method : string
  abstract member path : string
  abstract member originalUrl : string
  abstract member url : string
  abstract member headers : obj
  abstract member baseUrl : string
  abstract member hostname : string
  abstract member ip : string
  abstract member protocol : string
  abstract member secure : bool
  abstract member xhr : bool
  abstract member status : int
  abstract member gql : string -> obj -> obj -> JS.Promise<obj>
  abstract member session : obj
  abstract member fetchJson : string -> obj -> obj -> JS.Promise<{| text: unit -> JS.Promise<string>; json: unit -> JS.Promise<obj> |}>

type ExpressRes =
  abstract member send : obj -> unit
  abstract member renderComponent : ReactElement -> unit
  abstract member renderErrorComponent : ReactElement -> unit
  abstract member status : int -> unit
  abstract member redirect : string -> unit
  abstract member redirectBack : obj -> unit
  abstract member redirectBackAndMergeQuery : 'T -> unit
  abstract member navigate : string -> unit
  abstract member pageview : obj -> unit
  abstract member event : obj -> unit

type ExpressApp =
  abstract member get: string * (ExpressReq -> ExpressRes -> (obj -> unit) -> unit) -> unit
  abstract member post: string * (ExpressReq -> ExpressRes -> (obj -> unit) -> unit) -> unit
  abstract member listen: int * (unit -> unit) -> unit
  abstract member ``use``: (obj -> ExpressReq -> ExpressRes -> (unit -> unit) -> unit) -> unit
  abstract member ``use``: (ExpressReq -> ExpressRes -> (unit -> unit) -> unit) -> unit
  abstract member ``use``: string * ExpressApp -> unit

let gql (query: string) (variables: obj) (options: obj) (req: ExpressReq)  : JS.Promise<Result<obj, string>> =
  promise {
    try
      let! result = req.gql query variables options
      return Ok result
    with
    | ex -> return Error ex.Message
  }
module Global

open Fable.Core
open Feliz
open System
open Validus

[<Emit("console.log($0)")>]
let consoleLog text: unit = jsNative

[<Emit("fetch($0)")>] // TODO: fix this
let fetch (url: string): JS.Promise<{| text: unit -> JS.Promise<string>; json: unit -> JS.Promise<obj> |}> = jsNative

[<Emit("$0 === undefined")>]
let isUndefined (x: 'a) : bool = jsNative

[<Emit("undefined")>]
let undefined : obj = jsNative

[<Emit("app.use($0, $1)")>]
let useMiddlewareRoute route middleware: unit = jsNative

let addOrdinal day =
    match day % 10 with
    | 1 when day % 100 <> 11 -> sprintf "%dst" day
    | 2 when day % 100 <> 12 -> sprintf "%dnd" day
    | 3 when day % 100 <> 13 -> sprintf "%drd" day
    | _ -> sprintf "%dth" day

let monthNames monthNumber =
    match monthNumber with
    | 1 -> "January"
    | 2 -> "February"
    | 3 -> "March"
    | 4 -> "April"
    | 5 -> "May"
    | 6 -> "June"
    | 7 -> "July"
    | 8 -> "August"
    | 9 -> "September"
    | 10 -> "October"
    | 11 -> "November"
    | 12 -> "December"
    | _ -> failwith "Invalid month number"

let formatDateString inputDate =
    let parsedDate = DateTime.Parse(inputDate)
    let monthName = monthNames parsedDate.Month
    let dayWithOrdinal = addOrdinal parsedDate.Day
    sprintf "%s %s, %d" monthName dayWithOrdinal parsedDate.Year

let textInputFieldWithError fieldName placeholder (defaultValue: string) (errors : Map<string, string list>) =
    let hasError fieldName = errors.ContainsKey(fieldName)
    if hasError fieldName then 
        React.fragment [
            Html.input [ prop.type' "text"; prop.key fieldName; prop.name fieldName; prop.placeholder placeholder; prop.className "error"; prop.defaultValue defaultValue ]
            yield! errors.[fieldName] |> List.map (fun errorMsg -> Html.p [ prop.text errorMsg; prop.className "error" ])
        ]
    else
        Html.input [ prop.type' "text"; prop.key fieldName; prop.name fieldName; prop.placeholder placeholder; prop.defaultValue defaultValue ]

let textInputFieldWithStringListError fieldName placeholder (defaultValue: string) (errors : string array option) (messageMapping : string -> string) =
    let errors = defaultArg errors [||]
    let hasNoErrors = Array.isEmpty errors
    if hasNoErrors then 
        Html.input [ prop.type' "text"; prop.name fieldName; prop.placeholder placeholder; prop.defaultValue defaultValue ]
    else
        React.fragment [
            Html.input [ prop.type' "text"; prop.name fieldName; prop.placeholder placeholder; prop.className "error"; prop.defaultValue defaultValue ]
            yield! errors |> Array.map (fun errorType ->
                let errorMsg = messageMapping errorType
                Html.p [ prop.text errorMsg; prop.className "error" ]
            )
        ]

let extractErrors (validationErrors : ValidationErrors) fieldName =
    let errorsMap = validationErrors |> ValidationErrors.toMap
    match errorsMap.TryGetValue(fieldName) with
    | true, errors -> errors |> List.toArray
    | false, _ -> [||]


[<Emit("Object.keys($0).every(key => $0[key] === '' || $0[key] === null || $0[key] === undefined)")>]
let inline isObjEmpty (requestBody: obj) : bool = jsNative

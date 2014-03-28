module FurlRouteMapParser

open System
open System.Net.Http
open FParsec
open ParserHelpers
open FurlStrong.Parsing
open FurlStrong.Parsing.AST

let private phttpMethod = 
                stringReturn "GET" HttpMethod.Get <|>
                stringReturn "POST" HttpMethod.Post <|>
                stringReturn "PUT" HttpMethod.Put <|>
                stringReturn "DELETE" HttpMethod.Delete 
                <!> "http method"

let private pnamespace = attempt (opt (str "namespace " >>. many1Satisfy (isNoneOf " \t\n"))) |>> OptionalNamespace <!> "namespace declaration"
let private pcomment = attempt (opt (ch '#' >>. nbws >>. restOfLine false)) |>> OptionalComment <!> "comment"
let private proute = tuple3 (phttpMethod) (nbws >>. FurlPathParser.ppath) (nbws >>. pcomment .>> skipRestOfLine true) |>> Route <!> "route"
let private proute_map = tuple2 pnamespace (many (ws >>. proute)) |>> RouteMap <!> "route map"

let private parser = ws >>. proute_map .>> eof

let private ParseAST str =
    match run parser str with
    | Success(result, _, _)   -> result 
    | Failure(errorMsg, errorContext, _) -> raise (new ParseException(errorMsg, errorContext))

let Parse str = ParseAST str //|> ASTToObjectModelVisitor.Visit

let PrettyPrint a = sprintf "%A" a

#if DEBUG
let Test p str =
    match run p str with
    | Success(result, _, _)   -> printfn "Success: %A" result
    | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg

let Run p str =
    match run p str with
    | Success(result, _, _)   -> result
    | Failure(errorMsg, errorContext, _) -> raise (new ParseException(errorMsg, errorContext))
#endif
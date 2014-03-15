module FurlParser

open System
open FParsec
open ParserHelpers
open FurlStrong.Parsing
open FurlStrong.Parsing.AST


let private pscheme = attempt (manyChars letter .>> str "://" |>> Scheme ) <!> "scheme"
let private pcredentials = (parseIf (regex "[^/]+\@") "expected '@'" (anything_until ':' Username .>>. anything_until '@' Password) |>> Credentials) <!> "credentials"
let private phost = attempt (manySatisfy (fun c -> match c with ':'|'/'->false|_->true) |>> Host) <!> "host"
let private pport = attempt (ch ':' >>. pint32 |>> Port) <!> "port"
let private ppath = FurlPathParser.ppath

let private purl = tuple5 (opt pscheme) (opt pcredentials) (opt phost) (opt pport) (opt ppath) |>> Url <!> "url"

let private parser = purl .>> eof

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
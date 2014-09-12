module FurlParser

open FParsec
open ParserHelpers
open FurlSharp.Parsing
open FurlSharp.Parsing.AST

let private pscheme = attempt (manyChars letter .>> str "://" |>> Scheme ) <!> "scheme"
let private pcredentials = (parseIf (regex "[^/]+\@") "expected '@'" (anything_until ':' Username .>>. anything_until '@' Password) |>> Credentials) <!> "credentials"
let private isHostPartChar c = match c with
                               | ':'
                               | '/'
                               | '.' -> false
                               | _ -> true

let private phostPart = many1Satisfy isHostPartChar <!> "host part"
let private phost = attempt (phostPart .>> ch '.' .>>. sepBy1 (phostPart) (ch '.')
                             |>> (fun (firstPart,hostParts) -> firstPart + "." + System.String.Join(".", hostParts))
                             |>> Host) <!> "host"
let private pport = attempt (ch ':' >>. pint32 |>> Port) <!> "port"
let private ppath = FurlPathParser.ppath
let private pquery = FurlQueryStringParser.pqueryString

let private pfragment = attempt (ch '#' >>. ppath .>>. opt pquery |>> Fragment) <!> "fragment"

let private purl = tuple7 (opt pscheme) (opt pcredentials) (opt phost) (opt pport) (opt ppath) (opt pquery) (opt pfragment) |>> Url <!> "url"

let private parser = purl .>> eof

let private ParseAST str =
    match run parser str with
    | Success(result, _, _)   -> result
    | Failure(errorMsg, errorContext, _) -> raise (new ParseException(errorMsg, errorContext))

let Parse str = ParseAST str //|> ASTToObjectModelVisitor.Visit

let ParseFragment str =
    match run pfragment str with
    | Success(result, _, _)   -> result
    | Failure(errorMsg, errorContext, _) -> raise (new ParseException(errorMsg, errorContext))

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
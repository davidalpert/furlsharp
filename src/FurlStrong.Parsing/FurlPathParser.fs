module FurlPathParser

open System
open FParsec
open FurlStrong.Parsing
open FurlStrong.Parsing.AST
open ParserHelpers

// paths
let private proot = attempt (opt (ch '/')) <!> "path root"
let private ppathNode = attempt (many1Satisfy (isNoneOf "/#")) <|> (lookAhead (ch '/') >>. preturn "") // <!> "path node"
let private ppathPart = pipe2 (ppathNode) (opt (ch '/')) (fun node trailingSlash -> (node,trailingSlash.IsSome)) // <!> "path part"
let internal ppath = pipe2 proot (many (ppathPart)) (fun root nodes -> (root.IsSome,nodes)) |>> Path <!> "path"

let private parser = ppath

let Parse str = 
    match run parser str with
    | Success(result, _, _)   -> result 
    | Failure(errorMsg, errorContext, _) -> raise (new ParseException(errorMsg, errorContext))

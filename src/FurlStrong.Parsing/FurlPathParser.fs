module FurlPathParser

open System
open FParsec
open FurlStrong.Parsing
open FurlStrong.Parsing.AST
open ParserHelpers

// paths
let private proot = attempt (opt (ch '/')) <!> "path root"
let private ppathNode = pipe2 (many1Satisfy (isNoneOf "/#")) (opt (ch '/')) (fun node trailingSlash -> (node,trailingSlash.IsSome)) <!> "path node"
let internal ppath = pipe2 proot (many (ppathNode)) (fun root nodes -> (root.IsSome,nodes)) |>> Path <!> "path"

let private parser = ppath

let Parse str = 
    match run parser str with
    | Success(result, _, _)   -> result 
    | Failure(errorMsg, errorContext, _) -> raise (new ParseException(errorMsg, errorContext))

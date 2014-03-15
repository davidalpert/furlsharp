module FurlPathParser

open System
open FParsec
open FurlStrong.Parsing
open FurlStrong.Parsing.AST
open ParserHelpers

// paths
let private proot = attempt (opt (ch '/')) <!> "path root"
let private ppathNode = manySatisfy((<>) '/') <!> "path node"
//let private ppathPart = ch '/' >>. manySatisfy((<>) '/') <!> "path part"
let internal ppath = pipe3 proot ppathNode (many (ch '/' >>. ppathNode)) (fun root rootNode nodes -> (root.IsSome,rootNode::nodes)) |>> Path <!> "path"

let private parser = ppath

let Parse str = 
    match run parser str with
    | Success(result, _, _)   -> result 
    | Failure(errorMsg, errorContext, _) -> raise (new ParseException(errorMsg, errorContext))

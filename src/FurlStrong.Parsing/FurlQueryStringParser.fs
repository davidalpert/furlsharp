module FurlQueryStringParser

open System
open FParsec
open FurlStrong.Parsing
open FurlStrong.Parsing.AST
open ParserHelpers

// QueryStrings
let private pqueryPair = attempt (tuple2 (optional (ch '&') >>. many1Satisfy (isNoneOf "/#=&")) (attempt (opt (ch '=' >>. manySatisfy (isNoneOf "/#&")))) |>> QueryStringParameter) <!> "key/value"
let internal pqueryString = attempt (ch '?' >>. many pqueryPair |>> QueryString) <!> "querystring"

let private parser = pqueryString

let Parse str = 
    match run parser str with
    | Success(result, _, _)   -> result 
    | Failure(errorMsg, errorContext, _) -> raise (new ParseException(errorMsg, errorContext))

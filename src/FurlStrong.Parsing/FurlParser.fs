module FurlParser

open System
open FParsec
open FurlStrong.Parsing.AST

// make this compiler directive condition true to trace the parsers
#if DEBUG
let (<!>) (p: Parser<_,_>) label : Parser<_,_> =
    fun stream ->
        printfn "%A: Entering %s" stream.Position label
        let reply = p stream
        printfn "%A: Leaving %s (%A)" stream.Position label reply.Status
        reply
#else
let (<!>) (p: Parser<_,_>) label : Parser<_,_> =
    fun stream -> p stream
#endif

let private ws = spaces
let private ch c = pchar c
let private ch_ws c = ch c .>> ws
let private ws_ch_ws c = ws >>. ch c .>> ws

let private str s = pstring s
let private str_ws s = str s .>> ws
let private ws_str_ws s = ws >>. str s .>> ws

let private between_ch copen p cclose = between (ch copen) (ch cclose) p

let resultSatisfies predicate msg (p: Parser<_,_>) : Parser<_,_> =
    let error = messageError msg
    fun stream ->
      let state = stream.State
      let reply = p stream
      if reply.Status <> Ok || predicate reply.Result then reply
      else
          stream.BacktrackTo(state) // backtrack to beginning
          Reply(Error, error)

let parseIf (p1:Parser<_,_>) msg (p2:Parser<_,_>) : Parser<_,_> = 
    let error = messageError msg
    fun stream ->
        let state = stream.State
        let reply = p1 stream
        stream.BacktrackTo(state) // backtrack to beginning
        if reply.Status = Ok then
            p2 stream
        else
            Reply(Error, error)

//let private anything_until c = manySatisfy ((<>) c) .>> ch c
let private anything_until c a = manySatisfy ((<>) c) .>> ch c |>> a
let private max_int = Int32.MaxValue

let private pscheme = opt (manyChars letter .>> str "://" |>> Scheme ) <!> "scheme"
//let private pcredentials = opt (anything_until ':' .>>. anything_until '@' |>> (fun(u,p) -> new Credentials(new Username(u),new Password(p))
let private pcredentials = opt (parseIf (regex "[^/]+\@") "expected '@'" (anything_until ':' Username .>>. anything_until '@' Password) |>> Credentials)
let private phost = opt (manySatisfy (fun c -> match c with ':'|'/'->false|_->true) |>> Host) <!> "host"
let private pport = attempt (opt (ch ':' >>. pint32 |>> Port)) <!> "port"
let private purl = tuple4 pscheme pcredentials phost pport |>> Url <!> "url"

let private parser = purl .>> eof

exception ParseError of string * ParserError

type ParseException (message:string, context:ParserError) =
    inherit ApplicationException(message, null)
    let Context = context

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
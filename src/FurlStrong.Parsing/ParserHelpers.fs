module ParserHelpers

open System
open FParsec

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
let ch c = pchar c
let private ch_ws c = ch c .>> ws
let private ws_ch_ws c = ws >>. ch c .>> ws

let str s = pstring s
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

let anything_until c a = manySatisfy ((<>) c) .>> ch c |>> a
let max_int = Int32.MaxValue

let pipe6 p1 p2 p3 p4 p5 p6 fn =
        p1 >>= fun a ->
        p2 >>= fun b ->
        p3 >>= fun c ->
        p4 >>= fun d ->
        p5 >>= fun e -> 
        p6 >>= fun f -> preturn (fn a b c d e f)

let tuple6 p1 p2 p3 p4 p5 p6 = 
     pipe6 p1 p2 p3 p4 p5 p6 (fun a b c d e f -> (a, b, c, d, e, f))

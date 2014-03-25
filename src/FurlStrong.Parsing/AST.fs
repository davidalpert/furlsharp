namespace FurlStrong.Parsing.AST
open System.Net.Http

type Scheme = Scheme of string
type Username = Username of string
type Password = Password of string
type Credentials = Credentials of Username * Password
type Host = Host of string
type Port = Port of int
type PathNode = string * bool // the node * has trailing slash?
type Path = Path of bool * PathNode list
type QueryStringParameter = QueryStringParameter of string * string option
type QueryString = QueryString of QueryStringParameter list
type Fragment = Fragment of Path * QueryString option

type Url = Url of Scheme option * Credentials option * Host option * Port option * Path option * QueryString option * Fragment option

type OptionalComment = OptionalComment of string option
type Route = Route of HttpMethod * Path * OptionalComment 

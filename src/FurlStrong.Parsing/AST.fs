namespace FurlStrong.Parsing.AST

type Scheme = Scheme of string
type Username = Username of string
type Password = Password of string
type Credentials = Credentials of Username * Password
type Host = Host of string
type Port = Port of int

type Url = Url of Scheme option * Credentials option * Host option * Port option

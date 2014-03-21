using NUnit.Framework;

namespace Furlstrong.Tests
{
    [TestFixture]
    public class Parameters
    {
        [Test]
        public void Empty_strings_produce_empty_arguments()
        {
            var f = new Furl("http://sprop.su");

            f.Query["param"] = "";

            Assert.AreEqual("http://sprop.su/?param=", f.Url);
        }

        [Test]
        public void Null_values_produce_empty_arguments_without_the_trailing_slash()
        {
            var f = new Furl("http://sprop.su");

            f.Query["param"] = null;

            Assert.AreEqual("http://sprop.su/?param", f.Url);
        }

        [Test]
        public void Encode_can_be_used_to_encode_query_strings_with_custom_delimeters()
        {
            var f = new Furl();

            f.Query = FurlQuery.Parse("space=jams&woofs=squeeze%20dog");

            Assert.AreEqual("space=jams&woofs=squeeze%20dog", f.Query.Encode());

            Assert.AreEqual("space=jams;woofs=squeeze%20dog", f.Query.Encode(';'));
        }
        /*
         * 
Parameters
         * 
         * 
__params__ can also store multiple values for the same key because it's a
multivalue dictionary.

```pycon
>>> f = furl('http://www.google.com/?space=jams&space=slams')
>>> f.args['space']
'jams'
>>> f.args.getlist('space')
['jams', 'slams']
>>> f.args.addlist('repeated', ['1', '2', '3'])
>>> str(f.query)
'space=jams&space=slams&repeated=1&repeated=2&repeated=3'
>>> f.args.popvalue('space')
'slams'
>>> f.args.popvalue('repeated', '2')
'2'
>>> str(f.query)
'space=jams&repeated=1&repeated=3'
```

__params__ is one dimensional. If a list of values is provided as a query value,
that list is interpretted as multiple values.

```pycon
>>> f = furl()
>>> f.args['repeated'] = ['1', '2', '3']
>>> f.add(args={'space':['jams', 'slams']})
>>> str(f.query)
'repeated=1&repeated=2&repeated=3&space=jams&space=slams'
```

This makes sense -- URL queries are inherently one dimensional. Query values
cannot have subvalues.

See the [omdict](https://github.com/gruns/orderedmultidict) documentation for
more information on interacting with the ordered multivalue dictionary
__params__.

##### Parameters

To produce an empty query argument like `http://sprop.su/?param=`, use an empty
string as the parameter value.

```pycon
>>> f = furl('http://sprop.su')
>>> f.args['param'] = ''
>>> f.url
'http://sprop.su/?param='
```

To produce an empty query argument without a trailing `=`, use `None` as the
parameter value.

```pycon
>>> f = furl('http://sprop.su')
>>> f.args['param'] = None
>>> f.url
'http://sprop.su/?param'
```

__encode(delimeter='&')__ can be used to encode query strings with delimeters
like `;`.

```pycon
>>> f.query = 'space=jams&woofs=squeeze+dog'
>>> f.query.encode()
'space=jams&woofs=squeeze+dog'
>>> f.query.encode(';')
'space=jams;woofs=squeeze+dog'
```



### Fragment

URL fragments in furl are Fragment objects that have a Path __path__ and Query
__query__ separated by an optional '?' __separator__.

```pycon
>>> f = furl('http://www.google.com/#/fragment/path?with=params')
>>> f.fragment
Fragment('/fragment/path?with=params')
>>> f.fragment.path
Path('/fragment/path')
>>> f.fragment.query
Query('with=params')
>>> f.fragment.separator
True
```

Manipulation of Fragments is done through the Fragment's Path and Query
instances, __path__ and __query__.

```pycon
>>> f = furl('http://www.google.com/#/fragment/path?with=params')
>>> str(f.fragment)
'/fragment/path?with=params'
>>> f.fragment.path.segments.append('file.ext')
>>> str(f.fragment)
'/fragment/path/file.ext?with=params'

>>> f = furl('http://www.google.com/#/fragment/path?with=params')
>>> str(f.fragment)
'/fragment/path?with=params'
>>> f.fragment.args['new'] = 'yep'
>>> str(f.fragment)
'/fragment/path?new=yep&with=params'
```

Creating hash-bang fragments with furl illustrates the use of Fragment's
__separator__. When __separator__ is False, the '?' separating __path__ and
__query__ isn't included.

```pycon
>>> f = furl('http://www.google.com/')
>>> f.fragment.path = '!'
>>> f.fragment.args = {'a':'dict', 'of':'args'}
>>> f.fragment.separator
True
>>> str(f.fragment)
'!?a=dict&of=args'

>>> f.fragment.separator = False
>>> str(f.fragment)
'!a=dict&of=args'
>>> f.url
'http://www.google.com/#!a=dict&of=args'
```

### Encoding

Furl handles encoding for you, and furl's philosophy on encoding is simple:
whole path, query, and fragment strings should always be encoded.

```pycon
>>> f = furl()
>>> f.path = 'supply%20encoded/whole%20path%20strings'
>>> f.path.segments
['supply encoded', 'whole path strings']

>>> f.set(query='supply+encoded=query+strings,+too')
>>> f.query.params
omdict1D([('supply encoded', 'query strings, too')])

>>> f.set(fragment='encoded%20path%20string?and+encoded=query+string+too')
>>> f.fragment.path.segments
['encoded path string']
>>> f.fragment.args
omdict1D([('and encoded', 'query string too')])
```

Path, Query, and Fragment strings should always be decoded.

```pycon
>>> f = furl()
>>> f.set(path=['path segments are', 'decoded', '<>[]"#'])
>>> str(f.path)
'/path%20segments%20are/decoded/%3C%3E%5B%5D%22%23'

>>> f.set(args={'query parameters':'and values', 'are':'decoded, too'})
>>> str(f.query)
'query+parameters=and+values&are=decoded,+too'

>>> f.fragment.path.segments = ['decoded', 'path segments']
>>> f.fragment.args = {'and decoded':'query parameters and values'}
>>> str(f.fragment)
'decoded/path%20segments?and+decoded=query+parameters+and+values'
```

Python's
[urllib.quote()](http://docs.python.org/library/urllib.html#urllib.quote) and
[urllib.unquote()](http://docs.python.org/library/urllib.html#urllib.unquote)
can be used to encode and decode path strings. Similarly,
[urllib.quote_plus()](http://docs.python.org/library/urllib.html#urllib.quote_plus)
and
[urllib.unquote_plus()](http://docs.python.org/library/urllib.html#urllib.unquote_plus)
can be used to encode and decode query strings.


### Inline manipulation

For quick, single-line URL manipulation, the __add()__, __set()__, and
__remove()__ methods of furl objects manipulate various components of the URL
and return the furl object for method chaining.

```pycon
>>> url = 'http://www.google.com/#fragment' 
>>> furl(url).add(args={'example':'arg'}).set(port=99).remove(fragment=True).url
'http://www.google.com:99/?example=arg'
```

__add()__ adds items to a furl object with the optional arguments

 * __args__: Shortcut for __query_params__.
 * __path__: A list of path segments to add to the existing path segments, or a
   path string to join with the existing path string.
 * __query_params__: A dictionary of query keys and values to add to the query.
 * __fragment_path__: A list of path segments to add to the existing fragment
   path segments, or a path string to join with the existing fragment path
   string.
 * __fragment_args__: A dictionary of query keys and values to add to the
   fragment's query.

```pycon
>>> url = 'http://www.google.com/' 
>>> furl(url).add(path='/index.html', fragment_path='frag/path',
                  fragment_args={'frag':'args'}).url
'http://www.google.com/index.html#frag/path?frag=args'
```

__set()__ sets items of a furl object with the optional arguments

 * __args__: Shortcut for __query_params__.
 * __path__: List of path segments or a path string to adopt.
 * __scheme__: Scheme string to adopt.
 * __netloc__: Network location string to adopt.
 * __query__: Query string to adopt.
 * __query_params__: A dictionary of query keys and values to adopt.
 * __fragment__: Fragment string to adopt.
 * __fragment_path__: A list of path segments to adopt for the fragment's path
   or a path string to adopt as the fragment's path.
 * __fragment_args__: A dictionary of query keys and values for the fragment's
   query to adopt.
 * __fragment_separator__: Boolean whether or not there should be a '?'
   separator between the fragment path and the fragment query.
 * __host__: Host string to adopt.
 * __port__: Port number to adopt.
 * __username__: Username string to adopt.
 * __password__: password string to adopt.


```pycon
>>> furl().set(scheme='https', host='secure.google.com', port=99,
               path='index.html', args={'some':'args'}, fragment='great job').url
'https://secure.google.com:99/index.html?some=args#great%20job'
```

__remove()__ removes items from a furl object with the optional arguments

 * __args__: Shortcut for __query_params__.
 * __path__: A list of path segments to remove from the end of the existing path
        segments list, or a path string to remove from the end of the existing
        path string, or True to remove the path entirely.
 * __fragment__: If True, remove the fragment portion of the URL entirely.
 * __query__: If True, remove the query portion of the URL entirely.
 * __query_params__: A list of query keys to remove from the query, if they
        exist.
 * __port__: If True, remove the port from the network location string, if it
   exists.
 * __fragment_path__: A list of path segments to remove from the end of the
   fragment's path segments, or a path string to remove from the end of the
   fragment's path string, or True to remove the fragment path entirely.
 * __fragment_args__: A list of query keys to remove from the fragment's query,
   if they exist.
 * __username__: If True, remove the username, if it exists.
 * __password__: If True, remove the password, if it exists.


```pycon
>>> url = 'https://secure.google.com:99/a/path/?some=args#great job'
>>> furl(url).remove(args=['some'], path='path/', fragment=True, port=True).url
'https://secure.google.com/a/'
```


### Miscellaneous

__copy()__ creates and returns a new furl object with an identical URL.

```pycon
>>> f = furl('http://www.google.com')
>>> f.copy().set(path='/new/path').url
'http://www.google.com/new/path'
>>> f.url
'http://www.google.com'
```

__join()__ joins the furl object's URL with the provided relative or absolute
URL and returns the furl object for method chaining. __join()__'s action is the
same as clicking on the provided relative or absolute URL in a browser.

```pycon
>>> f = furl('http://www.google.com')
>>> f.join('new/path').url
'http://www.google.com/new/path'
>>> f.join('replaced').url
'http://www.google.com/new/replaced'
>>> f.join('../parent').url
'http://www.google.com/parent'
>>> f.join('path?query=yes#fragment').url
'http://www.google.com/path?query=yes#fragment'
>>> f.join('unknown://www.yahoo.com/new/url/').url
'unknown://www.yahoo.com/new/url/'
```
         */
    }
}
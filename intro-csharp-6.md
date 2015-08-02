# Introduction to C# 6.0
---
## History of C# language

|C# version| .NET Framework  |  Language Features  |
|---|---|---|
| C# 1.0 | *.NET Framework 1.0*     |OOP, properties, *'Java clone'*   |
| C# 2.0 | *.NET Framework 2.0*     |generics, partial types, anonymus methods, iterators, nullable types, static classes etc.   |
| C# 3.0 | *.NET Framework 3.0/3.5* |implicitly typed local variables (`var`), object and **collection initializers**, auto-properties, anonymus types, extension methods, lambda expression, query expressions, Expression Tree API, partial methods  |
| C# 4.0 | *.NET Framework 4.0*     |dynamic binding (`dynamic`), named and optional arguments, generic co- and contravariance   |
| C# 5.0 | *.NET Framework 4.5*     |asynchronous methods (`async/await`), caller info attributes   |

---
## C# 6.0 (*.NET Framework 4.6*)

- **compiler-as-service** ([Roslyn/Microsoft Code Analysis](https://github.com/dotnet/roslyn))
- using static
- nameof operator
- string interpolation
- null-conditional operator
- parameterless contructor in `struct`
- auto-property initializer
- expression bodied functions and properties
- index initializers
- exception filters
- await in catch and finally blocks
- ~~primary constructor~~
- ~~inline declarations for out params~~
- ~~indexed property operator ($)~~
- ~~binary numeric literal~~
- ~~numeric separator (_)~~

more: [https://damieng.com/blog/2013/12/09/probable-c-6-0-features-illustrated](https://damieng.com/blog/2013/12/09/probable-c-6-0-features-illustrated)
---
## Using static
### C# 2.0+ version
```csharp
public class Program
{
  public static double SolveQuadraticEquation(double a, double b, double c)
  {
    return (-b + Math.Sqrt(Math.Pow(b, 2) - 4*a*c)) / (2 * a);
  }
}
```
---
## Using static
### C# 6.0 version
```csharp
using static System.Math; // just specify static class name

public static class Program
{
  public static double SolveQuadraticEquation(double a, double b, double c)
  {
    // no need Math class name
    return (-b + Sqrt(Pow(b, 2) - 4*a*c)) / (2 * a);
  }
  /* other members */
}
```

---
## Using static - more realistic example
```csharp
public static class HtmlBuilder
{
    public static string Tag(string name, params string[] children)
    {
        return String.Format("<{0}>{1}</{2}>",
                            name,
                            string.Join(string.Empty, children),
                            name);
    }

    public static string Html(params string[] children)
    {
        return Tag("p", children);
    }
    public static string Body(params string[] children)
    {
        return Tag("p", children);
    }

    public static string P(params string[] children)
    {
        return Tag("p", children);
    }

    /* and so on ... */
}
```
---
## Using static - more realistic example
### Code (C# 1.0+)
```csharp
private static void Main(string[] args)
{
    Console.WriteLine(
      HtmlBuilder.Html(
        HtmlBuilder.Body(
          HtmlBuilder.H1("This is the title"),
          HtmlBuilder.P("Hello World!")
        )
      )
    );
}
```
### Output
```html
<html><body><h1>This is the title</h1><p>Hello World!</p></body></html>
```
---
## Using static - more realistic example
### Code (C# 6.0)
```csharp
using static MyLib.HtmlBuilder;

private static void Main(string[] args)
{
    Console.WriteLine(
      Html(
        Body(
          H1("This is the title"),
          P("Hello World!")
        )
      )
    );
}
```
### Output
```html
<html><body><h1>This is the title</h1><p>Hello World!</p></body></html>
```
---
## nameof operator ([link](https://dotnetfiddle.net/DaHvHW))
```csharp
public class Point
{
  public int X { get; private set; }
  public int Y { get; private set; }

  public Point(int x, int y)
  {
    this.X = x;
    this.Y = y;
  }
}
```
---
## nameof operator ([link](https://dotnetfiddle.net/DaHvHW))
### Code (C# 1.0+)
```csharp
public static double GetDistanceFromOrigin(Point point)
{
  // common design pattern: guard clause
  if (point != null) {
    throw new ArgumentNullException("point");
  }

  return Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));
}
```
---
## nameof operator ([link](https://dotnetfiddle.net/DaHvHW))
### Code (C# 1.0+)
```csharp
public static class Ensure {
  public static void NotNull<TValue>(TValue value, string argName)
    where TValue: class
  {
    if (value != null) {
      throw new ArgumentNullException(argName);
    }
  }
}

/* in other class: */
public static double GetDistanceFromOrigin(Point point)
{
  // common design pattern: guard clause
  Ensure.NotNull(point, "point");  // not refactoring-safe :(
  return Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));
}
```
---
## nameof operator ([link](https://dotnetfiddle.net/DaHvHW))
### Code (C# 3.5+)
```csharp
using System.Linq.Expressions;

public static class Ensure {
  private static string GetArgumentName(Expression<Func<object>> expression) {
    var body = expression.Body as MemberExpression;
    if (body == null) {
      body = ((UnaryExpression)expression.Body).Operand as MemberExpression;
    }
    return body.Member.Name;
  }

  public static void NotNull(Expression<Func<object>> argument)
  {
    var value = argument.Compile().Invoke();
    if (value == null) {
      var argName = GetArgumentName(argument);
      throw new ArgumentNullException(argName);
    }
  }
}
```
---
## nameof operator ([link](https://dotnetfiddle.net/DaHvHW))
### Code (C# 3.5+)
```csharp
public static class Ensure
{
  public static void NotNull(Expression<Func<object>> argument) {
    // implementation
  }
  // ...
}

/* in other class: */
public static double GetDistanceFromOrigin(Point point)
{
  Ensure.NotNull(() => point); // refactoring-safe with run-time overhead :(
  return Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));
}
```

> This problem occurs in WPF too when refreshing controls by `OnPropertyChanged/RaisePropertyChanged` methods
---
## nameof operator ([link](https://dotnetfiddle.net/DaHvHW))
### Code (C# 6.0+)
```csharp
public static class Ensure
{
  public static void NotNull<TValue>(TValue value, string argName)
    where TValue: class
  {
    // implementation
  }
}

/* in other class: */
public static double GetDistanceFromOrigin(Point point)
{
  // refactoring-safe *without* run-time overhead :)
  Ensure.NotNull(point, nameof(point));
  // Ensure.NotNull(point, "point");  <-- the compiler generates this
  return Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));
}
```
---
## nameof operator ([link](https://dotnetfiddle.net/DaHvHW))
### Code (C# 6.0+)
```csharp
/* in other class: */
public static class Program {
  public static void Main()
  {
    int x = 10;

    Console.WriteLine(nameof(Main));  // Main
    Console.WriteLine(nameof(Program.Main));  // Main
    Console.WriteLine(nameof(x)); // x
    Console.WriteLine(nameof(x.ToString)); // ToString
    Console.WriteLine(nameof(x) + "." + nameof(x.ToString)); // x.ToString
    Console.WriteLine(nameof(x + 1)); // compile error:
                                      // Expression does not have a name
  }
}
```
---
## nameof operator ([link](https://dotnetfiddle.net/DaHvHW))
### Code (IL)
```msil
IL_0027:  ldstr      "Main"
IL_002c:  call       void [mscorlib]System.Console::WriteLine(string)
IL_0031:  nop
IL_0032:  ldstr      "Main"
IL_0037:  call       void [mscorlib]System.Console::WriteLine(string)
IL_003c:  nop
IL_003d:  ldstr      "x"
IL_0042:  call       void [mscorlib]System.Console::WriteLine(string)
IL_0047:  nop
IL_0048:  ldstr      "ToString"
IL_004d:  call       void [mscorlib]System.Console::WriteLine(string)
IL_0052:  nop
IL_0053:  ldstr      "x.ToString"
IL_0058:  call       void [mscorlib]System.Console::WriteLine(string)
IL_005d:  nop
IL_005e:  ret
```
---
## String Interpolation ([link](https://dotnetfiddle.net/rg59n6))
### Code (C# 1.0+)
```csharp
public class Person
{
  public string FirstName { get; protected set; }
  public string LastName { get; protected set; }
  public int Age { get; protected set; }

  public Person(string firstName, string lastName, int age) {
    // initialize
  }

  public override string ToString()
  {
    // long ..., error-prone (could be) ..., difficult to read ...
    return string.Format("Person(FirstName: {0}, LastName: {1}, Age: {2})",
                          FirstName, LastName, Age);
  }
}

public static void Main() {
  Person p = new Person("John", "Doe", 42);
  Console.WriteLine(p); // Person(FirstName: John, LastName: Doe, Age: 42)
}
```
---
## String Interpolation ([link](https://dotnetfiddle.net/rg59n6))
### Code (C# 6.0+) with **nameof** operator
```csharp
public class Person
{
  // other members ...

  public override string ToString()
  {
    // just added a new parameter and used nameof operator to make it refactoring-safe
    return string.Format("{0}(FirstName: {1}, LastName: {2}, Age: {3})",
                          nameof(Person), FirstName, LastName, Age);
  }
}

public static void Main() {
  Person p = new Person("John", "Doe", 42);
  Console.WriteLine(p); // Person(FirstName: John, LastName: Doe, Age: 42)
}
```
---
## String Interpolation ([link](https://dotnetfiddle.net/rg59n6))
### Code (C# 6.0+) with string interpolation
```csharp
public class Person
{
  // other members ...

  public override string ToString()
  {
    // String interpolation: $"Constant string value {computed value within curly brackets}"
    return $"{nameof(Person)}(FirstName: {FirstName}, LastName: {LastName}, Age: {Age})";
  }
}

public static void Main() {
  Person p = new Person("John", "Doe", 42);
  Console.WriteLine(p); // Person(FirstName: John, LastName: Doe, Age: 42)
}
```
---
## String Interpolation ([link](https://dotnetfiddle.net/rg59n6))
### Code (C# 6.0+)
```csharp
public class Person
{
  // other members ...

  public override string ToString()
  {
    // the compiler generates this:
    return string.Format("{0}(FirstName: {1}, LastName: {2}, Age: {3})",
                          nameof(Person), FirstName, LastName, Age);
  }
}

public static void Main() {
  Person p = new Person("John", "Doe", 42);
  Console.WriteLine(p); // Person(FirstName: John, LastName: Doe, Age: 42)
}
```
---
## String Interpolation ([link](https://dotnetfiddle.net/rg59n6))
### Code (IL)
```msil
.method public hidebysig virtual instance string ToString() cil managed {
  .maxstack  5
  .locals init (string V_0)
  IL_0000:  nop
  IL_0001:  ldstr      "{0}(FirstName: {1}, LastName: {2}, Age: {3})"
  IL_0006:  ldc.i4.4
  IL_0007:  newarr     [mscorlib]System.Object
  // ...
  IL_000e:  ldstr      "Person"
  // ...
  IL_0017:  call       instance string Program/Person::get_FirstName()
  // ...
  IL_0020:  call       instance string Program/Person::get_LastName()
  // ...
  IL_0029:  call       instance int32 Program/Person::get_Age()
  IL_002e:  box        [mscorlib]System.Int32
  IL_0033:  stelem.ref
  IL_0034:  call       string [mscorlib]System.String::Format(string, object[])
  IL_0039:  stloc.0
  // ...
} // end of method Person::ToString
```
---
## String Interpolation ([link](https://dotnetfiddle.net/rg59n6))
### Code (C# 2.0+)
```csharp
public static class HtmlBuilder
{
  public static string Tag(string name, params string[] children)
  {
    return String.Format("<{0}>{1}</{2}>",
                         name,
                         string.Join(string.Empty, children),
                         name);
  }
}
```
---
## String Interpolation ([link](https://dotnetfiddle.net/rg59n6))
### Code (C# 6.0)
```csharp
public static class HtmlBuilder
{
  public static string Tag(string name, params string[] children)
  {
    return $"<{name}>{string.Join(string.Empty, children)}</{name}>";
  }
}
```
---
## String Interpolation ([link](https://dotnetfiddle.net/rg59n6))
### Multiline string interpolation
Unfortunately `$"string literals"` doesn't support multi-line values ...

```csharp
  var value = $"first line
  second line
  third line
  ";  // compiler error
```
**but** they are not considered as string literals.
```csharp
  var value = $@"first line
  second line
  third line
  "; // absolutely fine @"" represents multiline string constonts
```
---
## String Interpolation ([link](https://dotnetfiddle.net/rg59n6))
### Code (C# 6.0+) with **nameof** operator
```csharp
public class Person
{
  // other members ...

  public override string ToString()
  {
    // just added a new parameter and 
    // used nameof operator to make it refactoring-safe
    return string.Format("{0}(FirstName: {1}, LastName: {2}, Age: {3})",
                          nameof(Person), FirstName, LastName, Age);
  }
}

public static void Main() {
  Person p = new Person("John", "Doe", 42);
  Console.WriteLine(p); // Person(FirstName: John, LastName: Doe, Age: 42)
}
```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 1.0+)
Let's assume we have a couple of **DTO** objects ...
```csharp
public class Person 
{
  public String FirstName { get; set; }
  public String LastName { get; set; }
  public Address Address { get; set; }
}

public class Address 
{
  // format: {City} - {Street address}
  public String Location { get; set; }
  public Int32 PostCode { get; set; }
}
```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 1.0+) - Naive style
... and we would like to get city name and its postcode of person:
```csharp
public static String GetCityName(Person person)
{
  // first approach ... ugh ...
  if (person != null)
  {
    if (person.Address != null)
    {
      if (person.Address.Location != null)
      {
        var city = person.Address.Location.Split('-').FirstOrDefault();
        if (city != null) return city;
      }
    }
  }

  return String.Empty;
}

```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 1.0+) - Collapsing conditions
```csharp
public static String GetCityName(Person person)
{
  // better(?) ... nope ...
  if (person != null &&
      person.Address != null &&
      person.Address.Location != null)
  {
    var city = person.Address.Location.Split('-').FirstOrDefault();
    if (city != null) return city;
  }

  return String.Empty;
}
```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 1.0+) - Clean Code
```csharp
private static String GetLocationOrDefault(Person person)
{
  if (person != null && person.Address != null && person.Address.City != null) 
  {
    return person.Address.City;
  }

  return null;
}

public static String GetCityName(Person person)
{
  // we can be clean coders:
  var location = GetLocationOrDefault(person);
  if (location != null)
  {
    var city = location.Split('-').FirstOrDefault();
    if (city != null) return city;
  }

  return String.Empty;
}
```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 1.0+) - Clean Code
```csharp
public static String GetCityName(Person person)
{
  // we can be clean coders:
  var hasLocation = (person != null && 
                     person.Address != null && 
                     person.Address.City != null);
  if (hasLocation)
  {
    var city = person.Address.City.Split('-').FirstOrDefault();
    if (city != null) return city;
  }

  return String.Empty;
}
```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 1.0+) - Clean Code
```csharp
private static Boolean HasLocation(Person person)
{
  return person != null && person.Address != null && person.Address.City != null;
}

public static String GetCityName(Person person)
{
  if (HasLocation(person))
  {
    var city = person.Address.City.Split('-').FirstOrDefault();
    if (city != null) return city;
  }

  return String.Empty;
}
```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 3.0+) - Extension methods
```csharp
public static class ObjectUtils
{
  public static TResult Safe<TElement, TResult>(this TElement element, 
                                                Func<TElement, TResult> selector)
    where TElement : class
  {
    if (element != null) return selector(element);
    return default(TResult);
  }
}
```
```csharp
public static String GetCityName(Person person)
{
  // transformed to expression which is more readable and maintanable
  var city = person.Safe(p => p.Address)
                   .Safe(address => address.Location)
                   .Safe(location => location.Split('-'))
                   .Safe(values => values.FirstOrDefault());

  return city ?? String.Empty;  
}
```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 3.0+) - Extension methods
```csharp
public static class ObjectUtils
{
  public static TResult Safe<TElement, TResult>(this TElement element, 
                                                Func<TElement, TResult> selector)
    where TElement : class
  {
    if (element != null) return selector(element);
    return default(TResult);
  }
}

// what about getting value types?
public static Int32 GetPostCode(Person person)
{
  var postcode = person.Safe(p => p.Address)
                       .Safe(address => address.PostCode);
  
  // if person or person.Address is null => default(Int32) = 0
  // not the best implementation ...
  return postcode;
}
```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 3.0+) - Extension methods
```csharp
public static TResult Safe<TElement, TResult>(this TElement element, 
                                              Func<TElement, TResult> selector)
    where TElement : class where TResult : class
  {
    if (element != null) return selector(element);
    return default(TResult);
  }

  public static TResult Safe<TElement, TResult>(this Nullable<TElement> element, 
                                                Func<TElement, TResult> selector)
    where TElement : struct
  {
    if (element.HasValue) return selector(element.Value);
    return default(TResult);
  }

  // to be continued on the next slide ...

```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 3.0+) - Extension methods
```csharp
  // to be continued from previous slide ...
    
  public static Nullable<TResult> SafeValue<TElement, TResult>(this TElement element, 
                                                               Func<TElement, TResult> selector)
    where TElement : class
    where TResult : struct
  {
    if (element != null) return selector(element);
    return null;
  }

  public static Nullable<TResult> SafeValue<TElement, TResult>(this Nullable<TElement> element, 
                                                               Func<TElement, TResult> selector)
    where TElement : struct
    where TResult : struct
  {
    if (element.HasValue) return selector(element.Value);
    return null;
  }
}
```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 3.0+) - Extension methods
```csharp
public static Nullable<Int32> GetPostCode(Person person)
{
  Nullable<Int32> postcode = person.Safe(p => p.Address)
                                   .SafeValue(address => address.PostCode);
  return postcode;
}
```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 6.0) - Null-conditional operator (?.)
```csharp
public static String GetCityName(Person person)
{
  var city = person?.Address?.Location?.Split('-')?.FirstOrDefault();
  return city ?? String.Empty;
}
```
However `Split(...)` returns array of strings and we can use `?` operator for indexing too:

```csharp
public static String GetCityName(Person person)
{
  // we can use at indexer properties
  var city = person?.Address?.Location?.Split('-')?[0];
  return city ?? String.Empty;
}
```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 6.0) - Null-conditional operator (?.) with functions
```csharp
// We can use for dereferencing and indexing. What about for calling a function?
Func<String, String> function = null;
function?("Hello World!"); // compile error
                           // it doesn't work unfortunately 

// but ... delegates and lambda functions are object too
function?.Invoke("Hello World!");   
```
---
## null-conditional operator ([link](https://dotnetfiddle.net/doKZI0))
### Code (C# 6.0) - Null-conditional operator (?.) with value types
Let's extend `Person` class with `BirthDate: DateTime` property:
```csharp
public class Person {
  public DateTime BirthDate { get; set; }
  // ...
}
```
```csharp
Person person = ...;

// even if BirthDate type is DateTime, result will be Nullable<DateTime>
Nullable<DateTime> result = person?.BirthDate?.AddYear(10);
```
---
## parameterless constructor in struct
### Code (C# 6.0-)
Before 6.0 version overriding parameterless constructor wasn't allowed
```csharp
public struct Point {
  public int X { get; private set; }
  public int Y { get; private set; }

  // error: [CS0568] Structs cannot contain explicit parameterless constructors
  public Point() {
    X = 0;
    Y = 0;
  }
}
```
---
## parameterless constructor in struct
### Code (C# 6.0)
In C# 6.0 everybody can define default parameterless constructors
```csharp
public struct Point {
  public int X { get; private set; }
  public int Y { get; private set; }

  // OK!
  public Point() {
    X = 0;
    Y = 0;
  }
}
```
---
## parameterless constructor in struct
### Code (C# 6.0) - Restrictions
All constructors for a struct should initialize all its fields otherwise compiler will give an error. 
```csharp
public struct Point {
  public int X { get; private set; }
  public int Y { get; private set; }

  // error: [CS0171] Field 'Point.Y' must be 
  // fully assigned before control is returned to the caller
  public Point() {
    X = 0;
    // Y = 0;
  }
}
```
---
## parameterless constructor in struct
### Code (C# 6.0) - Restrictions
What happens if we have to initialize just a few field in a bunch of members?
```csharp
public struct Point {
  public int X_0 { get; private set; }
  // ... 1 to N
  public int X_N { get; private set; }

  // lots of code and not to mention it is error-prone ...
  public Point() {
    X_0 = 0;
    // ... 1 to N
    X_N = 0;
  }
}
```
---
## parameterless constructor in struct
### Code (C# 1.0+) - Restrictions
In previous versions we could have called `this()` constructor before executing parametric constructor
```csharp
public struct Point {
  public int X_0 { get; private set; }
  // ... 1 to N
  public int X_N { get; private set; }

  public Point(int x_0) 
    : this() // call parameterless constructor to initialize other fields
  {
    X_0 = x_0;
  }
}
```
---
## parameterless constructor in struct
### Code (C# 6.0) - Restrictions
This technique doesn't work with parameterless constructors (obviously) ...
```csharp
public struct Point {
  public int X_0 { get; private set; }
  // ... 1 to N
  public int X_N { get; private set; }

  public Point() 
    : this() // uhm ... are you trying to write a recursive constructor??
  {
    X_N = 10;
  }
}
```
---
## Future

- [**Nullable reference types** ](https://github.com/dotnet/roslyn/issues/3910)
- pattern matching
- Language support for tuples
- Local functions
- Method contracts / code contracts
- `params IEnumerable`
- and a lot ...

More: [Design Notes](https://github.com/dotnet/roslyn/labels/Design%20Notes)

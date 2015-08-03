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
  if (point == null) {
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
    if (value == null) {
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
    : this() // calling parameterless constructor to initialize other fields
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
## parameterless constructor in struct
### Code (C# 6.0) - Restrictions
However we can solve this problem with a weird syntax ...
```csharp
public struct Point {
  public int X_0 { get; private set; }
  // ... 1 to N
  public int X_N { get; private set; }

  public Point() 
  {
    // this assignment is only allowed in constructors!
    this = default(Point);  // initialize fields to default value
    // X_0 = default(int);
    // ...
    // X_N = default(int);
    X_N = 10;
  }
}
```
---
## parameterless constructor in struct
### Code (C# 6.0) - default(T) != new T()

```csharp
public struct Point {
  public int X { get; private set; }
  public int Y { get; private set; }

  public Point() {
    X = 10;
    Y = 10;
  }

  public override string ToString() {
    return $"{nameof(Point)}({nameof(X)}: {X}, {nameof(Y)}: {Y})";
  }
}

public static void Main() {
  Console.WriteLine(new Point());     // Point(X: 10, Y: 10)
  Console.WriteLine(default(Point));  // Point(X: 0, Y: 0)

  Console.WriteLine(new Point() != default(Point)); // true
}
```
---
## parameterless constructor in struct
### Code (C# 6.0) - Sounds simple, but ...

```csharp
public static void Main() {
  // we need to call explicitly the parameterless constructor
  Point p1 = new Point();
  Console.WriteLine(p1);     // Point(X: 10, Y: 10)

  // parameterless constructors isn't called(!)
  Point p2;
  Console.WriteLine(p2);     // Point(X: 0, Y: 0)

  // equivalent to
  // Point p2 = default(Point);

  // why does it feel weird (?) because we get used to this ...
  int a;                  // default usage
  int b = new int();      // strange, but totally equivalent to int b;
  int c = default(int);   // equivalent to default usage

  Console.WriteLine(a);   // 0
  Console.WriteLine(b);   // 0
  Console.WriteLine(c);   // 0
}
```
---
## auto-property initializer
### Code (C# 1.0+) - properties are first-class citizens

```csharp
public class Person 
{
  private String name;

  // first-class citizen properties in C#
  public String Name 
  {
    get { return name; }
    set { name = value; }
  }
}
```
---
## auto-property initializer
### Code (C# 3.0+) 

```csharp
public class Person 
{
  public String Name { get; set; }
}
```
In the background compiler generates this:
```csharp
public class Person 
{
  private String <Name>k__BackingField;

  public String Name 
  {
    get { return <Name>k__BackingField; }
    set { <Name>k__BackingField = value; }
  }
}
```
---
## auto-property initializer
### Code (C# 1.0+) - read-only properties

```csharp
public class Person 
{
  private readonly String name;

  public String Name { 
    get { return name; }
  }

  public Person(String name) {
    this.name = name;
  }
}
```
---
## auto-property initializer
### Code (C# 6.0) - read-only properties

In C# 6.0 you can define read-only auto properties as below:

```csharp
public class Person 
{
  // no setter!
  public String Name { get; }

  public Person(String name) 
  {
    // you have to initialize its value in the constructor
    // and you cannot change its value later
    this.Name = name;
  }
}
```
---
## auto-property initializer
### Code (C# 6.0) - read-only properties

... and the compiler will generate the below code:
```csharp
public class Person 
{
  private readonly String <Name>k__BackingField;

  public String Name {
    get { return <Name>k__BackingField; }
  }

  public Person(String name) {
    this.Name = name;
  }
}
```
---
## auto-property initializer
### Code (C# 6.0) - initializing read-only properties

You can initialize its value where it is defined:

```csharp
public class Person 
{
  // passing a default value to it
  public String Name { get; } = String.Empty;

  public Person() { }

  public Person(String name) 
  {
    this.Name = name;
  }
}
```
---
## auto-property initializer
### Code (C# 6.0) - initializing read-only properties

... and the compiler will generate the below code:
```csharp
public class Person 
{
  private readonly String <Name>k__BackingField = String.Empty;

  public String Name {
    get { return <Name>k__BackingField; }
  }

  public Person() { }

  public Person(String name) {
    this.Name = name;
  }
}
```
---
## auto-property initializer
### Code (C# 6.0) - initializing properties

However you can easily set a default value to any property.

```csharp
public class Person 
{
  // passing a default value to it
  public String Name { get; set; } = String.Empty;
}
```
So defining any kind of properties depend on your coding style ...
---
## expression bodied functions and properties
### Code (C# 6.0) - read-only properties

Defining just one statement-length method or property could be verbose ... In C# 6.0 you can easily define expression bodied functions and properties

```csharp
public struct Point {
  public int X { get; private set; }
  public int Y { get; private set; }

  public Point() {
    X = 10;
    Y = 10;
  }
  

  // this ToString method is verbose ...
  public override string ToString() {
    return $"{nameof(Point)}({nameof(X)}: {X}, {nameof(Y)}: {Y})";
  }
}
```
---
## expression bodied functions and properties
### Code (C# 6.0)

Defining just one statement-length method or property could be verbose ... In C# 6.0 you can easily define expression bodied functions and properties

```csharp
public struct Point {
  public int X { get; private set; }
  public int Y { get; private set; }

  public Point() {
    X = 10;
    Y = 10;
  }
  
  // no curly braces and return statement, just one expression
  // its syntax comes from lambda function style
  public override string ToString() 
    => $"{nameof(Point)}({nameof(X)}: {X}, {nameof(Y)}: {Y})";
  
}
```
---
## expression bodied functions and properties
### Code (C# 6.0)

It can be used for properties as well ...

```csharp
using static System.Math;

public struct Point {
  public int X { get; private set; }
  public int Y { get; private set; }

  // compiler will generate a getter section to this property
  public double Distance 
  {
    get { return Sqrt(Pow(X, 2) + Pow(Y, 2)); }
  } 

  public Point() {
    X = 10;
    Y = 10;
  }
}
```
---
## expression bodied functions and properties
### Code (C# 6.0)

It can be used for properties as well ...

```csharp
using static System.Math;

public struct Point {
  public int X { get; private set; }
  public int Y { get; private set; }

  // compiler will generate a getter section to this property
  public double Distance => Sqrt(Pow(X, 2) + Pow(Y, 2));

  public Point() {
    X = 10;
    Y = 10;
  }
}
```
---
## index initializers
### Code (C# 1.0+)

Initializing a dictionary is not always convinient especially if we have to pass some predefined values into it ...

```csharp
var dictionary = new Dictionary<int, String>();

// clumsy and verbose way ...
// we have to use several method call statement to intialize it ...
dictionary.Add(1, "John Doe");
dictionary.Add(2, "Jane Doe");
dictionary.Add(3, "Bruce Wayne");
dictionary.Add(4, "Clark Kent");
```
---
## index initializers
### Code (C# 1.0+)

Initializing a dictionary is not always convinient especially if we have to pass some predefined values into it ...

```csharp
var dictionary = new Dictionary<int, String>();

// in several case (if indexer property has setter) you can use this: 
// we still have to use several assignment statement to intialize it ...
dictionary[1] = "John Doe";
dictionary[2] = "Jane Doe";
dictionary[3] = "Bruce Wayne";
dictionary[4] = "Clark Kent";
```
---
## index initializers
### Code (C# 3.0+)

In C# 3.0 collection initializers had been implemented into language.

```csharp
// dictionary is a collection too
// increase readability, but not always helpful unfortunatelly
// initializing a dictionary could be just one expression (!)
// which is helps initialize properties or fields in type body
var dictionary = new Dictionary<int, String>() {
  { 1, "John Doe" }
  { 2, "Jane Doe" }
  { 3, "Bruce Wayne" }
  { 4, "Clark Kent" }
};
```
---
## index initializers
### Code (C# 6.0)

In C# 6.0 index initializers helps to improve readability of initializing key-like data structures.

```csharp
// dictionary is a collection too
// increase readability, but not always helpful unfortunatelly
// initializing a dictionary could be just one expression (!)
// which is helps initialize properties or fields in type body
var dictionary = new Dictionary<int, String>() {
  // we're using indexer property of type to set the specified values
  [1] = "John Doe",
  [2] = "Jane Doe",
  [3] = "Bruce Wayne",
  [4] = "Clark Kent"
};
```
---
## index initializers
### Code (C# 3.0+)

**Why do we need** this new syntax if we have already had one very similar? 

```csharp
var dictionary = new Dictionary<int, String>() {
  { 1, "John Doe" }
  { 2, "Jane Doe" }
  { 3, "Bruce Wayne" }
  { 4, "Clark Kent" }
};
```
The compiler will generate this code in the background: 
```csharp
var dictionary = new Dictionary<int, String>();
dictionary.Add(1, "John Doe");
dictionary.Add(2, "Jane Doe");
dictionary.Add(3, "Bruce Wayne");
dictionary.Add(4, "Clark Kent");
```
---
## index initializers
### Code (C# 6.0)

**Why do we need** this new syntax if we have already had one very similar? 

```csharp
var dictionary = new Dictionary<int, String>() {
  [1] = "John Doe",
  [2] = "Jane Doe",
  [3] = "Bruce Wayne",
  [4] = "Clark Kent"
};
```
The compiler will generate this code in the background: 
```csharp
var dictionary = new Dictionary<int, String>();
dictionary[1] = "John Doe";
dictionary[2] = "Jane Doe";
dictionary[3] = "Bruce Wayne";
dictionary[4] = "Clark Kent";
```
---
## index initializers
### Code (C# 3.0+)

**Why do we need** this new syntax if we have already had one very similar? 

```csharp
public partial class DummyObject : ICollection
{
  public void Add(Int32 first, String second, Boolean third)
  {
    Console.WriteLine("{0},{1},{2}", first, second, third);
  }
}

public static void Main(string[] args)
{
  // this works fine, because DummyObject has Add() method
  // and implements ICollection interface
  DummyObject dummy = new DummyObject
  {
    { 1, "one", true },     // 1,one,true
    { 2, "two", false }     // 2,two,false
  };
}
```
---
## index initializers
### Code (C# 6.0)

Because semantically they aren't equivalent. Doesn't need to implement `ICollection` interface to use index initializers. It could be very handy when working with `JSON` objects. 

```csharp
public class DummyObject
{
  public Boolean this[int first, string second]
  {
    set { Console.WriteLine("{0},{1},{2}", first, second, value); }
  }
}

public static void Main(string[] args)
{
  // no need to implement ICollection interface(!)
  DummyObject dummy = new DummyObject
  {
    [1, "one"] = true,     // 1,one,true
    [2, "two"] = false     // 2,two,false
  };
}
```
---
## exception filters
### Code (C# 1.0+)

Basic idea: capture exception with given condition

```csharp
public void Foo() 
{
  try 
  {
    DoVeryImportantThing();
  }  
  catch (CustomException ex)
  {
    // can we retry?
    if (!ex.CanRetry) {
      throw; // or throw ex;
    }

    DoSomeRecovering();
  }
}

```
---
## exception filters
### Code (C# 6.0)

Exception filters can manage which exception is wanted to catch and handle.

```csharp
public void Foo() 
{
  try 
  {
    DoVeryImportantThing();
  }  
  catch (CustomException ex) when (ex.CanRetry)
  {
    DoSomeRecovering();
  }
}
```
---
## exception filters
### Code (C# 6.0)

Pretty good, but is it just syntactic sugar? **NO**. Exception filters check condition before catching the exception so they **don't unwind the stack**. This subtle difference can help to investigate bugs on server-side. 

```csharp
public void Foo() 
{
  try 
  {
    DoVeryImportantThing();
  }  
  // exception filters don't unwind the stack
  catch (CustomException ex) when (ex.CanRetry) 
  {
    DoSomeRecovering();
  }
}

```
---
## exception filters
### Code (C# 6.0)

Exception filters have a strange side-effect:

```csharp
public static bool LogException(Exception ex, string message) {
  Console.WriteLine($"Exception has occured: {ex.Message}; {message}");
  return true;
}

public void Foo() {
  try {
    DoVeryImportantThing();
  }  
  // exception filters don't unwind the stack
  catch (CustomException ex) when (LogException(ex, "Logging my custom exception")) {
    DoSomeRecovering();
  }
}

```
> *How to implement pattern matching with exception filters* ([link](http://tomasp.net/blog/2015/csharp-pattern-matching/))
---
## Future

- [**Nullable reference types** ](https://github.com/dotnet/roslyn/issues/3910)
- [pattern matching](http://www.infoq.com/news/2014/08/Pattern-Matching)
- [Language support for tuples](https://github.com/dotnet/roslyn/issues/347)
- [Local functions](https://github.com/dotnet/roslyn/issues/259)
- [Method contracts / code contracts](https://github.com/dotnet/roslyn/issues/119)
- [`params IEnumerable`](https://github.com/dotnet/roslyn/issues/36)
- [and a lot ...](https://github.com/dotnet/roslyn/issues/2136)

More: [Design Notes](https://github.com/dotnet/roslyn/labels/Design%20Notes)
---
## References

- [.NET fiddle](https://dotnetfiddle.net/)
- [Roslyn official page](http://roslyn.io/)
- [volatile read](http://www.volatileread.com/Thread/Browse)
- [New Features in C# 6.0 (video)](https://www.youtube.com/playlist?list=PL8m4NUhTQU4-5RmHwMn5LpnEJbNCSVbAc)
- and Google
---
## Q & A

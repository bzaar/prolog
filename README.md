# XProlog

## What is XProlog? 

XProlog.dll is a library that allows you to run Prolog code in your .NET applications. 
 Prolog is particularly well suited for creating parsers for both computer and natural languages. 
 Its DCG notation very much resembles BNF but is more powerful and allows for ambiguous, context-sensitive grammars. 
 

## What is Prolog good for?

Prolog is considered by many an 'artificial intelligence language' because unlike conventional programming languages 
 it does not require the programmer to provide a recipe-style solution for a problem (take A, add B, put result into C); 
 instead, a Prolog program is a collection of facts and rules describing the problem
 (such as the rules of the game of chess) and it is Prolog's task to figure out the solution for a *goal* set by the programmer
 (e.g. I want to place 8 queens on the chessboard so that none beats the others).
 

How does Prolog figure out the solution? You may feel a bit underwhelmed but the answer is very simple. It is just *brute force search*.
 It is one of the few things that computers are very good at. In fact, much, much better than us humans. A billion times better.
 True, it's very far from how humans think but for some problems brute force does get us from A to B, and sometimes even to such C's which we could never have thought of.
 Here's just a few examples of problems that Prolog has been used to solve:
 

*   Toy problems such as Sudoku, the N queens problem or the farmer, wolf, goat and cabbage problem.
 
*   Querying large datasets with constraints, i.e. the same task you use SQL for. Prolog and SQL are quite different syntactically but in essence they are not that dissimilar. 
 SQL is also declarative in the sense that you specify what you want to get but don't tell it how.
 It is up to the database engine to decide which tables to query first, how to join the results, i.e. to build what database developers call an execution plan.
 In fact, there is a specialized version of Prolog for database programming called Datalog.
 
 
*   Text parsing (think regex, yacc or ANTLR).
 

It is the problem of natural language text parsing that brought XProlog to life.


## How this project started

I was working on a natural language processing project and was looking for a tool to parse natural language sentences.  Prolog immediately sprung to mind as I had done some toy projects with it before.  I have googled for the latest and greatest that was out there and ended up downloading and installing SWI-Prolog.  It had a workable IDE with a debugger.  DCG syntax support, cool stuff.  I've tried implementing a few natural language constructs and all went really well until I got hit by left recursion.  You see, some natural language constructs are naturally left-recursive ("my brother's best mate's birthday party") and any ISO-compliant Prolog just heads off into an infinite loop on such input.  Another round of googling brought me a solution, see the section on "Accommodating left recursion in top-down parsing" in the [Wikipedia article on left recursion](http://en.wikipedia.org/wiki/Left_recursion#Accommodating_left_recursion_in_top-down_parsing).  OK, the solution is there, that's good.  How do I use it?


## What makes XProlog different / Main principles

*   Not trying to be a full-fledged programming language. It's just a library.
 
*   Fully declarative: no cuts or fails; leave the procedural aspects to the execution engine.

*   Use-case driven: every feature is there for a purpose.


## How does XProlog compare to other parsing tools?

Yet another parser? What about 
 [yacc][1] 
 and [ANTLR][2] 
 and [regex][3]? 
 And [numerous others][4]?
 

1.  ANTLR and yacc are ‘parser generators’, i.e. they generate Java / C++ code that can only parse one grammar. 
 XProlog removes this intermediate step from the pipeline. 
 
 
2.  XProlog can parse ambiguous texts, i.e. texts that allow more than one interpretation, such as natural language texts.
 It is the [ state-of-the-art way ][5] of dealing with natural language ambigities: 
 first you build all possible parse trees and then choose one based on its weight/probability score.
 
 
## How to Run this Project

Most of this project is done as series of unit tests.  To get an idea of how to use the Prolog compiler, check out the test 'FarmerProblemHasTwoDistinctSolutions'.

You can also run up the IDE (`Prolog.IDE.csproj`) which currently features a simple debugger allowing you to step through your program while watching the solution tree being built.


## How does it work?

You give it a grammar and a text file to parse and get back a parse tree. Example code:

```csharp
    Program program = new Prolog.Compiler ().Compile ("program.pl");

    var solutions = new Runtime.Engine ().Run (program)

    foreach (var solution in solutions)
    {
        Console.WriteLine (solution.ToString ());
    }
```

## Achievements so far:
    
*   Compiles itself: the XProlog parser is written in XProlog!
 
*   Left recursion handling and infinite loop avoidance. 
 Some natural language constructs are naturally left-recursive: 
 *my brother’s girlfriend’s roommate’s best friend*. 
 Standard Prolog is notoriously bad at handling left recursion. 
 XProlog has a solution for that.
 
 
*   Walk the solution tree – see exactly how the Prolog engine arrived at a solution. Or if you are writing a parser, your solution tree will be your parse tree as well.
 
*   Seamless .NET interoperability. Easily write your own predicates in a .NET language to query external databases or web services or simply do things that are best described imperatively.  See `ExternalPredicateDeclaration`, `ExternalPredicateDefinition`.
 
*   Break/resume – once a solution is found, the .NET host can stop the Prolog engine or continue searching for more solutions.
 
*   Tracing.
 
*   Full and native Unicode support.
 
*   Prolog code is independent of the hosting language / platform.
 
*   Componentized: only use the building blocks your project needs (reference a dependencies chart here)
 
*   Extensible: any component (see the dependencies chart again) can be swapped for your own: if you don’t like the standard depth-first left-to-right engine, go ahead and write your own. There are plenty of possibilities to explore there: implement tabling (intermediate result caching), concurrency, best goal order detection...
 
*   Lean: XProlog runtime DLL is just 31K. That's all you need to run a compiled Prolog program.
 
*   Thread safe: no global variables anywhere.
 
*   Unit test driven from inception.
 
    
## Road map:
    
*   Generate C# classes to represent the parse tree for easy and type-safe navigation.
 
*   Create a custom binary serializer for compiled Prolog. Currently BinaryFormatter is used that is 
 
    *   very chatty (about 30x more than it needs to be),
 
    *   not version-safe (consider renaming a class) and 
 
    *   not cross-plaform.
 
 
*   Allow for the option to strip symbols from compiled Prolog. This will: 
 
    *   Reduce your program's binary footprint.
 
    *   Protect your intellectual property.
 
 
*   More detailed compilation error reporting. Currently you just get a yes/no from the compiler.
 
*   [Higher order programming][6] support (pass predicates around as arguments to other predicates).
 
*   IDE (Eclipse plugin?). Like any program, a Prolog program needs to be written, run, tested, traced and debugged. 
Preferably, in a comfy IDE with syntax highlighting and code completion.
Current status: there is currently a mini-debugger that shows the current solution tree and allows you to step through the program or skip to the next choice point.
 
 
*   Port the runtime to other languages, C++ and Java being at the top of the list.
 
*   Implement the anonymous variable (_) ([#1](https://github.com/bzaar/prolog/issues/1)).
 

    
 ![Module Dependencies][7]
    
    
 [Download v 1.0][8]

 [1]: http://dinosaur.compilertools.net/yacc/index.html
 [2]: http://www.antlr.org/
 [3]: http://en.wikipedia.org/wiki/Regular_expression
 [4]: http://morpher.ru/Prolog/Links.aspx
 [5]: http://en.wikipedia.org/wiki/Stochastic_context-free_grammar
 [6]: http://en.wikibooks.org/wiki/Prolog/Higher_Order_Programming
 [7]: http://morpher.ru/Prolog/Dependencies.png
 [8]: http://morpher.ru/Prolog/Prolog.zip  


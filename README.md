This Visual Studio solution was developed in dotnet 9. 

This solution uses ANTLR4 to generate lexer and parser. JRE 11 or higher is required by ANTLR4 ([ref](https://github.com/antlr/antlr4/blob/master/doc/getting-started.md)). You may download JRE 11 from https://adoptium.net/temurin/releases/?version=11&package=jre.

## Projects (Folders)

- **GeneratorCalculation**: a library of the coroutine composition engine
- **GeneratorCalculationTests**: tests for the coroutine composition engine
- **RequirementAnalysis**: a library to analyze REModel requirement file (Gu, Qiqi, and Wei Ke. "[Typing requirement model as coroutines](https://ieeexplore.ieee.org/abstract/document/10387433/)" IEEE Access 12 (2024): 8449-8460.)
- **RequirementAnalysisTests**: tests for the requirement analysis library
- **Go**: an executable to detect deadlocks in Go source code
- **GoTests**: tests for the Go deadlock detecter


## Compile and Test All Projects

Open Visual Studio developer prompt

```
dotnet restore GeneratorCalculation.sln
dotnet build --no-restore GeneratorCalculation.sln
dotnet test --no-restore --verbosity normal GeneratorCalculation.sln
```

You may also build the release versions of these projects.

## Deadlock Detection for Go

After compiling the projects, go to `Go\bin\Debug\net9.0` (or `Go\bin\Release\net9.0`). `GoAnalysis.exe` is the executable.

The command line is very simple: `GoAnalysis.exe path`.

You can use the companion Go test files to get a taste of GoAnalysis.

```console
$ Go\bin\Debug\net9.0\GoAnalysis.exe GoTests\basic.go
sum: ~>[+Int]
main: ~>[+Start(sum); +Start(sum); -Int; -Int]
Iterate 0 and check convergence
sum: ~>[+Int]
main: ~>[+Start(sum); +Start(sum); -Int; -Int]
D:\coroutine-program\Go\bin\Debug\net9.0\appsettings.json doesn't exist.
compose(main:   main: [+Start(sum)~~+Start(sum)~~-Int~~-Int]
)
where sum=~>[+Int],
main=~>[+Start(sum); +Start(sum); -Int; -Int]
.

main:   main: [+Start(sum)~~+Start(sum)~~-Int~~-Int] --> main: [+Start(sum)~~-Int~~-Int], yielded: [+Int]
main:   main: [+Start(sum)~~-Int~~-Int] --> main: [-Int~~-Int], yielded: [+Int]
main:   main: [-Int~~-Int]  -- Not ready to yield
:       [+Int] --> [], yielded: Int

:       [] -- Cannot receive Int
:       [+Int] -- Cannot receive Int
main:   main: [-Int~~-Int] can receive Int.
main:   main: [-Int]  -- Not ready to yield
:       [+Int]

: Solver[0]
       reached the simplest form. Remove from the list.
:       [] -- Cannot receive Int
main:   main: [-Int] can receive Int.

Composition order:
main ->
main ->
[] ->
main ->
[] ->
main
Composition result is []
```

[The standard .NET logging config file](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/overview?tabs=command-line#configure-logging-without-code) is supported, which should be written in `appsettings.json`. A template is provided at `GeneratorCalculation\appsettings.json.template`.

### Baseline Tools

#### GoDDaR

Consult https://github.com/JorgeGCoelho/GoDDaR for its installation steps. After the tool is installed, run GoDDaR on all tests files in the GoTest folder.

For example, GoDDaR crashes with basic.go.

```
$ dune exec -- GoDDaR go ~/coroutine-program/GoTests/basic.go
{"level":"warn","ts":1789480311.6275387,"caller":"migoinfer/instr.go:281","msg":"instr FieldAddr: &t3.pfd [#0] is not a struct\t*internal/poll.FD\n\t-"}
{"level":"warn","ts":1789480311.6275904,"caller":"migoinfer/instr.go:281","msg":"instr FieldAddr: &t3.pfd [#0] is not a struct\t*internal/poll.FD\n\t-"}
{"level":"warn","ts":1789480311.627614,"caller":"migoinfer/instr.go:281","msg":"instr FieldAddr: &t3.pfd [#0] is not a struct\t*internal/poll.FD\n\t-"}
{"level":"warn","ts":1789480311.6280894,"caller":"migoinfer/instr.go:281","msg":"instr FieldAddr: *t61 is not a struct\t*os.file\n\t/usr/local/go/src/os/file_unix.go:235:6"}
{"level":"warn","ts":1789480311.6284907,"caller":"migoinfer/instr.go:281","msg":"instr FieldAddr: *t50 is not a struct\t*os.file\n\t/usr/local/go/src/os/file_unix.go:219:6"}
{"level":"warn","ts":1789480311.6286674,"caller":"migoinfer/instr.go:281","msg":"instr FieldAddr: *t42 is not a struct\t*os.file\n\t/usr/local/go/src/os/file_unix.go:233:18"}
{"level":"warn","ts":1789480311.6327064,"caller":"migoinfer/instr.go:281","msg":"instr FieldAddr: parameter t : *Type is not a struct\t*internal/abi.Type\n\t/usr/local/go/src/internal/abi/type.go:169:44"}
Fatal error: exception Dlock.MiGo_to_CCS.Fail("Recursive call (main.sum#1)")
```

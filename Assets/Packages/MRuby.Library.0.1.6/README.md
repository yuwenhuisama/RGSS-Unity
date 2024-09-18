# mruby-for-dotnet

This is a mruby-wrapper for .NET, current for Windows and will come to other platform soon.

## How to Install

From nuget: https://www.nuget.org/packages/MRuby.Library/

```bash
dotnet add package MRuby.Library --version 0.1.6
```

## How to Use

A simple code to embed mruby into C# code.

```csharp
using MRuby.Library

// create ruby env
using var state = Ruby.Open();

// ruby code string
var code = @"
def hello
  'Hello, World!'
end

hello
";

// compile code, run and get the result
using var compiler = state.NewCompiler();
var res = compiler.LoadString(code);

// unbox the ruby value
var unboxed = res.ToString();
Assert.Equal("Hello, World!", unboxed);

```

## How to Build

1. `git submodule update --init --recursive`
2. `./build-mruby-win.bat` (for Windows, run this command under `VS x64 Command Prommpt)` or `./build-mruby-linux.sh` 
   for (*nix) or `./build-mruby-mac.sh` for macos 
3. `cd ../mruby-shared`
4. `xmake f -m releasedbg`
5. `xmake`
6. `cd ../mruby-wrapper`
7. `dotnet build --configuration Release`
8. `dotnet test`

## Status

- [X] 100% Unittest Coverage
- [X] Nuget package
- [X] Support Linux
- [X] Support macOS
- [ ] Unity integral test
- [ ] Support Android
- [ ] Support iOS
- [ ] Documentation

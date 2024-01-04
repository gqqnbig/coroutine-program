echo "%ProjectDir%"
cd /d "%ProjectDir%"
set CLASSPATH=.;%ProjectDir%..\antlr-4.13.0-complete.jar;%CLASSPATH%

call ..\antlr4.bat -Dlanguage=CSharp -visitor -o generated -package GoLang.Antlr GoLexer.g4 GoParser.g4


#!/bin/bash

cd "${ProjectDir}"
export CLASSPATH=".:${ProjectDir}../antlr-4.13.0-complete.jar:${CLASSPATH}"
# By default, aliases are not expanded when the shell is not interactive.
shopt -s expand_aliases
alias antlr4='java org.antlr.v4.Tool'

antlr4 -Dlanguage=CSharp -listener -visitor -o generated -package GoLang.Antlr GoLexer.g4 GoParser.g4
# antlr4.bat -Dlanguage=CSharp -no-listener -visitor -o generated -package DiffSyntax.Antlr  JavaLexer.g4 JavaParser.g4


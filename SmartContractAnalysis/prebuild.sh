#!/bin/bash

cd "${ProjectDir}"
export CLASSPATH=".:${ProjectDir}antlr-4.13.0-complete.jar:${CLASSPATH}"
alias antlr4='java org.antlr.v4.Tool'

antlr4 -Dlanguage=CSharp -no-listener -visitor -o generated -package DiffSyntax.Antlr REModel.g4
# antlr4.bat -Dlanguage=CSharp -no-listener -visitor -o generated -package DiffSyntax.Antlr  JavaLexer.g4 JavaParser.g4


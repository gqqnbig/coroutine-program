grammar REModel;
import OCL;


letExpression
    : 'let' ID ':' type 'in' expression
    ;

reFactor2Expression:
   | factor2Expression '->any' '(' identifier ':' type  '|' expression ')'
   | factor2Expression;


contractDefinition
      : ('static')? 'Contract' ID '::' ID
        '(' parameterDeclarations? ')' ':' type
        '{'
        ('definition:' ID ':' type '=' reFactor2Expression)?
        ('precondition:' expression)?
        ('postcondition:' expression)?
        '}';



LINE_COMMENT:       '//' ~[\r\n]*    -> channel(HIDDEN);
grammar REModel;
import OCL;

basicExpression
    : 'null'
    | basicExpression '.' ID '@pre'?
    | basicExpression '(' expressionList? ')'
    | basicExpression '[' expression ']'
    | INT
    | FLOAT_LITERAL
    | STRING_LITERAL
    | ID
    | '(' expression ')'
    ;

letExpression
    : 'let' ID ':' type 'in' expression
    ;

reFactor2Expression:
   | factor2Expression '->any' '(' identifier ':' type  '|' expression ')'
   | factor2Expression;

definition:
    ID ':' type '=' reFactor2Expression;

definitions:
    'definition:' (definition ',')* definition;

precondition:
    'precondition:' expression;

postcondition:
    'postcondition:' expression;

contractDefinition
      : ('static')? 'Contract' ID '::' ID
        '(' parameterDeclarations? ')' ':' type
        '{'
        definitions?
        precondition?
        postcondition?
        '}';



LINE_COMMENT: '//' ~[\r\n]*    -> channel(HIDDEN);
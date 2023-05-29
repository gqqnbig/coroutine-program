grammar REModel;
import OCL;


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
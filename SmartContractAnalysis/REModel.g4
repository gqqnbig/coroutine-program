grammar REModel;
import OCL;

type
    : 'Sequence' '(' type ')'
    | 'Set' '(' type ')'
    | 'Bag' '(' type ')'
    | 'OrderedSet' '(' type ')'
    | 'Map' '(' type ',' type ')'
    | 'Function' '(' type ',' type ')'
    | ID '[' ID ('|' ID)* ']'
    | ID
    ;

statement : ;

basicExpression
    : 'null'
    | basicExpression '.' ID '@pre'?
    | basicExpression '::' ID
    | basicExpression '(' ')'
    | basicExpression '(' expressionList ')'
    | basicExpression '[' expression ']'
    | INT
    | FLOAT_LITERAL
    | STRING_LITERAL
    | ID
    | '(' expression ')'
    ;

conditionalExpression
    : 'if' expression 'then' expression ('else' expression)? 'endif'
    ;

letExpression
    : 'let' (ID ':' type ',')* ID ':' type 'in' expression
    ;

//Special syntax in REModel
reFactor2Expression:
   | factor2Expression '->any' '(' identifier ':' type  '|' expression ')'
   | factor2Expression '->forAll' '(' identifier ':' type '|' expression ')'
   | factor2Expression '->select' '(' identifier ':' type '|' expression ')'
   | factor2Expression '->collect' '(' identifier ':' type '|' expression ')'
   | factor2Expression '->exists' '(' identifier ':' type '|' expression ')'
   | factor2Expression;

factorExpression
    : reFactor2Expression ('*' | '/' | 'mod' | 'div') reFactor2Expression
    | reFactor2Expression
    ;

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
        '(' parameterDeclarations? ')' (':' type)?
        '{'
        definitions?
        precondition?
        postcondition?
        '}';



LINE_COMMENT: '//' ~[\r\n]*    -> channel(HIDDEN);
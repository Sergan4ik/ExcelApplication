grammar ExcelApplication;

/*
* Parser Rules
*/

compileUnit : expression EOF;
expression :
	LPAREN expression RPAREN #ParenthesizedExpr
	|expression EXPONENT expression #ExponentialExpr
	|expression operatorToken=(MOD | DIV) expression #ModDivExpr
	|expression operatorToken=(MULTIPLY | DIVIDE) expression #MultiplicativeExpr
	|expression operatorToken=(ADD | SUBTRACT) expression #AdditiveExpr
	|operatorToken=(MMIN | MMAX) LPAREN expression (DESP expression)* RPAREN #MminMmaxExpr
	|operatorToken=(INC| DEC) LPAREN expression RPAREN #IncDecExpr
	|NUMBER #NumberExpr
	|IDENTIFIER #IdentifierExpr
	;

/*
* Lexer Rules
*/

NUMBER : INT (',' INT)?;
IDENTIFIER : [a-zA-Z]+[1-9][0-9]+;
INT : ('0'..'9')+;

INC: 'inc';
DEC: 'dec';
EXPONENT : '^';
MULTIPLY : '*';
DIVIDE : '/';
SUBTRACT : '-';
ADD : '+';
MMIN : 'MMIN';
MMAX : 'MMAX';
MOD : 'MOD';
DIV : 'DIV';
DESP : ',';
LPAREN : '(';
RPAREN : ')';
WS : [ \t\r\n] -> channel(HIDDEN);
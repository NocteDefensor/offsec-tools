using System.Diagnostics;
using System.Text;

namespace winPEAS._3rdParty.SQLite.src
{
  public partial class CSSQLite
  {
    /*
    ** 2001 September 15
    **
    ** The author disclaims copyright to this source code.  In place of
    ** a legal notice, here is a blessing:
    **
    **    May you do good and not evil.
    **    May you find forgiveness for yourself and forgive others.
    **    May you share freely, never taking more than you give.
    **
    *************************************************************************
    ** An tokenizer for SQL
    **
    ** This file contains C code that splits an SQL input string up into
    ** individual tokens and sends those tokens one-by-one over to the
    ** parser for analysis.
    **
    ** $Id: tokenize.c,v 1.163 2009/07/03 22:54:37 drh Exp $
    **
    *************************************************************************
    **  Included in SQLite3 port to C#-SQLite;  2008 Noah B Hart
    **  C#-SQLite is an independent reimplementation of the SQLite software library
    **
    **  $Header$
    *************************************************************************
    */
    //#include "sqliteInt.h"
    //#include <stdlib.h>

    /*
    ** The charMap() macro maps alphabetic characters into their
    ** lower-case ASCII equivalent.  On ASCII machines, this is just
    ** an upper-to-lower case map.  On EBCDIC machines we also need
    ** to adjust the encoding.  Only alphabetic characters and underscores
    ** need to be translated.
    */
#if SQLITE_ASCII
    //# define charMap(X) sqlite3UpperToLower[(unsigned char)X]
#endif
#if SQLITE_EBCDIC
//# define charMap(X) ebcdicToAscii[(unsigned char)X]
//const unsigned char ebcdicToAscii[] = {
/* 0   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F */
//   0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  /* 0x */
//   0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  /* 1x */
//   0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  /* 2x */
//   0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  /* 3x */
//   0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  /* 4x */
//   0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  /* 5x */
//   0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 95,  0,  0,  /* 6x */
//   0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  /* 7x */
//   0, 97, 98, 99,100,101,102,103,104,105,  0,  0,  0,  0,  0,  0,  /* 8x */
//   0,106,107,108,109,110,111,112,113,114,  0,  0,  0,  0,  0,  0,  /* 9x */
//   0,  0,115,116,117,118,119,120,121,122,  0,  0,  0,  0,  0,  0,  /* Ax */
//   0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  /* Bx */
//   0, 97, 98, 99,100,101,102,103,104,105,  0,  0,  0,  0,  0,  0,  /* Cx */
//   0,106,107,108,109,110,111,112,113,114,  0,  0,  0,  0,  0,  0,  /* Dx */
//   0,  0,115,116,117,118,119,120,121,122,  0,  0,  0,  0,  0,  0,  /* Ex */
//   0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  /* Fx */
//};
#endif

    /*
** The sqlite3KeywordCode function looks up an identifier to determine if
** it is a keyword.  If it is a keyword, the token code of that keyword is
** returned.  If the input is not a keyword, TK_ID is returned.
**
** The implementation of this routine was generated by a program,
** mkkeywordhash.h, located in the tool subdirectory of the distribution.
** The output of the mkkeywordhash.c program is written into a file
** named keywordhash.h and then included into this source file by
** the #include below.
*/
    //#include "keywordhash.h"


    /*
    ** If X is a character that can be used in an identifier then
    ** IdChar(X) will be true.  Otherwise it is false.
    **
    ** For ASCII, any character with the high-order bit set is
    ** allowed in an identifier.  For 7-bit characters,
    ** sqlite3IsIdChar[X] must be 1.
    **
    ** For EBCDIC, the rules are more complex but have the same
    ** end result.
    **
    ** Ticket #1066.  the SQL standard does not allow '$' in the
    ** middle of identfiers.  But many SQL implementations do.
    ** SQLite will allow '$' in identifiers for compatibility.
    ** But the feature is undocumented.
    */
#if SQLITE_ASCII
    static bool[] sqlite3IsAsciiIdChar = {
/* x0 x1 x2 x3 x4 x5 x6 x7 x8 x9 xA xB xC xD xE xF */
false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false,  /* 2x */
true, true, true, true, true, true, true, true, true, true, false, false, false, false, false, false,  /* 3x */
false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,  /* 4x */
true, true, true, true, true, true, true, true, true, true, true, false, false, false, false, true,  /* 5x */
false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,  /* 6x */
true, true, true, true, true, true, true, true, true, true, true, false, false, false, false, false,  /* 7x */
};
    //#define IdChar(C)  (((c=C)&0x80)!=0 || (c>0x1f && sqlite3IsAsciiIdChar[c-0x20]))
#endif
#if SQLITE_EBCDIC
//const char sqlite3IsEbcdicIdChar[] = {
/* x0 x1 x2 x3 x4 x5 x6 x7 x8 x9 xA xB xC xD xE xF */
//    0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0,  /* 4x */
//    0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0,  /* 5x */
//    0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0,  /* 6x */
//    0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0,  /* 7x */
//    0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0,  /* 8x */
//    0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 1, 0,  /* 9x */
//    1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0,  /* Ax */
//    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  /* Bx */
//    0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1,  /* Cx */
//    0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1,  /* Dx */
//    0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1,  /* Ex */
//    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0,  /* Fx */
//};
//#define IdChar(C)  (((c=C)>=0x42 && sqlite3IsEbcdicIdChar[c-0x40]))
#endif


    /*
** Return the length of the token that begins at z[0].
** Store the token type in *tokenType before returning.
*/
    static int sqlite3GetToken( string z, int iOffset, ref int tokenType )
    {
      int i;
      byte c = 0;
      switch ( z[iOffset + 0] )
      {
        case ' ':
        case '\t':
        case '\n':
        case '\f':
        case '\r':
          {
            testcase( z[0] == ' ' );
            testcase( z[0] == '\t' );
            testcase( z[0] == '\n' );
            testcase( z[0] == '\f' );
            testcase( z[0] == '\r' );
            for ( i = 1 ; z.Length > iOffset + i && sqlite3Isspace( z[iOffset + i] ) ; i++ ) { }
            tokenType = TK_SPACE;
            return i;
          }
        case '-':
          {
            if ( z.Length > iOffset + 1 && z[iOffset + 1] == '-' )
            {
              for ( i = 2 ; z.Length > iOffset + i && ( c = (byte)z[iOffset + i] ) != 0 && c != '\n' ; i++ ) { }
              tokenType = TK_SPACE;
              return i;
            }
            tokenType = TK_MINUS;
            return 1;
          }
        case '(':
          {
            tokenType = TK_LP;
            return 1;
          }
        case ')':
          {
            tokenType = TK_RP;
            return 1;
          }
        case ';':
          {
            tokenType = TK_SEMI;
            return 1;
          }
        case '+':
          {
            tokenType = TK_PLUS;
            return 1;
          }
        case '*':
          {
            tokenType = TK_STAR;
            return 1;
          }
        case '/':
          {
            if ( iOffset + 2 >= z.Length || z[iOffset + 1] != '*' )
            {
              tokenType = TK_SLASH;
              return 1;
            }
            for ( i = 3, c = (byte)z[iOffset + 2] ; iOffset + i < z.Length && ( c != '*' || ( z[iOffset + i] != '/' ) && ( c != 0 ) ) ; i++ ) { c = (byte)z[iOffset + i]; }
            if ( iOffset + i == z.Length ) c = 0;
            if ( c != 0 ) i++;
            tokenType = TK_SPACE;
            return i;
          }
        case '%':
          {
            tokenType = TK_REM;
            return 1;
          }
        case '=':
          {
            tokenType = TK_EQ;
            return 1 + ( z[iOffset + 1] == '=' ? 1 : 0 );
          }
        case '<':
          {
            if ( ( c = (byte)z[iOffset + 1] ) == '=' )
            {
              tokenType = TK_LE;
              return 2;
            }
            else if ( c == '>' )
            {
              tokenType = TK_NE;
              return 2;
            }
            else if ( c == '<' )
            {
              tokenType = TK_LSHIFT;
              return 2;
            }
            else
            {
              tokenType = TK_LT;
              return 1;
            }
          }
        case '>':
          {
            if ( z.Length > iOffset + 1 && ( c = (byte)z[iOffset + 1] ) == '=' )
            {
              tokenType = TK_GE;
              return 2;
            }
            else if ( c == '>' )
            {
              tokenType = TK_RSHIFT;
              return 2;
            }
            else
            {
              tokenType = TK_GT;
              return 1;
            }
          }
        case '!':
          {
            if ( z[iOffset + 1] != '=' )
            {
              tokenType = TK_ILLEGAL;
              return 2;
            }
            else
            {
              tokenType = TK_NE;
              return 2;
            }
          }
        case '|':
          {
            if ( z[iOffset + 1] != '|' )
            {
              tokenType = TK_BITOR;
              return 1;
            }
            else
            {
              tokenType = TK_CONCAT;
              return 2;
            }
          }
        case ',':
          {
            tokenType = TK_COMMA;
            return 1;
          }
        case '&':
          {
            tokenType = TK_BITAND;
            return 1;
          }
        case '~':
          {
            tokenType = TK_BITNOT;
            return 1;
          }
        case '`':
        case '\'':
        case '"':
          {
            int delim = z[iOffset + 0];
            testcase( delim == '`' );
            testcase( delim == '\'' );
            testcase( delim == '"' );
            for ( i = 1 ; ( iOffset + i ) < z.Length && ( c = (byte)z[iOffset + i] ) != 0 ; i++ )
            {
              if ( c == delim )
              {
                if ( z.Length > iOffset + i + 1 && z[iOffset + i + 1] == delim )
                {
                  i++;
                }
                else
                {
                  break;
                }
              }
            }
            if ( ( iOffset + i == z.Length && c != delim ) || z[iOffset + i] != delim )
            {
              tokenType = TK_ILLEGAL;
              return i + 1;
            }
            if ( c == '\'' )
            {
              tokenType = TK_STRING;
              return i + 1;
            }
            else if ( c != 0 )
            {
              tokenType = TK_ID;
              return i + 1;
            }
            else
            {
              tokenType = TK_ILLEGAL;
              return i;
            }
          }
        case '.':
          {
#if !SQLITE_OMIT_FLOATING_POINT
            if ( !sqlite3Isdigit( z[iOffset + 1] ) )
#endif
            {
              tokenType = TK_DOT;
              return 1;
            }
            /* If the next character is a digit, this is a floating point
            ** number that begins with ".".  Fall thru into the next case */
            goto case '0';
          }
        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
          {
            testcase( z[0] == '0' ); testcase( z[0] == '1' ); testcase( z[0] == '2' );
            testcase( z[0] == '3' ); testcase( z[0] == '4' ); testcase( z[0] == '5' );
            testcase( z[0] == '6' ); testcase( z[0] == '7' ); testcase( z[0] == '8' );
            testcase( z[0] == '9' );
            tokenType = TK_INTEGER;
            for ( i = 0 ; z.Length > iOffset + i && sqlite3Isdigit( z[iOffset + i] ) ; i++ ) { }
#if !SQLITE_OMIT_FLOATING_POINT
            if ( z.Length > iOffset + i && z[iOffset + i] == '.' )
            {
              i++;
              while ( z.Length > iOffset + i && sqlite3Isdigit( z[iOffset + i] ) ) { i++; }
              tokenType = TK_FLOAT;
            }
            if ( z.Length > iOffset + i + 1 && ( z[iOffset + i] == 'e' || z[iOffset + i] == 'E' ) &&
            ( sqlite3Isdigit( z[iOffset + i + 1] )
            || z.Length > iOffset + i + 2 && ( ( z[iOffset + i + 1] == '+' || z[iOffset + i + 1] == '-' ) && sqlite3Isdigit( z[iOffset + i + 2] ) )
            )
            )
            {
              i += 2;
              while ( z.Length > iOffset + i && sqlite3Isdigit( z[iOffset + i] ) ) { i++; }
              tokenType = TK_FLOAT;
            }
#endif
            while ( z.Length > iOffset + i && ( ( ( c = (byte)z[iOffset + i] ) & 0x80 ) != 0 || ( c > 0x1f && sqlite3IsAsciiIdChar[c - 0x20] ) ) )
            {// IdChar(z[iOffset+i]) ){
              tokenType = TK_ILLEGAL;
              i++;
            }
            return i;
          }
        case '[':
          {
            for ( i = 1, c = (byte)z[iOffset + 0] ; c != ']' && ( iOffset + i ) < z.Length && ( c = (byte)z[iOffset + i] ) != 0 ; i++ ) { }
            tokenType = c == ']' ? TK_ID : TK_ILLEGAL;
            return i;
          }
        case '?':
          {
            tokenType = TK_VARIABLE;
            for ( i = 1 ; z.Length > iOffset + i && sqlite3Isdigit( z[iOffset + i] ) ; i++ ) { }
            return i;
          }
        case '#':
          {
            for ( i = 1 ; z.Length > iOffset + i && sqlite3Isdigit( z[iOffset + i] ) ; i++ ) { }
            if ( i > 1 )
            {
              /* Parameters of the form #NNN (where NNN is a number) are used
              ** internally by sqlite3NestedParse.  */
              tokenType = TK_REGISTER;
              return i;
            }
            /* Fall through into the next case if the '#' is not followed by
            ** a digit. Try to match #AAAA where AAAA is a parameter name. */
            goto case ':';
          }
#if !SQLITE_OMIT_TCL_VARIABLE
        case '$':
#endif
        case '@':  /* For compatibility with MS SQL Server */
        case ':':
          {
            int n = 0;
            testcase( z[0] == '$' ); testcase( z[0] == '@' ); testcase( z[0] == ':' );
            tokenType = TK_VARIABLE;
            for ( i = 1 ; z.Length > iOffset + i && ( c = (byte)z[iOffset + i] ) != 0 ; i++ )
            {
              if ( ( ( c & 0x80 ) != 0 || ( c > 0x1f && sqlite3IsAsciiIdChar[c - 0x20] ) ) )
              {//IdChar(c) ){
                n++;
#if !SQLITE_OMIT_TCL_VARIABLE
              }
              else if ( c == '(' && n > 0 )
              {
                do
                {
                  i++;
                } while ( ( iOffset + i ) < z.Length && ( c = (byte)z[iOffset + i] ) != 0 && !sqlite3Isspace( c ) && c != ')' );
                if ( c == ')' )
                {
                  i++;
                }
                else
                {
                  tokenType = TK_ILLEGAL;
                }
                break;
              }
              else if ( c == ':' && z[iOffset + i + 1] == ':' )
              {
                i++;
#endif
              }
              else
              {
                break;
              }
            }
            if ( n == 0 ) tokenType = TK_ILLEGAL;
            return i;
          }
#if !SQLITE_OMIT_BLOB_LITERAL
        case 'x':
        case 'X':
          {
            testcase( z[0] == 'x' ); testcase( z[0] == 'X' );
            if ( z.Length > iOffset + 1 && z[iOffset + 1] == '\'' )
            {
              tokenType = TK_BLOB;
              for ( i = 2 ; z.Length > iOffset + i && ( c = (byte)z[iOffset + i] ) != 0 && c != '\'' ; i++ )
              {
                if ( !sqlite3Isxdigit( c ) )
                {
                  tokenType = TK_ILLEGAL;
                }
              }
              if ( i % 2 != 0 || z.Length == iOffset + i && c != '\'' ) tokenType = TK_ILLEGAL;
              if ( c != 0 ) i++;
              return i;
            }
            goto default;
            /* Otherwise fall through to the next case */
          }
#endif
        default:
          {
            if ( !( ( ( c = (byte)z[iOffset + 0] ) & 0x80 ) != 0 || ( c > 0x1f && sqlite3IsAsciiIdChar[c - 0x20] ) ) )
            {//IdChar(*z) ){
              break;
            }
            for ( i = 1 ; z.Length > iOffset + i && ( ( ( c = (byte)z[iOffset + i] ) & 0x80 ) != 0 || ( c > 0x1f && sqlite3IsAsciiIdChar[c - 0x20] ) ) ; i++ ) { }//IdChar(z[iOffset+i]); i++){}
            tokenType = keywordCode( z, iOffset, i );
            return i;
          }
      }
      tokenType = TK_ILLEGAL;
      return 1;
    }

    /*
    ** Run the parser on the given SQL string.  The parser structure is
    ** passed in.  An SQLITE_ status code is returned.  If an error occurs
    ** then an and attempt is made to write an error message into
    ** memory obtained from sqlite3_malloc() and to make pzErrMsg point to that
    ** error message.
    */
    static int sqlite3RunParser( Parse pParse, string zSql, ref string pzErrMsg )
    {
      int nErr = 0;                   /* Number of errors encountered */
      int i;                          /* Loop counter */
      yyParser pEngine;               /* The LEMON-generated LALR(1) parser */
      int tokenType = 0;              /* type of the next token */
      int lastTokenParsed = -1;       /* type of the previous token */
      byte enableLookaside;           /* Saved value of db->lookaside.bEnabled */
      sqlite3 db = pParse.db;         /* The database connection */
      int mxSqlLen;                   /* Max length of an SQL string */


      mxSqlLen = db.aLimit[SQLITE_LIMIT_SQL_LENGTH];
      if ( db.activeVdbeCnt == 0 )
      {
        db.u1.isInterrupted = false;
      }
      pParse.rc = SQLITE_OK;
      pParse.zTail = new StringBuilder( zSql );
      i = 0;
      Debug.Assert( pzErrMsg != null );
      pEngine = sqlite3ParserAlloc();//sqlite3ParserAlloc((void*(*)(size_t))sqlite3Malloc);
      if ( pEngine == null )
      {
////        db.mallocFailed = 1;
        return SQLITE_NOMEM;
      }
      Debug.Assert( pParse.pNewTable == null );
      Debug.Assert( pParse.pNewTrigger == null );
      Debug.Assert( pParse.nVar == 0 );
      Debug.Assert( pParse.nVarExpr == 0 );
      Debug.Assert( pParse.nVarExprAlloc == 0 );
      Debug.Assert( pParse.apVarExpr == null );
      enableLookaside = db.lookaside.bEnabled;
      if ( db.lookaside.pStart != 0 ) db.lookaside.bEnabled = 1;
      while ( /*  0 == db.mallocFailed && */  i < zSql.Length )
      {
        Debug.Assert( i >= 0 );
        //pParse->sLastToken.z = &zSql[i];
        pParse.sLastToken.n = sqlite3GetToken( zSql, i, ref tokenType );
        pParse.sLastToken.z = zSql.Substring( i );
        i += pParse.sLastToken.n;
        if ( i > mxSqlLen )
        {
          pParse.rc = SQLITE_TOOBIG;
          break;
        }
        switch ( tokenType )
        {
          case TK_SPACE:
            {
              if ( db.u1.isInterrupted )
              {
                sqlite3ErrorMsg( pParse, "interrupt" );
                pParse.rc = SQLITE_INTERRUPT;
                goto abort_parse;
              }
              break;
            }
          case TK_ILLEGAL:
            {
              //sqlite3DbFree( db, ref pzErrMsg );
              pzErrMsg = sqlite3MPrintf( db, "unrecognized token: \"%T\"",
                (object)pParse.sLastToken );
              nErr++;
              goto abort_parse;
            }
          case TK_SEMI:
            {
              //pParse.zTail = new StringBuilder(zSql.Substring( i,zSql.Length-i ));
              /* Fall thru into the default case */
              goto default;
            }
          default:
            {
              sqlite3Parser( pEngine, tokenType, pParse.sLastToken, pParse );
              lastTokenParsed = tokenType;
              if ( pParse.rc != SQLITE_OK )
              {
                goto abort_parse;
              }
              break;
            }
        }
      }
abort_parse:
      pParse.zTail = new StringBuilder( zSql.Length <= i ? "" : zSql.Substring( i, zSql.Length - i ) );
      if ( zSql.Length >= i && nErr == 0 && pParse.rc == SQLITE_OK )
      {
        if ( lastTokenParsed != TK_SEMI )
        {
          sqlite3Parser( pEngine, TK_SEMI, pParse.sLastToken, pParse );
        }
        sqlite3Parser( pEngine, 0, pParse.sLastToken, pParse );
      }
#if YYTRACKMAXSTACKDEPTH
sqlite3StatusSet(SQLITE_STATUS_PARSER_STACK,
sqlite3ParserStackPeak(pEngine)
);
#endif //* YYDEBUG */
      sqlite3ParserFree(pEngine, null);//sqlite3_free );
      db.lookaside.bEnabled = enableLookaside;
      //if ( db.mallocFailed != 0 )
      //{
      //  pParse.rc = SQLITE_NOMEM;
      //}
      if ( pParse.rc != SQLITE_OK && pParse.rc != SQLITE_DONE && pParse.zErrMsg == "" )
      {
        sqlite3SetString( ref pParse.zErrMsg, db, sqlite3ErrStr( pParse.rc ) );
      }
      //assert( pzErrMsg!=0 );
      if ( pParse.zErrMsg != null )
      {
        pzErrMsg = pParse.zErrMsg;
        pParse.zErrMsg = "";
        nErr++;
      }
      if ( pParse.pVdbe != null && pParse.nErr > 0 && pParse.nested == 0 )
      {
        sqlite3VdbeDelete( ref pParse.pVdbe );
        pParse.pVdbe = null;
      }
#if !SQLITE_OMIT_SHARED_CACHE
if ( pParse.nested == 0 )
{
//sqlite3DbFree( db, ref pParse.aTableLock );
pParse.aTableLock = null;
pParse.nTableLock = 0;
}
#endif
#if !SQLITE_OMIT_VIRTUALTABLE
//sqlite3DbFree(db,pParse.apVtabLock);
#endif
      if ( !IN_DECLARE_VTAB )
      {
        /* If the pParse.declareVtab flag is set, do not delete any table
        ** structure built up in pParse.pNewTable. The calling code (see vtab.c)
        ** will take responsibility for freeing the Table structure.
        */
        sqlite3DeleteTable( ref pParse.pNewTable );
      }

#if !SQLITE_OMIT_TRIGGER
      sqlite3DeleteTrigger( db, ref pParse.pNewTrigger );
#endif
      //sqlite3DbFree( db, ref pParse.apVarExpr );
      //sqlite3DbFree( db, ref pParse.aAlias );
      while ( pParse.pAinc != null )
      {
        AutoincInfo p = pParse.pAinc;
        pParse.pAinc = p.pNext;
        //sqlite3DbFree( db, ref p );
      }
      while ( pParse.pZombieTab != null )
      {
        Table p = pParse.pZombieTab;
        pParse.pZombieTab = p.pNextZombie;
        sqlite3DeleteTable( ref p );
      }
      if ( nErr > 0 && pParse.rc == SQLITE_OK )
      {
        pParse.rc = SQLITE_ERROR;
      }
      return nErr;
    }
  }
}

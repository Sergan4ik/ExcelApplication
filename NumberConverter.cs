using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelApplication
{
    public struct Index
    {
        public int row;
        public int column;
    }

    public static class NumberConverter
    {
        const int LastLetter = 26; // number of last letter in english alphabet
        const int FirstLetterInASCII = 65; //first index of char in ASCII
        const int LastLetterInASCII = 90; //last index of char in ASCII
        const int FirstNumberInASCII = 48; //first index of int in ASCII
        const int LastNumberInASCII = 57; //last index of int in ASCII
        public static string ConvertTo26System(int x)
        {
            x++;
            int mod;
            string colName = "";
            if(x == 0)
            {
                return ((char)(FirstLetterInASCII - 1)).ToString();
            }
            while(x > 0)
            {
                mod = (x - 1) % LastLetter;
                colName = ((char)(FirstLetterInASCII + mod)).ToString() + colName;
                x = (x - mod) / LastLetter;
            }

            return colName;
        }

        public static Index ConvertFrom26System(string s)
        {
            Index index = new Index();
            index.column = 0;
            index.row = 0;
            for(int i = 0; i < s.Length; i++)
            {
                if(s[i] >= FirstLetterInASCII - 1 && s[i] < LastLetterInASCII + 1)
                {
                    index.column *= LastLetter;
                    index.column += (s[i]) - (FirstLetterInASCII - 1);
                }
                else if(s[i] > FirstNumberInASCII - 1 && s[i] < LastNumberInASCII + 1)
                {
                    index.row *= 10; 
                    index.row += s[i] - FirstNumberInASCII;
                }
            }
            index.column--;
            return index;
        }
    }
}

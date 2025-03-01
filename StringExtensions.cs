﻿using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public static class StringExtensions
    {
        public static string ReplaceByIndex(this string me, int index, string valueToReplace, char Replacement = '#')
        {
            /*
                // the below replacment technique is NOT accurate! use the Remove-Insert instead. 
                
                // BUG:
                // Input = 'abcd 1234 abcd 3456 abcd', Match = first 34 (index 7)
                // Output = 'abcd 12## abcd ##56 abcd'

                Input = Input.Replace(value,  new string('#', value.Length));
                Input = Input.Replace("34", "##");
            */

            /*
                // Input = 'abcd 1234 abcd 3456 abcd', Match = first 34 (index 7)
                // Output = 'abcd 12## abcd 3456 abcd'

                Input = Input.Remove(index, value.Length).Insert(index, new string('#', value.Length))
                Input = Input.Remove(7, "34").Insert(7, "##");
            */
            return me.Remove(index, valueToReplace.Length).Insert(index, new string(Replacement, valueToReplace.Length));
        }

        public static bool ContainsIgnoreCase(this string me, string valueToTest)
        {            
            return me.IndexOf(valueToTest, StringComparison.OrdinalIgnoreCase) >= 0;

            /*
            var index = me.IndexOf(valueToTest, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                System.Diagnostics.Debugger.Break();

            return index >= 0;
            */
        }

        public static bool ContainsIgnoreCase(this IEnumerable<string> me, string valueToTest) {
            return me.Contains(valueToTest, new ExtendedStringComparer());
        }

        public static bool StartsWithIgnoreCase(this string me, string valueToTest)
        {
            return me.Trim().StartsWith(valueToTest, StringComparison.OrdinalIgnoreCase);
        }
    }
}

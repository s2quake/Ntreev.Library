//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Ntreev.Library.Properties;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ntreev.Library
{
    /// <summary>
    /// 식별자로 사용되는 문자열의 검증 기능을 제공합니다.
    /// </summary>
    public static class IdentifierValidator
    {
        public const string IdentiFierPattern = @"^[a-zA-Z_$][a-zA-Z0-9_$]*$";

        /// <summary>
        /// 식별자에 적합하지 않은 문자열을 '_'로 대체합니다.
        /// </summary>
        /// <param name="input">
        /// 식별자로 사용되는 문자열입니다.
        /// </param>
        /// <returns>
        /// 올바른 식별자로 대체된 문자열을 반환합니다.
        /// </returns>
        [Obsolete]
        public static string Replace(string input)
        {
            Regex regex = new Regex("[^_a-zA-Z0-9]");
            return regex.Replace(input.Trim(), "_");
        }

        /// <summary>
        /// 식별자가 올바른지에 대한 여부를 확인합니다.
        /// </summary>
        /// <param name="input">
        /// 식별자로 사용되는 문자열입니다.
        /// </param>
        /// <returns>
        /// 식별자로 사용되기에 적합하다면 true를, 그렇지 않다면 false를 반환합니다.
        /// </returns>
        /// <remarks>
        /// 이 메소드는 검사에 대한 유연성을 제공하지 않습니다. 전달 받은 문자열에 대한 정확한 검사를 수행합니다.
        /// 만약 문자열 처음이나 끝에 공백이 삽입되 있을 경우에는 검사에 실패하게 됩니다.
        /// </remarks>
        [Obsolete]
        public static bool IsCorrected(string input)
        {
            if (input == null)
                return false;
            Regex regex = new Regex(IdentifierValidator.IdentiFierPattern);
            return regex.IsMatch(input) == true;
        }

        public static bool Verify(string value)
        {
#if NETFRAMEWORK
            return CodeGenerator.IsValidLanguageIndependentIdentifier(value);
#else
            return IsCorrected(value);
#endif
        }

        public static void Validate(string value)
        {
            if (Verify(value) == false)
                throw new ArgumentException($"{value} is invalid identifier", nameof(value));
        }
    }
}

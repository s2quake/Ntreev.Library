// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Ntreev.Library
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using System;
using System.Text.RegularExpressions;

namespace JSSoft.Library
{
    /// <summary>
    /// 占식븝옙占쌘뤄옙 占쏙옙占실댐옙 占쏙옙占쌘울옙占쏙옙 占쏙옙占쏙옙 占쏙옙占쏙옙占?占쏙옙占쏙옙占쌌니댐옙.
    /// </summary>
    public static class IdentifierValidator
    {
        public const string IdentiFierPattern = @"^[a-zA-Z_$][a-zA-Z0-9_$]*$";

        /// <summary>
        /// 占식븝옙占쌘울옙 占쏙옙占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙 占쏙옙占쌘울옙占쏙옙 '_'占쏙옙 占쏙옙체占쌌니댐옙.
        /// </summary>
        /// <param name="input">
        /// 占식븝옙占쌘뤄옙 占쏙옙占실댐옙 占쏙옙占쌘울옙占쌉니댐옙.
        /// </param>
        /// <returns>
        /// 占시바몌옙 占식븝옙占쌘뤄옙 占쏙옙체占쏙옙 占쏙옙占쌘울옙占쏙옙 占쏙옙환占쌌니댐옙.
        /// </returns>
        [Obsolete]
        public static string Replace(string input)
        {
            Regex regex = new Regex("[^_a-zA-Z0-9]");
            return regex.Replace(input.Trim(), "_");
        }

        /// <summary>
        /// 占식븝옙占쌘곤옙 占시바몌옙占쏙옙占쏙옙 占쏙옙占쏙옙 占쏙옙占싸몌옙 확占쏙옙占쌌니댐옙.
        /// </summary>
        /// <param name="input">
        /// 占식븝옙占쌘뤄옙 占쏙옙占실댐옙 占쏙옙占쌘울옙占쌉니댐옙.
        /// </param>
        /// <returns>
        /// 占식븝옙占쌘뤄옙 占쏙옙占실기에 占쏙옙占쏙옙占싹다몌옙 true占쏙옙, 占쌓뤄옙占쏙옙 占십다몌옙 false占쏙옙 占쏙옙환占쌌니댐옙.
        /// </returns>
        /// <remarks>
        /// 占쏙옙 占쌨소듸옙占?占싯사에 占쏙옙占쏙옙 占쏙옙占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙占쏙옙占쏙옙 占십쏙옙占싹댐옙. 占쏙옙占쏙옙 占쏙옙占쏙옙 占쏙옙占쌘울옙占쏙옙 占쏙옙占쏙옙 占쏙옙확占쏙옙 占싯사를 占쏙옙占쏙옙占쌌니댐옙.
        /// 占쏙옙占쏙옙 占쏙옙占쌘울옙 처占쏙옙占싱놂옙 占쏙옙占쏙옙 占쏙옙占쏙옙占쏙옙 占쏙옙占쌉듸옙 占쏙옙占쏙옙 占쏙옙荑∽옙占?占싯사에 占쏙옙占쏙옙占싹곤옙 占싯니댐옙.
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
            if (value == null)
                return false;
            Regex regex = new Regex(IdentifierValidator.IdentiFierPattern);
            return regex.IsMatch(value) == true;
        }

        public static void Validate(string value)
        {
            if (Verify(value) == false)
                throw new ArgumentException($"{value} is invalid identifier", nameof(value));
        }
    }
}

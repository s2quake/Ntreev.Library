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

namespace JSSoft.Library
{
    public sealed class FieldStack<T> : IFieldStack<T> where T : struct
    {
        public FieldStack()
        {
        }

        public FieldStack(T value)
        {
            this.Field = value;
        }

        public override string ToString()
        {
            return $"{this.Field}";
        }

        public static implicit operator T(FieldStack<T> obj)
        {
            return obj.Field;
        }

        public IDisposable Set(T value)
        {
            return new UsingClass(this, value);
        }

        public T Field { get; set; }

        #region classes

        class UsingClass : IDisposable
        {
            private readonly FieldStack<T> fieldStack;
            private readonly T oldValue;

            public UsingClass(FieldStack<T> field, T value)
            {
                this.fieldStack = field;
                this.oldValue = this.fieldStack.Field;
                this.fieldStack.Field = value;
            }

            void IDisposable.Dispose()
            {
                this.fieldStack.Field = this.oldValue;
            }
        }

        #endregion
    }
}

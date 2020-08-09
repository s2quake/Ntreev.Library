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

using System;

namespace Ntreev.Library
{
    public sealed class FieldStack<T> : IFieldStack<T> where T : struct
    {
        private T field;

        public FieldStack()
        {

        }

        public FieldStack(T value)
        {
            this.field = value;
        }

        public override string ToString()
        {
            return $"{this.field}";
        }

        public static implicit operator T(FieldStack<T> obj)
        {
            return obj.field;
        }

        public IDisposable Set(T value)
        {
            return new UsingClass(this, value);
        }

        public T Field
        {
            get => this.field;
            set => this.field = value;
        }

        #region classes

        class UsingClass : IDisposable
        {
            private readonly FieldStack<T> fieldStack;
            private readonly T oldValue;

            public UsingClass(FieldStack<T> field, T value)
            {
                this.fieldStack = field;
                this.oldValue = this.fieldStack.field;
                this.fieldStack.field = value;
            }

            void IDisposable.Dispose()
            {
                this.fieldStack.field = this.oldValue;
            }
        }

        #endregion
    }
}

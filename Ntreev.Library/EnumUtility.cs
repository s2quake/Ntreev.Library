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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Library
{
    public static class EnumUtility
    {
        public static void GetEnumData<T>(IEnumerable<T> items, Func<T, string> nameSelector, Func<T, ulong> valueSelector, out string[] enumNames, out ulong[] enumValues)
        {
            int count = items.Count();
            ulong[] array = new ulong[count];
            string[] array2 = new string[count];
            int i = 0;
            foreach (var item in items)
            {
                array2[i] = nameSelector(item);
                array[i] = valueSelector(item);
                i++;
            }

            IComparer @default = Comparer.Default;
            for (int j = 1; j < array.Length; j++)
            {
                int num = j;
                string text = array2[j];
                ulong obj = array[j];
                bool flag = false;
                while (@default.Compare(array[num - 1], obj) > 0)
                {
                    array2[num] = array2[num - 1];
                    array[num] = array[num - 1];
                    num--;
                    flag = true;
                    if (num == 0)
                    {
                        break;
                    }
                }
                if (flag)
                {
                    array2[num] = text;
                    array[num] = obj;
                }
            }
            enumNames = array2;
            enumValues = array;
        }

        public static string ConvertToString(string[] names, ulong[] values, bool isFlag, ulong value)
        {
            if (isFlag == true)
            {
                ulong num = (ulong)value;
                int num2 = values.Length - 1;
                StringBuilder stringBuilder = new StringBuilder();
                bool flag = true;
                ulong num3 = num;
                while (num2 >= 0 && (num2 != 0 || values[num2] != 0uL))
                {
                    if ((num & values[num2]) == values[num2])
                    {
                        num -= values[num2];
                        if (!flag)
                        {
                            stringBuilder.Insert(0, " ");
                        }
                        stringBuilder.Insert(0, names[num2]);
                        flag = false;
                    }
                    num2--;
                }
                if (num != 0uL)
                {
                    return value.ToString();
                }
                if (num3 != 0uL)
                {
                    return stringBuilder.ToString();
                }
                if (values.Length != 0 && values[0] == 0uL)
                {
                    return names[0];
                }
                return "0";
            }
            else
            {
                for (var i = 0; i < names.Length; i++)
                {
                    if (values[i] == value)
                        return names[i];
                }
                return value.ToString();
                //List<ulong> array = new List<ulong>(values.Length);
                //string[] array2 = new string[names.Length];
                //IComparer @default = Comparer.Default;
                //for (int j = 1; j < names.Length; j++)
                //{
                //    int num = j;
                //    string text = names[j];
                //    ulong obj = values[j];
                //    bool flag = false;

                //    while (@default.Compare(array[num - 1], obj) > 0)
                //    {
                //        array2[num] = array2[num - 1];
                //        array[num] = array[num - 1];
                //        num--;
                //        flag = true;
                //        if (num == 0)
                //        {
                //            break;
                //        }
                //    }
                //    if (flag)
                //    {
                //        array2[num] = text;
                //        array[num] = obj;
                //    }
                //}

                //int index = array.IndexOf((ulong)value);
                //return array2[index];
            }
        }

        public static ulong ConvertFromString(string[] names, ulong[] values, bool isFlag, string textValue)
        {
            if (isFlag == true)
            {
                var ss = textValue.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                ulong value = 0;
                foreach (var item in ss)
                {
                    bool existed = false;
                    for (var i = 0; i < names.Length; i++)
                    {
                        if (names[i] == item)
                        {
                            value |= values[i];
                            existed = true;
                            break;
                        }
                    }

                    if (existed == false)
                        throw new InvalidOperationException(string.Format("{0} 값은 정의되어 있지 않습니다.", item));
                }
                return value;
            }
            else
            {
                for (var i = 0; i < names.Length; i++)
                {
                    if (textValue == names[i])
                        return values[i];
                }
                throw new InvalidOperationException(string.Format("{0} 값은 정의되어 있지 않습니다.", textValue));
            }
        }
    }
}

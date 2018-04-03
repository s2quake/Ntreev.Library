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

using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Random.Properties;
using Ntreev.Library.IO;

namespace Ntreev.Library.Random
{
    public static class RandomUtility
    {
        private static System.Random random = new System.Random(DateTime.Now.Millisecond);
        private static string[] words;
        private static byte[] longBytes = new byte[8];
        private static byte[] intBytes = new byte[4];

        static RandomUtility()
        {
            int i = 0;
            using (var sr = new StringReader(Resources.words))
            {
                string line = null;

                while ((line = sr.ReadLine()) != null)
                {
                    i++;
                }
            }

            words = new string[i];
            i = 0;
            using (var sr = new StringReader(Resources.words))
            {
                string line = null;

                while ((line = sr.ReadLine()) != null)
                {
                    words[i++] = line;
                }
            }
        }

        public static int Next(int min, int max)
        {
            return random.Next(min, max);
        }

        public static int Next(int max)
        {
            return random.Next(max);
        }

        public static long NextLong(long max)
        {
            return NextLong(0, max);
        }

        public static long NextLong(long min, long max)
        {
            var bytes = BitConverter.GetBytes((long)0);
            random.NextBytes(bytes);
            var value = (long)BitConverter.ToInt64(bytes, 0);
            return Math.Abs(value % (max - min)) + min;
        }

        public static string NextWord()
        {
            int index = Next(words.Length);
            return words[index];
        }

        public static string NextName()
        {
            string name;

            while (NameValidator.VerifyName((name = NextWord())) == false)
                ;
            return name;
        }

        public static string NextInvalidName()
        {
            string name;

            while (NameValidator.VerifyName((name = NextWord())) == true)
                ;
            return name;
        }

        public static string NextCategoryPath()
        {
            var depth = Next(0, 5);
            return NextCategoryPath(depth);
        }

        public static string NextCategoryPath(int depth)
        {
            var items = new List<string>();
            for (var i = 0; i < depth; i++)
            {
                items.Add(NextName());
            }
            var path = string.Join(PathUtility.Separator, items);
            if (path == string.Empty)
                return PathUtility.Separator;
            return path.WrapSeparator();
        }

        public static string NextInvalidCategoryPath()
        {
            var depth = Next(0, 5);
            return NextInvalidCategoryPath(depth);
        }

        public static string NextInvalidCategoryPath(int depth)
        {
            var items = new List<string>();
            for (var i = 0; i < depth; i++)
            {
                items.Add(NextInvalidName());
            }
            var path = string.Join(PathUtility.Separator, items);
            if (path == string.Empty)
                return string.Empty;
            return path.WrapSeparator();
        }

        public static string NextIdentifier()
        {
            string name;

            while (IdentifierValidator.Verify((name = NextWord())) == false)
                ;
            return name;
        }

        public static string NextInvalidIdentifier()
        {
            string name;

            while (IdentifierValidator.Verify((name = NextWord())) == true)
                ;
            return name;
        }

        public static T NextEnum<T>() where T : IConvertible
        {
            Type type = typeof(T);
            var values = Enum.GetValues(type);
            int index = Next(values.Length);

            return (T)values.GetValue(index);
        }

        public static T Next<T>()
        {
            return (T)Next(typeof(T));
        }

        public static object Next(Type type)
        {
            if (type == typeof(System.Boolean))
            {
                if (Next(2) == 0)
                    return false;
                return true;
            }
            else if (type == typeof(string))
            {
                return NextIdentifier();
            }
            else if (type == typeof(float))
            {
                return (float)random.NextDouble() * random.Next();
            }
            else if (type == typeof(double))
            {
                return (double)random.NextDouble() * random.Next();
            }
            else if (type == typeof(sbyte))
            {
                var bytes = new byte[1];
                random.NextBytes(bytes);
                return (sbyte)bytes[0];
            }
            else if (type == typeof(byte))
            {
                var bytes = new byte[1];
                random.NextBytes(bytes);
                return bytes[0];
            }
            else if (type == typeof(short))
            {
                var bytes = new byte[2];
                random.NextBytes(bytes);
                return BitConverter.ToInt16(bytes, 0);
            }
            else if (type == typeof(ushort))
            {
                var bytes = new byte[2];
                random.NextBytes(bytes);
                return BitConverter.ToUInt16(bytes, 0);
            }
            else if (type == typeof(int))
            {
                var bytes = new byte[4];
                random.NextBytes(bytes);
                return BitConverter.ToInt32(bytes, 0);
            }
            else if (type == typeof(uint))
            {
                var bytes = new byte[4];
                random.NextBytes(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
            else if (type == typeof(long))
            {
                random.NextBytes(longBytes);
                return (long)BitConverter.ToInt64(longBytes, 0);
            }
            else if (type == typeof(ulong))
            {
                random.NextBytes(longBytes);
                return (ulong)BitConverter.ToUInt64(longBytes, 0);
            }
            else if (type == typeof(System.DateTime))
            {
                var year = Next(1970, 2050+1);
                var month = Next(1, 12+1);
                var day = Next(1, 12 + 1);

                var minValue = new DateTime(1970, 1, 1, 0, 0, 0).Ticks;
                var maxValue = new DateTime(2050, 12, 31, 0, 0, 0).Ticks;
                var value = NextLong(minValue, maxValue) / (long)10000000 * (long)10000000;
                return new DateTime(value);
            }
            else if (type == typeof(System.TimeSpan))
            {
                return new TimeSpan(NextLong(new TimeSpan(365, 0, 0, 0).Ticks));
            }
            else if (type == typeof(Guid))
            {
                return Guid.NewGuid();
            }
            else if (type.IsEnum == true)
            {
                string[] names = Enum.GetNames(type);

                if (names.Length == 0)
                    return null;

                string name = null;

                if (type.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    var query = (from item in names
                                 where Next(3) == 0
                                 select item).ToArray();

                    if (query.Any())
                        name = string.Join(", ", query);
                    else
                        return names.First();
                }
                else
                {
                    name = names[Next(names.Length)];
                }

                return Enum.Parse(type, name);
            }

            throw new Exception("지원하지 않는 타입입니다.");
        }

        public static T RandomOrDefault<T>(this IEnumerable<T> enumerable)
        {
            return RandomOrDefault(enumerable, item => true);
        }

        public static T RandomOrDefault<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            var list = enumerable.Where(predicate).ToArray();
            if (list.Any() == false)
                return default(T);
            var count = list.Length;
            var index = Next(count);
            return list[index];
        }

        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            return Random(enumerable, item => true);
        }

        public static T Random<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            var item = RandomOrDefault(enumerable, predicate);
            if (item == null)
                throw new InvalidOperationException();
            return item;
        }

        public static T WeightedRandomOrDefault<T>(this IEnumerable<T> enumerable, Func<T, int> weightSelector)
        {
            return WeightedRandomOrDefault(enumerable, weightSelector, item => true);
        }

        public static T WeightedRandomOrDefault<T>(this IEnumerable<T> enumerable, Func<T, int> weightSelector, Func<T, bool> predicate)
        {
            var totalWeight = 0;
            foreach (var item in enumerable.Where(predicate))
            {
                var weight = weightSelector(item);
                if (weight < 0)
                    throw new ArgumentException("weight must be greater or equals than zero");
                totalWeight += weight;
            }

            var value = RandomUtility.Next(totalWeight) + 1;

            totalWeight = 0;
            foreach (var item in enumerable.Where(predicate))
            {
                var weight = weightSelector(item);
                totalWeight += weight;
                if (value <= totalWeight)
                    return item;
            }

            return default(T);
        }

        public static T WeightedRandom<T>(this IEnumerable<T> enumerable, Func<T, int> weightSelector)
        {
            return WeightedRandom(enumerable, weightSelector, item => true);
        }

        public static T WeightedRandom<T>(this IEnumerable<T> enumerable, Func<T, int> weightSelector, Func<T, bool> predicate)
        {
            var item = WeightedRandomOrDefault(enumerable, weightSelector, predicate);
            if (item == null)
                throw new InvalidOperationException();
            return item;
        }

        public static string NextString()
        {
            return NextString(false);
        }

        public static string NextString(bool multiline)
        {
            string s = string.Empty;
            int count = Next(20);
            for (int i = 0; i < count; i++)
            {
                s += Next<string>();
                if (i > 0 && multiline == true && Within(5) == true)
                {
                    s += Environment.NewLine;
                }
                else if (i + 1 != count)
                {
                    s += " ";
                }
            }

            return s;
        }

        public static bool NextBoolean()
        {
            return Next<bool>();
        }

        public static long NextBit()
        {
            if (RandomUtility.Within(1) == true)
                return 0;
            return 1 << random.Next(32);
        }

        public static TagInfo NextTags()
        {
            var value = Next(10);

            if (value >= 9)
            {
                return TagInfo.All;
            }
            else if (value >= 8)
            {
                return TagInfo.Unused;
            }
            else
            {
                var items = new List<string>();
                for (var i = 0; i < Next(1, 4); i++)
                {
                    items.Add(NextIdentifier());
                }
                return (TagInfo)string.Join($"{TagInfo.Separator} ", items);
            }
        }

        public static bool Within(int percent)
        {
            var value = Next(100);
            return percent >= value;
        }
    }
}

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

using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ntreev.Library.Linq
{
    public static class EnumerableUtility
    {
        public static IEnumerable<TSource> AsEnumerable<TSource>(TSource e)
        {
            yield return e;
        }

        public static IEnumerable<T> OrderByAttribute<T>(this IEnumerable<T> nodes)
        {
            return nodes.OrderBy(item => GetOrder(item));
        }

        public static IEnumerable<T> TopologicalSort<T>(this IEnumerable<T> nodes)
        {
            return nodes.TopologicalSort<T>(item => GetDependencies(item, nodes));
        }

        public static IEnumerable<T> TopologicalSort<T>(this IEnumerable<T> nodes, Func<T, IEnumerable<T>> dependencies)
        {
            var sorted = new List<T>();
            var visited = new Dictionary<T, bool>();

            foreach (var item in nodes)
            {
                Visit(item, dependencies, sorted, visited);
            }

            return sorted;
        }

        public static IEnumerable<TSource> FamilyTree<TSource>(TSource e, Func<TSource, IEnumerable<TSource>> childsFunc)
        {
            return FamilyTree<TSource>(e, childsFunc, item => true);
        }

        public static IEnumerable<TSource> FamilyTree<TSource>(TSource e, Func<TSource, IEnumerable<TSource>> childsFunc, Predicate<TSource> predicate)
        {
            if (predicate(e) == true)
            {
                yield return e;
            }

            foreach (var item in childsFunc(e))
            {
                foreach (var i in FamilyTree<TSource>(item, childsFunc, predicate))
                {
                    if (predicate(i) == true)
                    {
                        yield return i;
                    }
                }
            }
        }

        public static IEnumerable<TResult> FamilyTree<TSource, TResult>(TSource e, Func<TSource, IEnumerable<TSource>> childsFunc)
            where TSource : class where TResult : class
        {
            if (e is TResult == true)
            {
                yield return e as TResult;
            }

            foreach (var item in childsFunc(e))
            {
                foreach (var i in FamilyTree<TSource, TResult>(item, childsFunc))
                {
                    if (i is TResult == true)
                    {
                        yield return i as TResult;
                    }
                }
            }
        }

        public static IEnumerable<TResult> FamilyTree<TSource, TResult>(TSource e, Func<TSource, IEnumerable<TSource>> childsFunc, Predicate<TSource> predicate)
            where TSource : class
            where TResult : class
        {
            if (e is TResult && predicate(e) == true)
            {
                yield return e as TResult;
            }

            foreach (var item in childsFunc(e))
            {
                foreach (var i in FamilyTree<TSource>(item, childsFunc, predicate))
                {
                    if (i is TResult && predicate(i) == true)
                    {
                        yield return i as TResult;
                    }
                }
            }
        }

        public static IEnumerable<TSource> FamilyTree<TSource>(this IEnumerable<TSource> e, Func<TSource, IEnumerable<TSource>> childsFunc)
        {
            foreach (var item in e)
            {
                foreach (var i in FamilyTree<TSource>(item, childsFunc))
                {
                    yield return i;
                }
            }
        }

        public static IEnumerable<TSource> FamilyTree<TSource>(this IEnumerable<TSource> e, Func<TSource, IEnumerable<TSource>> childsFunc, Predicate<TSource> predicate)
        {
            foreach (var item in e)
            {
                foreach (var i in FamilyTree<TSource>(item, childsFunc, predicate))
                {
                    yield return i;
                }
            }
        }

        public static IEnumerable<TResult> FamilyTree<TSource, TResult>(this IEnumerable<TSource> e, Func<TSource, IEnumerable<TSource>> childsFunc)
            where TSource : class where TResult : class
        {
            foreach (var item in e)
            {
                foreach (var i in FamilyTree<TSource, TResult>(item, childsFunc))
                {
                    yield return i;
                }
            }
        }

        public static IEnumerable<TResult> FamilyTree<TSource, TResult>(this IEnumerable<TSource> e, Func<TSource, IEnumerable<TSource>> childsFunc, Predicate<TSource> predicate)
            where TSource : class
            where TResult : class
        {
            foreach (var item in e)
            {
                foreach (var i in FamilyTree<TSource, TResult>(item, childsFunc, predicate))
                {
                    yield return i;
                }
            }
        }

        public static IEnumerable<TSource> One<TSource>(TSource item)
        {
            yield return item;
        }

        public static IEnumerable<TSource> Descendants<TSource>(TSource e, Func<TSource, IEnumerable<TSource>> childsFunc)
        {
            return Descendants<TSource>(e, childsFunc, item => true);
        }

        public static IEnumerable<TSource> Descendants<TSource>(TSource e, Func<TSource, IEnumerable<TSource>> childsFunc, Predicate<TSource> predicate)
        {
            foreach (var item in childsFunc(e))
            {
                if (predicate(item) == true)
                {
                    yield return item;
                }

                foreach (var i in Descendants<TSource>(item, childsFunc, predicate))
                {
                    if (predicate(i) == true)
                    {
                        yield return i;
                    }
                }
            }
        }

        public static IEnumerable<TResult> Descendants<TSource, TResult>(TSource e, Func<TSource, IEnumerable<TSource>> childsFunc)
            where TSource : class
            where TResult : class
        {
            return Descendants<TSource, TResult>(e, childsFunc, item => true);
        }

        public static IEnumerable<TResult> Descendants<TSource, TResult>(TSource e, Func<TSource, IEnumerable<TSource>> childsFunc, Predicate<TResult> predicate)
            where TSource : class
            where TResult : class
        {
            foreach (var item in childsFunc(e))
            {
                if (item is TResult && predicate(item as TResult) == true)
                {
                    yield return item as TResult;
                }

                foreach (var i in Descendants<TSource, TResult>(item, childsFunc, predicate))
                {
                    if (i is TResult && predicate(i) == true)
                    {
                        yield return i as TResult;
                    }
                }
            }
        }

        public static IEnumerable<TSource> Descendants<TSource>(this IEnumerable<TSource> e, Func<TSource, IEnumerable<TSource>> childsFunc)
        {
            return Descendants<TSource>(e, childsFunc, item => true);
        }

        public static IEnumerable<TSource> Descendants<TSource>(this IEnumerable<TSource> e, Func<TSource, IEnumerable<TSource>> childsFunc, Predicate<TSource> predicate)
        {
            foreach (var item in e)
            {
                foreach (var i in Descendants<TSource>(item, childsFunc, predicate))
                {
                    yield return i;
                }
            }
        }

        public static IEnumerable<TResult> Descendants<TSource, TResult>(this IEnumerable<TSource> e, Func<TSource, IEnumerable<TSource>> childsFunc)
            where TSource : class
            where TResult : class
        {
            return Descendants<TSource, TResult>(e, childsFunc, item => true);
        }

        public static IEnumerable<TResult> Descendants<TSource, TResult>(this IEnumerable<TSource> e, Func<TSource, IEnumerable<TSource>> childsFunc, Predicate<TResult> predicate)
            where TSource : class
            where TResult : class
        {
            foreach (var item in e)
            {
                foreach (var i in Descendants<TSource, TResult>(item, childsFunc, predicate))
                {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// 자신과 목록을 합칩니다.
        /// </summary>
        public static IEnumerable<TSource> Friends<TSource>(TSource e, IEnumerable<TSource> items)
        {
            yield return e;

            foreach (var item in items)
            {
                yield return item;
            }
        }

        public static IEnumerable<IItem> Ancestors(this IItem e)
        {
            return Ancestors(e, item => true);
        }

        public static IEnumerable<IItem> Ancestors(this IItem e, Predicate<IItem> predicate)
        {
            var item = e;
            while (item != null)
            {
                item = item.Parent;
                if (item != null && predicate(item))
                    yield return item;
            }
        }

        public static IEnumerable<TSource> Ancestors<TSource>(TSource e, Func<TSource, TSource> parentFunc)
        {
            return Ancestors<TSource>(e, parentFunc, item => true);
        }

        public static IEnumerable<TSource> Ancestors<TSource>(TSource e, Func<TSource, TSource> parentFunc, Predicate<TSource> predicate)
        {
            var item = e;
            while (item != null)
            {
                item = parentFunc(item);
                if (item != null && predicate(item))
                    yield return item;
            }
        }

        public static IEnumerable<IEnumerable<T>> Permutations<T>(this IEnumerable<T> source, int length)
        {
            if (length == 1) return source.Select(t => new T[] { t });

            return Permutations(source, length - 1)
                .SelectMany(t => source.Where(e => !t.Contains(e)), (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        public static int IndexOf<TSource>(this IEnumerable<TSource> source, TSource item)
        {
            return IndexOf<TSource>(source, i => object.Equals(i, item));
        }

        public static int IndexOf<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate)
        {
            var index = 0;
            foreach (var item in source)
            {
                if (predicate(item) == true)
                    return index;
                index++;
            }
            return -1;
        }

        private static void Visit<T>(T item, Func<T, IEnumerable<T>> getDependencies, List<T> sorted, Dictionary<T, bool> visited)
        {
            var alreadyVisited = visited.TryGetValue(item, out bool inProcess);

            if (alreadyVisited)
            {
                if (inProcess)
                {
                    throw new ArgumentException("Cyclic dependency found.");
                }
            }
            else
            {
                visited[item] = true;

                var dependencies = getDependencies(item);
                if (dependencies != null)
                {
                    foreach (var dependency in dependencies)
                    {
                        Visit(dependency, getDependencies, sorted, visited);
                    }
                }

                visited[item] = false;
                sorted.Add(item);
            }
        }

        private static IEnumerable<T> GetDependencies<T>(T plugin, IEnumerable<T> plugins)
        {
            var attrs = plugin.GetType().GetCustomAttributes(typeof(DependencyAttribute), true);

            foreach (var item in attrs)
            {
                var attr = item as DependencyAttribute;

                if (attr.DependencyType == null)
                {
                    System.Diagnostics.Trace.WriteLine(string.Format("'{0}' 타입을 찾을 수 없습니다.", attr.DependencyTypeName));
                    continue;
                }

                var value = plugins.FirstOrDefault(i => (i.GetType() == attr.DependencyType || attr.DependencyType.IsAssignableFrom(i.GetType())));
                if (value != null)
                {
                    yield return value;
                }
            }
        }

        private static int GetOrder<T>(T item)
        {
            var attr = item.GetType().GetCustomAttribute<OrderAttribute>();
            if (attr != null)
            {
                return attr.Order;
            }
            return 0;
        }
    }
}

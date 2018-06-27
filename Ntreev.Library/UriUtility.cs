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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ntreev.Library
{
    public static class UriUtility
    {
        private readonly static string seperator = Path.AltDirectorySeparatorChar.ToString();

        public static Uri Combine(Uri uri, params string[] paths)
        {
            var query = uri.Query;
            var path = uri.AbsolutePath;

            if (path.EndsWith(seperator) == false)
                path += seperator;
            path += string.Join(seperator, paths);

            return new Uri(uri.Scheme + Uri.SchemeDelimiter + uri.Host + path + query);
        }

        public static Uri GetDirectoryName(Uri uri)
        {
            var segs = uri.Segments.Take(uri.Segments.Length - 1).Aggregate((w, n) => w + n);
            return new Uri(uri.Scheme + Uri.SchemeDelimiter + uri.Host + segs);
        }

        public static string Combine(string uri, params string[] paths)
        {
            return Combine(new Uri(uri), paths).ToString();
        }

        public static string MakeRelativeOfDirectory(Uri uri1, Uri uri2)
        {
            return MakeRelativeOfDirectory(uri1.ToString(), uri2.ToString());
        }

        public static string MakeRelativeOfDirectory(string path1, string path2)
        {
            if (path1.EndsWith(Path.DirectorySeparatorChar.ToString()) == false && path1.EndsWith(Path.AltDirectorySeparatorChar.ToString()) == false)
                path1 += Path.AltDirectorySeparatorChar;

            var uri1 = new Uri(path1);
            var uri2 = new Uri(path2);
            var relativeUri = uri1.MakeRelativeUri(uri2);
            return Uri.UnescapeDataString(relativeUri.ToString());
        }

        public static string MakeRelative(string path1, string path2)
        {
            var uri1 = new Uri(path1);
            var uri2 = new Uri(path2);

            var relativeUri = uri1.MakeRelativeUri(uri2);
            return Uri.UnescapeDataString(relativeUri.ToString());
        }

        [Obsolete]
        public static Uri MakeRelative(Uri uri1, Uri uri2)
        {
            if (Path.GetExtension(uri1.ToString()) == string.Empty)
                uri1 = new Uri(uri1.ToString() + "/a.txt");
            if (Path.GetExtension(uri2.ToString()) == string.Empty)
                uri2 = new Uri(uri2.ToString() + "/a.txt");

            return uri1.MakeRelativeUri(uri2);
        }

        public static string MakeRelativeString(Uri uri1, Uri uri2)
        {
            return MakeRelative(uri1, uri2).ToString();
        }

        public static string RemoveExtension(Uri uri)
        {
            var uriString = uri.ToString();
            var extension = Path.GetExtension(uriString);
            if (extension == string.Empty)
                return uriString;
            return Regex.Replace(uriString, extension + "(?=\r?$)", string.Empty);
        }
    }
}

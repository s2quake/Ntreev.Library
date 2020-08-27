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
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace JSSoft.Library
{
    public static class AppUtility
    {
        private static string productName;
        private static string productVersion;
        private static string userAppDataPath;

        public static string ProductName
        {
            get
            {
                if (productName == null)
                {
                    var assembly = Assembly.GetEntryAssembly();
                    productName = FileVersionInfo.GetVersionInfo(assembly.Location).ProductName;
                }
                return productName;
            }
            set => productName = value;
        }

        public static string ProductVersion
        {
            get
            {
                if (productVersion != null)
                    return productVersion;
                var assembly = Assembly.GetEntryAssembly();
                var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return versionInfo.ProductVersion;
            }
            set => productVersion = value;
        }

        public static string StartupPath => System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

        public static string UserAppDataPath
        {
            get
            {
                if (userAppDataPath != null)
                    return userAppDataPath;
                return GetUserAppDataPath();
            }
            set => userAppDataPath = value;
        }

        public static string GetUserAppDataPath()
        {
            var assembly = Assembly.GetEntryAssembly();
            var companyName = !(assembly.GetCustomAttribute(typeof(AssemblyCompanyAttribute)) is AssemblyCompanyAttribute attr) || attr.Company == string.Empty ? "UnknownCompany" : attr.Company;
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), companyName, assembly.GetName().Name.ToString());
            return path;
        }

        public static string GetDocumentFilename(string filename)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(path, AppUtility.ProductName, filename);
        }

        public static string GetDocumentPath(params string[] paths)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(path, AppUtility.ProductName, Path.Combine(paths));
        }
    }
}

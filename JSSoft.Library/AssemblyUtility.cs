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
using System.IO;

namespace JSSoft.Library
{
    public static class AssemblyUtility
    {

        /// <summary>
        /// http://geekswithblogs.net/rupreet/archive/2005/11/02/58873.aspx
        /// </summary>
        /// <param name="path"></param>
        public static bool GetIsAssembly(string path)
        {
            uint peHeader;
            uint peHeaderSignature;
            ushort machine;
            ushort sections;
            uint timestamp;
            uint pSymbolTable;
            uint noOfSymbol;
            ushort optionalHeaderSize;
            ushort characteristics;
            ushort dataDictionaryStart;
            uint[] dataDictionaryRVA = new uint[16];
            uint[] dataDictionarySize = new uint[16];


            using Stream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fs);

            //PE Header starts @ 0x3C (60). Its a 4 byte header.
            fs.Position = 0x3C;

            peHeader = reader.ReadUInt32();

            //Moving to PE Header start location...
            fs.Position = peHeader;
            peHeaderSignature = reader.ReadUInt32();

            //We can also show all these value, but we will be       
            //limiting to the CLI header test.
            machine = reader.ReadUInt16();
            sections = reader.ReadUInt16();
            timestamp = reader.ReadUInt32();
            pSymbolTable = reader.ReadUInt32();
            noOfSymbol = reader.ReadUInt32();
            optionalHeaderSize = reader.ReadUInt16();
            characteristics = reader.ReadUInt16();

            dataDictionaryStart = Convert.ToUInt16(Convert.ToUInt16
            (fs.Position) + 0x60);
            fs.Position = dataDictionaryStart;
            for (int i = 0; i < 15; i++)
            {
                dataDictionaryRVA[i] = reader.ReadUInt32();
                dataDictionarySize[i] = reader.ReadUInt32();
            }
            if (dataDictionaryRVA[14] == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}

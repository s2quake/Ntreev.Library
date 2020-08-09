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

using System.IO;

namespace Ntreev.Library.IO
{
    public static class StreamExtensions
    {
        public static void CopyTo(this Stream sourceStream, long length, Stream destStream, IProgress progress)
        {
            var chunkSize = 2048;
            var buffer = new byte[chunkSize];

            var i = 0;
            var position = (long)0;

            do
            {
                var bytesRead = sourceStream.Read(buffer, 0, chunkSize);
                if (bytesRead == 0)
                    break;

                destStream.Write(buffer, 0, bytesRead);
                position += bytesRead;

                var c = (int)((double)position / length * 100);
                if (c > i)
                {
                    progress.Report(c * 0.01);
                    i = c;
                }
            }
            while (true);

            progress.Complete();
        }
    }
}

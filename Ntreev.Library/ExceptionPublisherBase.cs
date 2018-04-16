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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Library
{
    public abstract class ExceptionPublisherBase : IExceptionPublisher
    {
        public bool ReportDetails
        {
            get; set;
        }

        protected virtual void GetExceptionContext(object exceptionObject, IDictionary<string, string> exceptionContext)
        {
            if (this.ReportDetails == true)
                exceptionContext.Add($"{nameof(Environment.MachineName)}", $"{Environment.MachineName}");
            exceptionContext.Add("DateTime", $"{DateTime.Now}");
            exceptionContext.Add($"AppDomainName", $"{ AppDomain.CurrentDomain.FriendlyName}");
            if (this.ReportDetails == true)
                exceptionContext.Add($"{nameof(Environment.UserDomainName)}", $"{Environment.UserDomainName}");
            if (this.ReportDetails == true)
                exceptionContext.Add($"{nameof(Environment.UserName)}", $"{Environment.UserName}");
            exceptionContext.Add("Program Version", $"{AppUtility.ProductVersion}");
        }

        protected virtual string BuildExceptionMessage(object exceptionObject, IDictionary<string, string> exceptionContext)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"General Information");
            foreach (var item in exceptionContext)
            {
                sb.AppendLine($"{item.Key}: {item.Value}");
            }
            sb.AppendLine();

            sb.AppendLine($"Exception Information");
            if (exceptionObject == null)
                sb.AppendLine($"exception object is null");
            else
                sb.AppendLine(exceptionObject.ToString());

            return sb.ToString();
        }

        protected abstract void OnPublish(object exceptionObject, string exceptionMessage);

        #region IExceptionPublisher

        void IExceptionPublisher.Publish(object exceptionObject)
        {
            var exceptionContext = new Dictionary<string, string>();
            this.GetExceptionContext(exceptionObject, exceptionContext);
            var exceptionMessage = this.BuildExceptionMessage(exceptionObject, exceptionContext);
            this.OnPublish(exceptionObject, exceptionMessage);
        }

        #endregion
    }
}

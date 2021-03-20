// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.CommandLine;
using System.CommandLine.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Microsoft.Tye.BuildServer
{
    public class LoggerConsole : IConsole, IStandardStreamWriter
    {
        private readonly ILogger logger;

        public LoggerConsole(ILogger logger)
        {
            this.logger = logger;
            this.Captured = new StringBuilder();
        }

        public StringBuilder Captured { get; }

        public IStandardStreamWriter Out => this;

        public bool IsOutputRedirected => true;

        public IStandardStreamWriter Error => this;

        public bool IsErrorRedirected => true;

        public bool IsInputRedirected => true;

        public void Write(string value)
        {
            this.logger.LogInformation(value);
            this.Captured.Append(value);
        }
    }
}

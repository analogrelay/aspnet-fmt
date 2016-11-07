using System;
using System.IO;

namespace AspNetFormatter.Rules
{
    public class LicenseHeaderRule : FormattingRule
    {
        public static readonly string LicenseHeader = $@"// Copyright (c) .NET Foundation. All rights reserved.{Environment.NewLine}// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.";

        public override bool Validate(string path, string fileContent)
        {
            return Path.GetExtension(path) != ".cs" || fileContent.StartsWith(LicenseHeader);
        }

        public override string Fix(string path, string fileContent)
        {
            if (Path.GetExtension(path) != ".cs")
            {
                return fileContent;
            }
            return LicenseHeader + Environment.NewLine + Environment.NewLine + fileContent.TrimStart();
        }
    }
}

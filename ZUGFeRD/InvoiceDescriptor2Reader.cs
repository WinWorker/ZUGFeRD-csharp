/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System.IO;

using ZUGFeRD;

namespace s2industries.ZUGFeRD
{
    internal class InvoiceDescriptor2Reader : InvoiceDescriptorReader
    {
        public override InvoiceDescriptor Load(Stream stream)
        {
            return base.Load(stream, new Zugferd20Dialect());
        }

        public override bool IsReadableByThisReaderVersion(Stream stream)
        {
            return base.IsReadableByThisReaderVersion(stream, new Zugferd20Dialect());
        }
    }

    internal class InvoiceDescriptorXRechnungReader : InvoiceDescriptorReader
    {
        public override InvoiceDescriptor Load(Stream stream)
        {
            return base.Load(stream, new XRechnungDialect());
        }

        public override bool IsReadableByThisReaderVersion(Stream stream)
        {
            return base.IsReadableByThisReaderVersion(stream, new XRechnungDialect());
        }
    }

    internal class InvoiceDescriptorFacturXReader : InvoiceDescriptorReader
    {
        public override InvoiceDescriptor Load(Stream stream)
        {
            return base.Load(stream, new FacturXDialect());
        }

        public override bool IsReadableByThisReaderVersion(Stream stream)
        {
            return base.IsReadableByThisReaderVersion(stream, new FacturXDialect());
        }
    }
}

﻿/*
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace s2industries.ZUGFeRD
{
    internal class InvoiceDescriptor1Writer : IInvoiceDescriptorWriter
    {
        private XmlTextWriter Writer;
        private InvoiceDescriptor Descriptor;


        public override void Save(InvoiceDescriptor descriptor, Stream stream)
        {
            if (!stream.CanWrite || !stream.CanSeek)
            {
                throw new IllegalStreamException("Cannot write to stream");
            }

            long streamPosition = stream.Position;

            this.Descriptor = descriptor;
            this.Writer = new XmlTextWriter(stream, Encoding.UTF8);
            Writer.Formatting = Formatting.Indented;
            Writer.WriteStartDocument();

            #region Kopfbereich
            Writer.WriteStartElement("rsm:CrossIndustryDocument");
            Writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            Writer.WriteAttributeString("xmlns", "rsm", null, "urn:ferd:CrossIndustryDocument:invoice:1p0");
            Writer.WriteAttributeString("xmlns", "ram", null, "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:12");
            Writer.WriteAttributeString("xmlns", "udt", null, "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:15");
            #endregion

            #region SpecifiedExchangedDocumentContext
            Writer.WriteStartElement("rsm:SpecifiedExchangedDocumentContext");
            Writer.WriteStartElement("ram:TestIndicator");
            Writer.WriteElementString("udt:Indicator", this.Descriptor.IsTest ? "true" : "false");
            Writer.WriteEndElement(); // !ram:TestIndicator
            Writer.WriteStartElement("ram:GuidelineSpecifiedDocumentContextParameter");
            Writer.WriteElementString("ram:ID", this.Descriptor.Profile.EnumToString(ZugFeRDVersion.Version1));
            Writer.WriteEndElement(); // !ram:GuidelineSpecifiedDocumentContextParameter
            Writer.WriteEndElement(); // !rsm:SpecifiedExchangedDocumentContext

            Writer.WriteStartElement("rsm:HeaderExchangedDocument");
            Writer.WriteElementString("ram:ID", this.Descriptor.InvoiceNo);
            Writer.WriteElementString("ram:Name", _translateInvoiceType(this.Descriptor.Type));
            Writer.WriteElementString("ram:TypeCode", string.Format("{0}", _encodeInvoiceType(this.Descriptor.Type)));

            if (this.Descriptor.InvoiceDate.HasValue)
            {
                Writer.WriteStartElement("ram:IssueDateTime");
                Writer.WriteStartElement("udt:DateTimeString");
                Writer.WriteAttributeString("format", "102");
                Writer.WriteValue(_formatDate(this.Descriptor.InvoiceDate.Value));
                Writer.WriteEndElement(); // !udt:DateTimeString
                Writer.WriteEndElement(); // !IssueDateTime
            }
            _writeNotes(Writer, this.Descriptor.Notes);
            Writer.WriteEndElement(); // !rsm:HeaderExchangedDocument
            #endregion

            #region SpecifiedSupplyChainTradeTransaction
            Writer.WriteStartElement("rsm:SpecifiedSupplyChainTradeTransaction");
            Writer.WriteStartElement("ram:ApplicableSupplyChainTradeAgreement");
            if (!string.IsNullOrEmpty(this.Descriptor.ReferenceOrderNo))
            {
                Writer.WriteElementString("ram:BuyerReference", this.Descriptor.ReferenceOrderNo);
            }

            _writeOptionalParty(Writer, "ram:SellerTradeParty", this.Descriptor.Seller, this.Descriptor.SellerContact, TaxRegistrations: this.Descriptor.SellerTaxRegistration);
            _writeOptionalParty(Writer, "ram:BuyerTradeParty", this.Descriptor.Buyer, this.Descriptor.BuyerContact, TaxRegistrations: this.Descriptor.BuyerTaxRegistration);

            if (this.Descriptor.OrderDate.HasValue || ((this.Descriptor.OrderNo != null) && (this.Descriptor.OrderNo.Length > 0)))
            {
                Writer.WriteStartElement("ram:BuyerOrderReferencedDocument");
                if (this.Descriptor.OrderDate.HasValue)
                {
                    Writer.WriteStartElement("ram:IssueDateTime");
                    //Writer.WriteStartElement("udt:DateTimeString");
                    //Writer.WriteAttributeString("format", "102");
                    Writer.WriteValue(_formatDate(this.Descriptor.OrderDate.Value, false));
                    //Writer.WriteEndElement(); // !udt:DateTimeString
                    Writer.WriteEndElement(); // !IssueDateTime()
                }

                Writer.WriteElementString("ram:ID", this.Descriptor.OrderNo);
                Writer.WriteEndElement(); // !BuyerOrderReferencedDocument
            }

            if (this.Descriptor.AdditionalReferencedDocument != null)
            {
                Writer.WriteStartElement("ram:AdditionalReferencedDocument");
                if (this.Descriptor.AdditionalReferencedDocument.IssueDateTime.HasValue)
                {
                    Writer.WriteStartElement("ram:IssueDateTime");
                    //Writer.WriteStartElement("udt:DateTimeString");
                    //Writer.WriteAttributeString("format", "102");
                    Writer.WriteValue(_formatDate(this.Descriptor.AdditionalReferencedDocument.IssueDateTime.Value, false));
                    //Writer.WriteEndElement(); // !udt:DateTimeString
                    Writer.WriteEndElement(); // !IssueDateTime()
                }

                if (this.Descriptor.AdditionalReferencedDocument.ReferenceTypeCode != ReferenceTypeCodes.Unknown)
                {
                    Writer.WriteElementString("ram:TypeCode", this.Descriptor.AdditionalReferencedDocument.ReferenceTypeCode.EnumToString());
                }

                Writer.WriteElementString("ram:ID", this.Descriptor.AdditionalReferencedDocument.ID);
                Writer.WriteEndElement(); // !ram:AdditionalReferencedDocument
            }

            Writer.WriteEndElement(); // !ApplicableSupplyChainTradeAgreement

            Writer.WriteStartElement("ram:ApplicableSupplyChainTradeDelivery"); // Pflichteintrag

            if (Descriptor.Profile == Profile.Extended)
            {
                _writeOptionalParty(Writer, "ram:ShipToTradeParty", this.Descriptor.ShipTo);
                _writeOptionalParty(Writer, "ram:ShipFromTradeParty", this.Descriptor.ShipFrom);
            }

            if (this.Descriptor.ActualDeliveryDate.HasValue)
            {
                Writer.WriteStartElement("ram:ActualDeliverySupplyChainEvent");
                Writer.WriteStartElement("ram:OccurrenceDateTime");
                Writer.WriteStartElement("udt:DateTimeString");
                Writer.WriteAttributeString("format", "102");
                Writer.WriteValue(_formatDate(this.Descriptor.ActualDeliveryDate.Value));
                Writer.WriteEndElement(); // "udt:DateTimeString
                Writer.WriteEndElement(); // !OccurrenceDateTime()
                Writer.WriteEndElement(); // !ActualDeliverySupplyChainEvent
            }

            if (this.Descriptor.DeliveryNoteReferencedDocument != null)
            {
                Writer.WriteStartElement("ram:DeliveryNoteReferencedDocument");

                if (this.Descriptor.DeliveryNoteReferencedDocument.IssueDateTime.HasValue)
                {
                    Writer.WriteStartElement("ram:IssueDateTime");
                    Writer.WriteValue(_formatDate(this.Descriptor.DeliveryNoteReferencedDocument.IssueDateTime.Value, false));
                    Writer.WriteEndElement(); // !IssueDateTime
                }

                Writer.WriteElementString("ram:ID", this.Descriptor.DeliveryNoteReferencedDocument.ID);
                Writer.WriteEndElement(); // !DeliveryNoteReferencedDocument
            }

            Writer.WriteEndElement(); // !ApplicableSupplyChainTradeDelivery

            Writer.WriteStartElement("ram:ApplicableSupplyChainTradeSettlement");

            if (Descriptor.Profile != Profile.Basic)
            {
                _writeOptionalParty(Writer, "ram:InvoiceeTradeParty", this.Descriptor.Invoicee);
            }
            if (Descriptor.Profile == Profile.Extended)
            {
                _writeOptionalParty(Writer, "ram:PayeeTradeParty", this.Descriptor.Payee);
            }

            if (!string.IsNullOrEmpty(this.Descriptor.InvoiceNoAsReference))
            {
                _writeOptionalElementString(Writer, "ram:PaymentReference", this.Descriptor.InvoiceNoAsReference);
            }
            Writer.WriteElementString("ram:InvoiceCurrencyCode", this.Descriptor.Currency.EnumToString());

            if (this.Descriptor.CreditorBankAccounts.Count == 0 && this.Descriptor.DebitorBankAccounts.Count == 0)
            {
                if (this.Descriptor.PaymentMeans != null)
                {
                    Writer.WriteStartElement("ram:SpecifiedTradeSettlementPaymentMeans");

                    if ((this.Descriptor.PaymentMeans != null) && (this.Descriptor.PaymentMeans.TypeCode != PaymentMeansTypeCodes.Unknown))
                    {
                        Writer.WriteElementString("ram:TypeCode", this.Descriptor.PaymentMeans.TypeCode.EnumToString());
                        Writer.WriteElementString("ram:Information", this.Descriptor.PaymentMeans.Information);

                        if (!string.IsNullOrEmpty(this.Descriptor.PaymentMeans.SEPACreditorIdentifier) && !string.IsNullOrEmpty(this.Descriptor.PaymentMeans.SEPAMandateReference))
                        {
                            Writer.WriteStartElement("ram:ID");
                            Writer.WriteAttributeString("schemeAgencyID", this.Descriptor.PaymentMeans.SEPACreditorIdentifier);
                            Writer.WriteValue(this.Descriptor.PaymentMeans.SEPAMandateReference);
                            Writer.WriteEndElement(); // !ram:ID
                        }
                    }
                    Writer.WriteEndElement(); // !SpecifiedTradeSettlementPaymentMeans
                }
            }
            else
            {
                foreach (BankAccount account in this.Descriptor.CreditorBankAccounts)
                {
                    Writer.WriteStartElement("ram:SpecifiedTradeSettlementPaymentMeans");

                    if ((this.Descriptor.PaymentMeans != null) && (this.Descriptor.PaymentMeans.TypeCode != PaymentMeansTypeCodes.Unknown))
                    {
                        Writer.WriteElementString("ram:TypeCode", this.Descriptor.PaymentMeans.TypeCode.EnumToString());
                        Writer.WriteElementString("ram:Information", this.Descriptor.PaymentMeans.Information);

                        if (!string.IsNullOrEmpty(this.Descriptor.PaymentMeans.SEPACreditorIdentifier) && !string.IsNullOrEmpty(this.Descriptor.PaymentMeans.SEPAMandateReference))
                        {
                            Writer.WriteStartElement("ram:ID");
                            Writer.WriteAttributeString("schemeAgencyID", this.Descriptor.PaymentMeans.SEPACreditorIdentifier);
                            Writer.WriteValue(this.Descriptor.PaymentMeans.SEPAMandateReference);
                            Writer.WriteEndElement(); // !ram:ID
                        }
                    }

                    Writer.WriteStartElement("ram:PayeePartyCreditorFinancialAccount");
                    Writer.WriteElementString("ram:IBANID", account.IBAN);
                    if (!string.IsNullOrEmpty(account.Name))
                    {
                        Writer.WriteElementString("ram:AccountName", account.Name);
                    }
                    if (!string.IsNullOrEmpty(account.ID))
                    {
                        Writer.WriteElementString("ram:ProprietaryID", account.ID);
                    }
                    Writer.WriteEndElement(); // !PayeePartyCreditorFinancialAccount

                    Writer.WriteStartElement("ram:PayeeSpecifiedCreditorFinancialInstitution");
                    Writer.WriteElementString("ram:BICID", account.BIC);

                    if (!string.IsNullOrEmpty(account.Bankleitzahl))
                    {
                        Writer.WriteElementString("ram:GermanBankleitzahlID", account.Bankleitzahl);
                    }

                    if (!string.IsNullOrEmpty(account.BankName))
                    {
                        Writer.WriteElementString("ram:Name", account.BankName);
                    }
                    Writer.WriteEndElement(); // !PayeeSpecifiedCreditorFinancialInstitution
                    Writer.WriteEndElement(); // !SpecifiedTradeSettlementPaymentMeans
                }

                foreach (BankAccount account in this.Descriptor.DebitorBankAccounts)
                {
                    Writer.WriteStartElement("ram:SpecifiedTradeSettlementPaymentMeans");

                    if ((this.Descriptor.PaymentMeans != null) && (this.Descriptor.PaymentMeans.TypeCode != PaymentMeansTypeCodes.Unknown))
                    {
                        Writer.WriteElementString("ram:TypeCode", this.Descriptor.PaymentMeans.TypeCode.EnumToString());
                        Writer.WriteElementString("ram:Information", this.Descriptor.PaymentMeans.Information);

                        if (!string.IsNullOrEmpty(this.Descriptor.PaymentMeans.SEPACreditorIdentifier) && !string.IsNullOrEmpty(this.Descriptor.PaymentMeans.SEPAMandateReference))
                        {
                            Writer.WriteStartElement("ram:ID");
                            Writer.WriteAttributeString("schemeAgencyID", this.Descriptor.PaymentMeans.SEPACreditorIdentifier);
                            Writer.WriteValue(this.Descriptor.PaymentMeans.SEPAMandateReference);
                            Writer.WriteEndElement(); // !ram:ID
                        }
                    }

                    Writer.WriteStartElement("ram:PayerPartyDebtorFinancialAccount");
                    Writer.WriteElementString("ram:IBANID", account.IBAN);
                    if (!string.IsNullOrEmpty(account.ID))
                    {
                        Writer.WriteElementString("ram:ProprietaryID", account.ID);
                    }
                    Writer.WriteEndElement(); // !PayerPartyDebtorFinancialAccount

                    Writer.WriteStartElement("ram:PayerSpecifiedDebtorFinancialInstitution");
                    Writer.WriteElementString("ram:BICID", account.BIC);

                    if (!string.IsNullOrEmpty(account.Bankleitzahl))
                    {
                        Writer.WriteElementString("ram:GermanBankleitzahlID", account.Bankleitzahl);
                    }

                    if (!string.IsNullOrEmpty(account.BankName))
                    {
                        Writer.WriteElementString("ram:Name", account.BankName);
                    }
                    Writer.WriteEndElement(); // !PayerSpecifiedDebtorFinancialInstitution
                    Writer.WriteEndElement(); // !SpecifiedTradeSettlementPaymentMeans
                }
            }

            _writeOptionalTaxes(Writer);

            if ((this.Descriptor.TradeAllowanceCharges != null) && (this.Descriptor.TradeAllowanceCharges.Count > 0))
            {
                foreach (TradeAllowanceCharge tradeAllowanceCharge in this.Descriptor.TradeAllowanceCharges)
                {
                    Writer.WriteStartElement("ram:SpecifiedTradeAllowanceCharge");
                    Writer.WriteStartElement("ram:ChargeIndicator");
                    Writer.WriteElementString("udt:Indicator", tradeAllowanceCharge.ChargeIndicator ? "true" : "false");
                    Writer.WriteEndElement(); // !ram:ChargeIndicator

                    Writer.WriteStartElement("ram:BasisAmount");
                    Writer.WriteAttributeString("currencyID", tradeAllowanceCharge.Currency.EnumToString());
                    Writer.WriteValue(_formatDecimal(tradeAllowanceCharge.BasisAmount));
                    Writer.WriteEndElement();

                    Writer.WriteStartElement("ram:ActualAmount");
                    Writer.WriteAttributeString("currencyID", tradeAllowanceCharge.Currency.EnumToString());
                    Writer.WriteValue(_formatDecimal(tradeAllowanceCharge.ActualAmount));
                    Writer.WriteEndElement();


                    _writeOptionalElementString(Writer, "ram:Reason", tradeAllowanceCharge.Reason);

                    if (tradeAllowanceCharge.Tax != null)
                    {
                        Writer.WriteStartElement("ram:CategoryTradeTax");
                        Writer.WriteElementString("ram:TypeCode", tradeAllowanceCharge.Tax.TypeCode.EnumToString());
                        if (tradeAllowanceCharge.Tax.CategoryCode.HasValue)
                            Writer.WriteElementString("ram:CategoryCode", tradeAllowanceCharge.Tax.CategoryCode?.EnumToString());
                        Writer.WriteElementString("ram:ApplicablePercent", _formatDecimal(tradeAllowanceCharge.Tax.Percent));
                        Writer.WriteEndElement();
                    }
                    Writer.WriteEndElement();
                }
            }

            if ((this.Descriptor.ServiceCharges != null) && (this.Descriptor.ServiceCharges.Count > 0))
            {
                foreach (ServiceCharge serviceCharge in this.Descriptor.ServiceCharges)
                {
                    Writer.WriteStartElement("ram:SpecifiedLogisticsServiceCharge");
                    if (!string.IsNullOrEmpty(serviceCharge.Description))
                    {
                        Writer.WriteElementString("ram:Description", serviceCharge.Description);
                    }
                    Writer.WriteElementString("ram:AppliedAmount", _formatDecimal(serviceCharge.Amount));
                    if (serviceCharge.Tax != null)
                    {
                        Writer.WriteStartElement("ram:AppliedTradeTax");
                        Writer.WriteElementString("ram:TypeCode", serviceCharge.Tax.TypeCode.EnumToString());
                        if (serviceCharge.Tax.CategoryCode.HasValue)
                            Writer.WriteElementString("ram:CategoryCode", serviceCharge.Tax.CategoryCode?.EnumToString());
                        Writer.WriteElementString("ram:ApplicablePercent", _formatDecimal(serviceCharge.Tax.Percent));
                        Writer.WriteEndElement();
                    }
                    Writer.WriteEndElement();
                }
            }

            if (this.Descriptor.PaymentTerms != null)
            {
                Writer.WriteStartElement("ram:SpecifiedTradePaymentTerms");
                _writeOptionalElementString(Writer, "ram:Description", this.Descriptor.PaymentTerms.Description);
                if (this.Descriptor.PaymentTerms.DueDate.HasValue)
                {
                    Writer.WriteStartElement("ram:DueDateDateTime");
                    _writeElementWithAttribute(Writer, "udt:DateTimeString", "format", "102", _formatDate(this.Descriptor.PaymentTerms.DueDate.Value));
                    Writer.WriteEndElement(); // !ram:DueDateDateTime
                }
                Writer.WriteEndElement();
            }

            Writer.WriteStartElement("ram:SpecifiedTradeSettlementMonetarySummation");
            _writeOptionalAmount(Writer, "ram:LineTotalAmount", this.Descriptor.LineTotalAmount);
            _writeOptionalAmount(Writer, "ram:ChargeTotalAmount", this.Descriptor.ChargeTotalAmount);
            _writeOptionalAmount(Writer, "ram:AllowanceTotalAmount", this.Descriptor.AllowanceTotalAmount);
            _writeOptionalAmount(Writer, "ram:TaxBasisTotalAmount", this.Descriptor.TaxBasisAmount);
            _writeOptionalAmount(Writer, "ram:TaxTotalAmount", this.Descriptor.TaxTotalAmount);
            _writeOptionalAmount(Writer, "ram:GrandTotalAmount", this.Descriptor.GrandTotalAmount);
            _writeOptionalAmount(Writer, "ram:TotalPrepaidAmount", this.Descriptor.TotalPrepaidAmount);
            _writeOptionalAmount(Writer, "ram:DuePayableAmount", this.Descriptor.DuePayableAmount);
            Writer.WriteEndElement(); // !ram:SpecifiedTradeSettlementMonetarySummation

            Writer.WriteEndElement(); // !ram:ApplicableSupplyChainTradeSettlement

            foreach (TradeLineItem tradeLineItem in this.Descriptor.TradeLineItems)
            {
                Writer.WriteStartElement("ram:IncludedSupplyChainTradeLineItem");

                if (tradeLineItem.AssociatedDocument != null)
                {
                    Writer.WriteStartElement("ram:AssociatedDocumentLineDocument");
                    if (tradeLineItem.AssociatedDocument.LineID.HasValue)
                    {
                        Writer.WriteElementString("ram:LineID", string.Format("{0}", tradeLineItem.AssociatedDocument.LineID));
                    }
                    _writeNotes(Writer, tradeLineItem.AssociatedDocument.Notes);
                    Writer.WriteEndElement(); // ram:AssociatedDocumentLineDocument
                }

                // handelt es sich um einen Kommentar?
                if ((tradeLineItem.AssociatedDocument?.Notes.Count > 0) && (tradeLineItem.BilledQuantity == 0) && (string.IsNullOrEmpty(tradeLineItem.Description)))
                {
                    Writer.WriteEndElement(); // !ram:IncludedSupplyChainTradeLineItem
                    continue;
                }

                if (Descriptor.Profile != Profile.Basic)
                {
                    Writer.WriteStartElement("ram:SpecifiedSupplyChainTradeAgreement");

                    if (tradeLineItem.BuyerOrderReferencedDocument != null)
                    {
                        Writer.WriteStartElement("ram:BuyerOrderReferencedDocument");
                        if (tradeLineItem.BuyerOrderReferencedDocument.IssueDateTime.HasValue)
                        {
                            Writer.WriteStartElement("ram:IssueDateTime");
                            Writer.WriteValue(_formatDate(tradeLineItem.BuyerOrderReferencedDocument.IssueDateTime.Value, false));
                            Writer.WriteEndElement(); // !ram:IssueDateTime
                        }
                        if (!string.IsNullOrEmpty(tradeLineItem.BuyerOrderReferencedDocument.ID))
                        {
                            Writer.WriteElementString("ram:ID", tradeLineItem.BuyerOrderReferencedDocument.ID);
                        }

                        Writer.WriteEndElement(); // !ram:BuyerOrderReferencedDocument
                    }

                    if (tradeLineItem.ContractReferencedDocument != null)
                    {
                        Writer.WriteStartElement("ram:ContractReferencedDocument");
                        if (tradeLineItem.ContractReferencedDocument.IssueDateTime.HasValue)
                        {
                            Writer.WriteStartElement("ram:IssueDateTime");
                            Writer.WriteValue(_formatDate(tradeLineItem.ContractReferencedDocument.IssueDateTime.Value, false));
                            Writer.WriteEndElement(); // !ram:IssueDateTime
                        }
                        if (!string.IsNullOrEmpty(tradeLineItem.ContractReferencedDocument.ID))
                        {
                            Writer.WriteElementString("ram:ID", tradeLineItem.ContractReferencedDocument.ID);
                        }

                        Writer.WriteEndElement(); // !ram:ContractReferencedDocument
                    }

                    if ((tradeLineItem.AdditionalReferencedDocuments != null) && (tradeLineItem.AdditionalReferencedDocuments.Count > 0))
                    {
                        foreach (AdditionalReferencedDocument doc in tradeLineItem.AdditionalReferencedDocuments)
                        {
                            Writer.WriteStartElement("ram:AdditionalReferencedDocument");
                            if (doc.IssueDateTime.HasValue)
                            {
                                Writer.WriteStartElement("ram:IssueDateTime");
                                Writer.WriteValue(_formatDate(doc.IssueDateTime.Value, false));
                                Writer.WriteEndElement(); // !ram:IssueDateTime
                            }

                            Writer.WriteElementString("ram:LineID", string.Format("{0}", tradeLineItem.AssociatedDocument?.LineID));

                            if (!string.IsNullOrEmpty(doc.ID))
                            {
                                Writer.WriteElementString("ram:ID", doc.ID);
                            }

                            Writer.WriteElementString("ram:ReferenceTypeCode", doc.ReferenceTypeCode.EnumToString());

                            Writer.WriteEndElement(); // !ram:AdditionalReferencedDocument
                        }
                    }

                    Writer.WriteStartElement("ram:GrossPriceProductTradePrice");
                    _writeOptionalAmount(Writer, "ram:ChargeAmount", tradeLineItem.GrossUnitPrice, 4);
                    if (tradeLineItem.UnitQuantity.HasValue)
                    {
                        _writeElementWithAttribute(Writer, "ram:BasisQuantity", "unitCode", tradeLineItem.UnitCode.EnumToString(), _formatDecimal(tradeLineItem.UnitQuantity.Value, 4));
                    }

                    foreach (TradeAllowanceCharge tradeAllowanceCharge in tradeLineItem.TradeAllowanceCharges)
                    {
                        Writer.WriteStartElement("ram:AppliedTradeAllowanceCharge");

                        Writer.WriteStartElement("ram:ChargeIndicator");
                        Writer.WriteElementString("udt:Indicator", tradeAllowanceCharge.ChargeIndicator ? "true" : "false");
                        Writer.WriteEndElement(); // !ram:ChargeIndicator

                        Writer.WriteStartElement("ram:BasisAmount");
                        Writer.WriteAttributeString("currencyID", tradeAllowanceCharge.Currency.EnumToString());
                        Writer.WriteValue(_formatDecimal(tradeAllowanceCharge.BasisAmount, 4));
                        Writer.WriteEndElement();
                        Writer.WriteStartElement("ram:ActualAmount");
                        Writer.WriteAttributeString("currencyID", tradeAllowanceCharge.Currency.EnumToString());
                        Writer.WriteValue(_formatDecimal(tradeAllowanceCharge.ActualAmount, 4));
                        Writer.WriteEndElement();

                        _writeOptionalElementString(Writer, "ram:Reason", tradeAllowanceCharge.Reason);

                        Writer.WriteEndElement(); // !AppliedTradeAllowanceCharge
                    }

                    Writer.WriteEndElement(); // ram:GrossPriceProductTradePrice

                    Writer.WriteStartElement("ram:NetPriceProductTradePrice");
                    _writeOptionalAmount(Writer, "ram:ChargeAmount", tradeLineItem.NetUnitPrice, 4);

                    if (tradeLineItem.UnitQuantity.HasValue)
                    {
                        _writeElementWithAttribute(Writer, "ram:BasisQuantity", "unitCode", tradeLineItem.UnitCode.EnumToString(), _formatDecimal(tradeLineItem.UnitQuantity.Value, 4));
                    }
                    Writer.WriteEndElement(); // ram:NetPriceProductTradePrice

                    Writer.WriteEndElement(); // !ram:SpecifiedSupplyChainTradeAgreement
                }

                if (Descriptor.Profile != Profile.Basic)
                {
                    Writer.WriteStartElement("ram:SpecifiedSupplyChainTradeDelivery");
                    _writeElementWithAttribute(Writer, "ram:BilledQuantity", "unitCode", tradeLineItem.UnitCode.EnumToString(), _formatDecimal(tradeLineItem.BilledQuantity, 4));

                    if (tradeLineItem.DeliveryNoteReferencedDocument != null)
                    {
                        Writer.WriteStartElement("ram:DeliveryNoteReferencedDocument");
                        if (tradeLineItem.DeliveryNoteReferencedDocument.IssueDateTime.HasValue)
                        {
                            Writer.WriteStartElement("ram:IssueDateTime");
                            Writer.WriteValue(_formatDate(tradeLineItem.DeliveryNoteReferencedDocument.IssueDateTime.Value, false));
                            Writer.WriteEndElement(); // !ram:IssueDateTime
                        }
                        if (!string.IsNullOrEmpty(tradeLineItem.DeliveryNoteReferencedDocument.ID))
                        {
                            Writer.WriteElementString("ram:ID", tradeLineItem.DeliveryNoteReferencedDocument.ID);
                        }

                        Writer.WriteEndElement(); // !ram:DeliveryNoteReferencedDocument
                    }

                    if (tradeLineItem.ActualDeliveryDate.HasValue)
                    {
                        Writer.WriteStartElement("ram:ActualDeliverySupplyChainEvent");
                        Writer.WriteStartElement("ram:OccurrenceDateTime");
                        Writer.WriteStartElement("udt:DateTimeString");
                        Writer.WriteAttributeString("format", "102");
                        Writer.WriteValue(_formatDate(tradeLineItem.ActualDeliveryDate.Value));
                        Writer.WriteEndElement(); // "udt:DateTimeString
                        Writer.WriteEndElement(); // !OccurrenceDateTime()
                        Writer.WriteEndElement(); // !ActualDeliverySupplyChainEvent
                    }

                    Writer.WriteEndElement(); // !ram:SpecifiedSupplyChainTradeDelivery
                }
                else
                {
                    Writer.WriteStartElement("ram:SpecifiedSupplyChainTradeDelivery");
                    _writeElementWithAttribute(Writer, "ram:BilledQuantity", "unitCode", tradeLineItem.UnitCode.EnumToString(), _formatDecimal(tradeLineItem.BilledQuantity, 4));
                    Writer.WriteEndElement(); // !ram:SpecifiedSupplyChainTradeDelivery
                }

                Writer.WriteStartElement("ram:SpecifiedSupplyChainTradeSettlement");

                if (Descriptor.Profile != Profile.Basic)
                {
                    Writer.WriteStartElement("ram:ApplicableTradeTax");
                    Writer.WriteElementString("ram:TypeCode", tradeLineItem.TaxType.EnumToString());
                    Writer.WriteElementString("ram:CategoryCode", tradeLineItem.TaxCategoryCode.EnumToString());
                    Writer.WriteElementString("ram:ApplicablePercent", _formatDecimal(tradeLineItem.TaxPercent));
                    Writer.WriteEndElement(); // !ram:ApplicableTradeTax
                }
                Writer.WriteStartElement("ram:SpecifiedTradeSettlementMonetarySummation");

                decimal _total = 0m;

                if (tradeLineItem.LineTotalAmount.HasValue)
                {
                    _total = tradeLineItem.LineTotalAmount.Value;
                }
                else
                {
                    _total = tradeLineItem.NetUnitPrice * tradeLineItem.BilledQuantity;
                }

                _writeElementWithAttribute(Writer, "ram:LineTotalAmount", "currencyID", this.Descriptor.Currency.EnumToString(), _formatDecimal(_total));
                Writer.WriteEndElement(); // ram:SpecifiedTradeSettlementMonetarySummation
                Writer.WriteEndElement(); // !ram:SpecifiedSupplyChainTradeSettlement

                Writer.WriteStartElement("ram:SpecifiedTradeProduct");
                if ((tradeLineItem.GlobalID != null) && !string.IsNullOrEmpty(tradeLineItem.GlobalID.SchemeID) && !string.IsNullOrEmpty(tradeLineItem.GlobalID.ID))
                {
                    _writeElementWithAttribute(Writer, "ram:GlobalID", "schemeID", tradeLineItem.GlobalID.SchemeID, tradeLineItem.GlobalID.ID);
                }

                _writeOptionalElementString(Writer, "ram:SellerAssignedID", tradeLineItem.SellerAssignedID);
                _writeOptionalElementString(Writer, "ram:BuyerAssignedID", tradeLineItem.BuyerAssignedID);
                _writeOptionalElementString(Writer, "ram:Name", tradeLineItem.Name);
                _writeOptionalElementString(Writer, "ram:Description", tradeLineItem.Description);

                Writer.WriteEndElement(); // !ram:SpecifiedTradeProduct
                Writer.WriteEndElement(); // !ram:IncludedSupplyChainTradeLineItem
            } // !foreach(tradeLineItem)

            Writer.WriteEndElement(); // !ram:SpecifiedSupplyChainTradeTransaction
            #endregion

            Writer.WriteEndElement(); // !ram:Invoice
            Writer.WriteEndDocument();
            Writer.Flush();

            stream.Seek(streamPosition, SeekOrigin.Begin);
        } // !Save()      


        private void _writeOptionalAmount(XmlTextWriter writer, string tagName, decimal value, int numDecimals = 2)
        {
            if (value != decimal.MinValue)
            {
                writer.WriteStartElement(tagName);
                writer.WriteAttributeString("currencyID", this.Descriptor.Currency.EnumToString());
                writer.WriteValue(_formatDecimal(value, numDecimals));
                writer.WriteEndElement(); // !tagName
            }
        } // !_writeOptionalAmount()


        private void _writeElementWithAttribute(XmlTextWriter writer, string tagName, string attributeName, string attributeValue, string nodeValue)
        {
            writer.WriteStartElement(tagName);
            writer.WriteAttributeString(attributeName, attributeValue);
            writer.WriteValue(nodeValue);
            writer.WriteEndElement(); // !tagName
        } // !_writeElementWithAttribute()


        private void _writeOptionalTaxes(XmlTextWriter writer)
        {
            foreach (Tax tax in this.Descriptor.Taxes)
            {
                writer.WriteStartElement("ram:ApplicableTradeTax");

                writer.WriteStartElement("ram:CalculatedAmount");
                writer.WriteAttributeString("currencyID", this.Descriptor.Currency.EnumToString());
                writer.WriteValue(_formatDecimal(tax.TaxAmount));
                writer.WriteEndElement(); // !CalculatedAmount

                writer.WriteElementString("ram:TypeCode", tax.TypeCode.EnumToString());

                writer.WriteStartElement("ram:BasisAmount");
                writer.WriteAttributeString("currencyID", this.Descriptor.Currency.EnumToString());
                writer.WriteValue(_formatDecimal(tax.BasisAmount));
                writer.WriteEndElement(); // !BasisAmount

                if (tax.AllowanceChargeBasisAmount != 0)
                {
                    writer.WriteStartElement("ram:AllowanceChargeBasisAmount");
                    writer.WriteAttributeString("currencyID", this.Descriptor.Currency.EnumToString());
                    writer.WriteValue(_formatDecimal(tax.AllowanceChargeBasisAmount));
                    writer.WriteEndElement(); // !AllowanceChargeBasisAmount
                }

                if (tax.CategoryCode.HasValue)
                {
                    writer.WriteElementString("ram:CategoryCode", tax.CategoryCode?.EnumToString());
                }
                writer.WriteElementString("ram:ApplicablePercent", _formatDecimal(tax.Percent));
                writer.WriteEndElement(); // !ApplicableTradeTax
            }
        } // !_writeOptionalTaxes()


        private void _writeNotes(XmlTextWriter writer, List<Note> notes)
        {
            if (notes.Count > 0)
            {
                foreach (Note note in notes)
                {
                    writer.WriteStartElement("ram:IncludedNote");
                    if (note.ContentCode != ContentCodes.Unknown)
                    {
                        writer.WriteElementString("ram:ContentCode", note.ContentCode.EnumToString());
                    }
                    writer.WriteElementString("ram:Content", note.Content);
                    if (note.SubjectCode != SubjectCodes.Unknown)
                    {
                        writer.WriteElementString("ram:SubjectCode", note.SubjectCode.EnumToString());
                    }
                    writer.WriteEndElement();
                }
            }
        } // !_writeNotes()


        private void _writeOptionalParty(XmlTextWriter writer, string PartyTag, Party Party, Contact Contact = null, List<TaxRegistration> TaxRegistrations = null)
        {
            if (Party != null)
            {
                writer.WriteStartElement(PartyTag);

                if (!string.IsNullOrEmpty(Party.ID))
                {
                    writer.WriteElementString("ram:ID", Party.ID);
                }

                if ((Party.GlobalID != null) && !string.IsNullOrEmpty(Party.GlobalID.ID) && !string.IsNullOrEmpty(Party.GlobalID.SchemeID))
                {
                    writer.WriteStartElement("ram:GlobalID");
                    writer.WriteAttributeString("schemeID", Party.GlobalID.SchemeID);
                    writer.WriteValue(Party.GlobalID.ID);
                    writer.WriteEndElement();
                }

                if (!string.IsNullOrEmpty(Party.Name))
                {
                    writer.WriteElementString("ram:Name", Party.Name);
                }

                if (Contact != null)
                {
                    _writeOptionalContact(writer, "ram:DefinedTradeContact", Contact);
                }

                writer.WriteStartElement("ram:PostalTradeAddress");
                writer.WriteElementString("ram:PostcodeCode", Party.Postcode);
                writer.WriteElementString("ram:LineOne", string.IsNullOrEmpty(Party.ContactName) ? Party.Street : Party.ContactName);
                if (!string.IsNullOrEmpty(Party.ContactName))
                    writer.WriteElementString("ram:LineTwo", Party.Street);
                writer.WriteElementString("ram:CityName", Party.City);
                writer.WriteElementString("ram:CountryID", Party.Country.EnumToString());
                writer.WriteEndElement(); // !PostalTradeAddress

                if (TaxRegistrations != null)
                {
                    foreach (TaxRegistration _reg in TaxRegistrations)
                    {
                        if (!string.IsNullOrEmpty(_reg.No))
                        {
                            writer.WriteStartElement("ram:SpecifiedTaxRegistration");
                            writer.WriteStartElement("ram:ID");
                            writer.WriteAttributeString("schemeID", _reg.SchemeID.EnumToString());
                            writer.WriteValue(_reg.No);
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                        }
                    }
                }
                writer.WriteEndElement(); // !*TradeParty
            }
        } // !_writeOptionalParty()


        private void _writeOptionalContact(XmlTextWriter writer, string contactTag, Contact contact)
        {
            if (contact != null)
            {
                writer.WriteStartElement(contactTag);

                if (!string.IsNullOrEmpty(contact.Name))
                {
                    writer.WriteElementString("ram:PersonName", contact.Name);
                }

                if (!string.IsNullOrEmpty(contact.OrgUnit))
                {
                    writer.WriteElementString("ram:DepartmentName", contact.OrgUnit);
                }

                if (!string.IsNullOrEmpty(contact.PhoneNo))
                {
                    writer.WriteStartElement("ram:TelephoneUniversalCommunication");
                    writer.WriteElementString("ram:CompleteNumber", contact.PhoneNo);
                    writer.WriteEndElement();
                }

                if (!string.IsNullOrEmpty(contact.FaxNo))
                {
                    writer.WriteStartElement("ram:FaxUniversalCommunication");
                    writer.WriteElementString("ram:CompleteNumber", contact.FaxNo);
                    writer.WriteEndElement();
                }

                if (!string.IsNullOrEmpty(contact.EmailAddress))
                {
                    writer.WriteStartElement("ram:EmailURIUniversalCommunication");
                    writer.WriteElementString("ram:URIID", contact.EmailAddress);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        } // !_writeOptionalContact()


        private void _writeOptionalElementString(XmlTextWriter writer, string tagName, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                writer.WriteElementString(tagName, value);
            }
        } // !_writeOptionalElementString()


        private string _formatDecimal(decimal value, int numDecimals = 2)
        {
            string formatString = "0.";
            for (int i = 0; i < numDecimals; i++)
            {
                formatString += "0";
            }

            return value.ToString(formatString).Replace(",", ".");
        } // !_formatDecimal()


        private string _translateInvoiceType(InvoiceType type)
        {
            switch (type)
            {
                case InvoiceType.Invoice: return "RECHNUNG";
                case InvoiceType.Correction: return "KORREKTURRECHNUNG";
                case InvoiceType.CreditNote: return "GUTSCHRIFT";
                case InvoiceType.DebitNote: return "";
                case InvoiceType.SelfBilledInvoice: return "";
                default: return "";
            }
        } // !_translateInvoiceType()


        private int _encodeInvoiceType(InvoiceType type)
        {
            if ((int)type > 1000)
            {
                type -= 1000;
            }

            // only these types are allowed
            // 84: 'Wertbelastung/Wertrechnung ohne Warenbezug'
            // 380: 'Handelsrechnung (Rechnung für Waren und Dienstleistungen)'
            // 389: 'Selbst ausgestellte Rechnung (Steuerrechtliche Gutschrift/Gutschriftsverfahren)'
            switch (type)
            {
                case InvoiceType.Correction: return (int)InvoiceType.Invoice;
                case InvoiceType.CreditNote: return (int)InvoiceType.Invoice;
                case InvoiceType.DebitNote: return (int)InvoiceType.Invoice;
                case InvoiceType.Invoice: return (int)InvoiceType.Invoice;
                case InvoiceType.SelfBilledInvoice: return (int)InvoiceType.SelfBilledInvoice;
                default: return (int)InvoiceType.Unknown;
            }
        } // !_translateInvoiceType()


        private string _formatDate(DateTime date, bool formatAs102 = true)
        {
            if (formatAs102)
            {
                return date.ToString("yyyyMMdd");
            }

            return date.ToString("yyyy-MM-ddTHH:mm:ss");
        } // !_formatDate()
    }
}

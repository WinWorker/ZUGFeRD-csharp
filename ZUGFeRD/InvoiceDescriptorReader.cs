using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using s2industries.ZUGFeRD;

namespace ZUGFeRD
{
    internal class InvoiceDescriptorReader : IInvoiceDescriptorReader
    {
        public override InvoiceDescriptor Load(Stream stream)
        {
            return null;
        }

        public override bool IsReadableByThisReaderVersion(Stream stream) => false;

        public bool IsReadableByThisReaderVersion(Stream stream, ZugferdDialect zugferdDialect)
        {
            if (!stream.CanRead)
            {
                throw new IllegalStreamException("Cannot read from stream");
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(stream);

            if (doc.DocumentElement?.OwnerDocument?.NameTable == null)
            {
                return false;
            }
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.DocumentElement.OwnerDocument.NameTable);

            foreach ((string item1, string item2) in zugferdDialect.GetNamespaces())
            {
                nsmgr.AddNamespace(item1, item2);
            }

            string profile = NodeAsString(doc.DocumentElement, "//ram:GuidelineSpecifiedDocumentContextParameter/ram:ID", nsmgr);
            if (profile.StartsWith(zugferdDialect.DocumentId))
            {
                return true;
            }
            return false;
        }

        public InvoiceDescriptor Load(Stream stream, ZugferdDialect zugferdDialect)
        {
            if (!stream.CanRead)
            {
                throw new IllegalStreamException("Cannot read from stream");
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(stream);

            if (doc.DocumentElement?.OwnerDocument?.NameTable == null)
            {
                return null;
            }
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.DocumentElement.OwnerDocument.NameTable);

            foreach ((string item1, string item2) in zugferdDialect.GetNamespaces())
            {
                nsmgr.AddNamespace(item1, item2);
            }

            InvoiceDescriptor retval;
            retval = new InvoiceDescriptor();
            retval.IsTest = NodeAsBool(doc.DocumentElement, $"//rsm:{zugferdDialect.ExchangedDocument}/ram:TestIndicator", nsmgr);
            retval.Profile = default(Profile).FromString(NodeAsString(doc.DocumentElement, "//ram:GuidelineSpecifiedDocumentContextParameter/ram:ID", nsmgr));
            retval.Type = default(InvoiceType).FromString(NodeAsString(doc.DocumentElement, $"//rsm:{zugferdDialect.ExchangedDocument}/ram:TypeCode", nsmgr));
            retval.InvoiceNo = NodeAsString(doc.DocumentElement, $"//rsm:{zugferdDialect.ExchangedDocument}/ram:ID", nsmgr);
            retval.InvoiceDate = NodeAsDateTime(doc.DocumentElement, $"//rsm:{zugferdDialect.ExchangedDocument}/ram:IssueDateTime/udt:DateTimeString", nsmgr);

            foreach (XmlNode node in doc.SelectNodes($"//rsm:{zugferdDialect.ExchangedDocument}/ram:IncludedNote", nsmgr))
            {
                string content = NodeAsString(node, ".//ram:Content", nsmgr);
                string subjectCode = NodeAsString(node, ".//ram:SubjectCode", nsmgr);
                SubjectCodes subjectCodeEnum = default(SubjectCodes).FromString(subjectCode);
                retval.AddNote(content, subjectCodeEnum);
            }

            retval.ReferenceOrderNo = NodeAsString(doc, $"//ram:{zugferdDialect.ApplicableTradeAgreement}/ram:BuyerReference", nsmgr);

            retval.Seller = NodeAsParty(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeAgreement}/ram:SellerTradeParty", nsmgr);
            foreach (XmlNode node in doc.SelectNodes($"//ram:{zugferdDialect.ApplicableTradeAgreement}/ram:SellerTradeParty/ram:SpecifiedTaxRegistration", nsmgr))
            {
                string id = NodeAsString(node, ".//ram:ID", nsmgr);
                string schemeID = NodeAsString(node, ".//ram:ID/@schemeID", nsmgr);

                retval.AddSellerTaxRegistration(id, default(TaxRegistrationSchemeID).FromString(schemeID));
            }

            if (doc.SelectSingleNode("//ram:SellerTradeParty/ram:DefinedTradeContact", nsmgr) != null)
            {
                retval.SellerContact = new Contact
                {
                    Name = NodeAsString(doc.DocumentElement, "//ram:SellerTradeParty/ram:DefinedTradeContact/ram:PersonName", nsmgr),
                    OrgUnit = NodeAsString(doc.DocumentElement, "//ram:SellerTradeParty/ram:DefinedTradeContact/ram:DepartmentName", nsmgr),
                    PhoneNo = NodeAsString(doc.DocumentElement, "//ram:SellerTradeParty/ram:DefinedTradeContact/ram:TelephoneUniversalCommunication/ram:CompleteNumber", nsmgr),
                    FaxNo = NodeAsString(doc.DocumentElement, "//ram:SellerTradeParty/ram:DefinedTradeContact/ram:FaxUniversalCommunication/ram:CompleteNumber", nsmgr),
                    EmailAddress = NodeAsString(doc.DocumentElement, "//ram:SellerTradeParty/ram:DefinedTradeContact/ram:EmailURIUniversalCommunication/ram:CompleteNumber", nsmgr)
                };
            }

            retval.Buyer = NodeAsParty(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeAgreement}/ram:BuyerTradeParty", nsmgr);
            foreach (XmlNode node in doc.SelectNodes($"//ram:{zugferdDialect.ApplicableTradeAgreement}/ram:BuyerTradeParty/ram:SpecifiedTaxRegistration", nsmgr))
            {
                string id = NodeAsString(node, ".//ram:ID", nsmgr);
                string schemeID = NodeAsString(node, ".//ram:ID/@schemeID", nsmgr);

                retval.AddBuyerTaxRegistration(id, default(TaxRegistrationSchemeID).FromString(schemeID));
            }

            if (doc.SelectSingleNode("//ram:BuyerTradeParty/ram:DefinedTradeContact", nsmgr) != null)
            {
                retval.BuyerContact = new Contact
                {
                    Name = NodeAsString(doc.DocumentElement, "//ram:BuyerTradeParty/ram:DefinedTradeContact/ram:PersonName", nsmgr),
                    OrgUnit = NodeAsString(doc.DocumentElement, "//ram:BuyerTradeParty/DefinedTradeContact/ram:DepartmentName", nsmgr),
                    PhoneNo = NodeAsString(doc.DocumentElement, "//ram:BuyerTradeParty/ram:DefinedTradeContact/ram:TelephoneUniversalCommunication/ram:CompleteNumber", nsmgr),
                    FaxNo = NodeAsString(doc.DocumentElement, "//ram:BuyerTradeParty/ram:DefinedTradeContact/ram:FaxUniversalCommunication/ram:CompleteNumber", nsmgr),
                    EmailAddress = NodeAsString(doc.DocumentElement, "//ram:BuyerTradeParty/ram:DefinedTradeContact/ram:EmailURIUniversalCommunication/ram:CompleteNumber", nsmgr)
                };
            }

            retval.ShipTo = NodeAsParty(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeDelivery}/ram:ShipToTradeParty", nsmgr);
            retval.ShipFrom = NodeAsParty(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeDelivery}/ram:ShipFromTradeParty", nsmgr);
            retval.ActualDeliveryDate = NodeAsDateTime(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeDelivery}/ram:ActualDeliverySupplyChainEvent/ram:OccurrenceDateTime/udt:DateTimeString", nsmgr);

            string deliveryNoteNo = NodeAsString(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeDelivery}/ram:DeliveryNoteReferencedDocument/ram:ID", nsmgr);
            DateTime? deliveryNoteDate = NodeAsDateTime(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeDelivery}/ram:DeliveryNoteReferencedDocument/ram:IssueDateTime/udt:DateTimeString", nsmgr) ?? NodeAsDateTime(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeDelivery}/ram:DeliveryNoteReferencedDocument/ram:IssueDateTime", nsmgr);

            if (deliveryNoteDate.HasValue || !string.IsNullOrEmpty(deliveryNoteNo))
            {
                retval.DeliveryNoteReferencedDocument = new DeliveryNoteReferencedDocument
                {
                    ID = deliveryNoteNo,
                    IssueDateTime = deliveryNoteDate
                };
            }

            retval.Invoicee = NodeAsParty(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeSettlement}/ram:InvoiceeTradeParty", nsmgr);
            retval.Payee = NodeAsParty(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeSettlement}/ram:PayeeTradeParty", nsmgr);

            retval.InvoiceNoAsReference = NodeAsString(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeSettlement}/ram:PaymentReference", nsmgr);
            retval.Currency = default(CurrencyCodes).FromString(NodeAsString(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeSettlement}/ram:InvoiceCurrencyCode", nsmgr));

            // TODO: Multiple SpecifiedTradeSettlementPaymentMeans can exist for each account/institution (with different SEPA?)
            PaymentMeans tempPaymentMeans = new PaymentMeans
            {
                TypeCode = default(PaymentMeansTypeCodes).FromString(NodeAsString(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeSettlement}/ram:SpecifiedTradeSettlementPaymentMeans/ram:TypeCode", nsmgr)),
                Information = NodeAsString(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeSettlement}/ram:SpecifiedTradeSettlementPaymentMeans/ram:Information", nsmgr),
                SEPACreditorIdentifier = NodeAsString(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeSettlement}/ram:SpecifiedTradeSettlementPaymentMeans/ram:ID", nsmgr),
                SEPAMandateReference = NodeAsString(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeSettlement}/ram:SpecifiedTradeSettlementPaymentMeans/ram:ID/@schemeAgencyID", nsmgr)
            };
            retval.PaymentMeans = tempPaymentMeans;

            XmlNodeList creditorFinancialAccountNodes = doc.SelectNodes($"//ram:{zugferdDialect.ApplicableTradeSettlement}/ram:SpecifiedTradeSettlementPaymentMeans/ram:PayeePartyCreditorFinancialAccount", nsmgr);
            XmlNodeList creditorFinancialInstitutions = doc.SelectNodes($"//ram:{zugferdDialect.ApplicableTradeSettlement}/ram:SpecifiedTradeSettlementPaymentMeans/ram:PayeeSpecifiedCreditorFinancialInstitution", nsmgr);

            if (creditorFinancialAccountNodes.Count == creditorFinancialInstitutions.Count)
            {
                for (int i = 0; i < creditorFinancialAccountNodes.Count; i++)
                {
                    BankAccount account = new BankAccount
                    {
                        ID = NodeAsString(creditorFinancialAccountNodes[0], ".//ram:ProprietaryID", nsmgr),
                        IBAN = NodeAsString(creditorFinancialAccountNodes[0], ".//ram:IBANID", nsmgr),
                        BIC = NodeAsString(creditorFinancialInstitutions[0], ".//ram:BICID", nsmgr),
                        Bankleitzahl = NodeAsString(creditorFinancialInstitutions[0], ".//ram:GermanBankleitzahlID", nsmgr),
                        BankName = NodeAsString(creditorFinancialInstitutions[0], ".//ram:Name", nsmgr),
                        Name = NodeAsString(creditorFinancialInstitutions[0], ".//ram:AccountName", nsmgr)
                    };

                    retval.CreditorBankAccounts.Add(account);
                } 
            }

            XmlNodeList debitorFinancialAccountNodes = doc.SelectNodes($"//ram:{zugferdDialect.ApplicableTradeSettlement}/ram:SpecifiedTradeSettlementPaymentMeans/ram:PayerPartyDebtorFinancialAccount", nsmgr);
            XmlNodeList debitorFinancialInstitutions = doc.SelectNodes($"//ram:{zugferdDialect.ApplicableTradeSettlement}/ram:SpecifiedTradeSettlementPaymentMeans/ram:PayerSpecifiedDebtorFinancialInstitution", nsmgr);

            if (debitorFinancialAccountNodes.Count == debitorFinancialInstitutions.Count)
            {
                for (int i = 0; i < debitorFinancialAccountNodes.Count; i++)
                {
                    BankAccount account = new BankAccount
                    {
                        ID = NodeAsString(debitorFinancialAccountNodes[0], ".//ram:ProprietaryID", nsmgr),
                        IBAN = NodeAsString(debitorFinancialAccountNodes[0], ".//ram:IBANID", nsmgr),
                        BIC = NodeAsString(debitorFinancialInstitutions[0], ".//ram:BICID", nsmgr),
                        Bankleitzahl = NodeAsString(debitorFinancialInstitutions[0], ".//ram:GermanBankleitzahlID", nsmgr),
                        BankName = NodeAsString(debitorFinancialInstitutions[0], ".//ram:Name", nsmgr)
                    };

                    retval.DebitorBankAccounts.Add(account);
                } // !for(i)
            }

            foreach (XmlNode node in doc.SelectNodes($"//ram:{zugferdDialect.ApplicableTradeSettlement}/ram:ApplicableTradeTax", nsmgr))
            {
                retval.AddApplicableTradeTax(NodeAsDecimal(node, ".//ram:BasisAmount", nsmgr, 0).Value,
                                             NodeAsDecimal(node, $".//ram:{zugferdDialect.ApplicablePercent}", nsmgr, 0).Value,
                                             default(TaxTypes).FromString(NodeAsString(node, ".//ram:TypeCode", nsmgr)),
                                             default(TaxCategoryCodes).FromString(NodeAsString(node, ".//ram:CategoryCode", nsmgr)));
            }

            foreach (XmlNode node in doc.SelectNodes("//ram:SpecifiedTradeAllowanceCharge", nsmgr))
            {
                retval.AddTradeAllowanceCharge(!NodeAsBool(node, ".//ram:ChargeIndicator", nsmgr), // wichtig: das not (!) beachten
                                               NodeAsDecimal(node, ".//ram:BasisAmount", nsmgr, 0).Value,
                                               retval.Currency,
                                               NodeAsDecimal(node, ".//ram:ActualAmount", nsmgr, 0).Value,
                                               NodeAsString(node, ".//ram:Reason", nsmgr),
                                               default(TaxTypes).FromString(NodeAsString(node, ".//ram:CategoryTradeTax/ram:TypeCode", nsmgr)),
                                               default(TaxCategoryCodes).FromString(NodeAsString(node, ".//ram:CategoryTradeTax/ram:CategoryCode", nsmgr)),
                                               NodeAsDecimal(node, $".//ram:CategoryTradeTax/ram:{zugferdDialect.ApplicablePercent}", nsmgr, 0).Value);
            }

            foreach (XmlNode node in doc.SelectNodes("//ram:SpecifiedLogisticsServiceCharge", nsmgr))
            {
                retval.AddLogisticsServiceCharge(NodeAsDecimal(node, ".//ram:AppliedAmount", nsmgr, 0).Value,
                                                 NodeAsString(node, ".//ram:Description", nsmgr),
                                                 default(TaxTypes).FromString(NodeAsString(node, ".//ram:AppliedTradeTax/ram:TypeCode", nsmgr)),
                                                 default(TaxCategoryCodes).FromString(NodeAsString(node, ".//ram:AppliedTradeTax/ram:CategoryCode", nsmgr)),
                                                 NodeAsDecimal(node, $".//ram:AppliedTradeTax/ram:{zugferdDialect.ApplicablePercent}", nsmgr, 0).Value);
            }

            retval.PaymentTerms = new PaymentTerms
            {
                Description = NodeAsString(doc.DocumentElement, "//ram:SpecifiedTradePaymentTerms/ram:Description", nsmgr),
                DueDate = NodeAsDateTime(doc.DocumentElement, "//ram:SpecifiedTradePaymentTerms/ram:DueDateDateTime", nsmgr)
            };

            retval.LineTotalAmount = NodeAsDecimal(doc.DocumentElement, $"//ram:{zugferdDialect.SpecifiedTradeSettlementMonetarySummation}/ram:LineTotalAmount", nsmgr, 0).Value;
            retval.ChargeTotalAmount = NodeAsDecimal(doc.DocumentElement, $"//ram:{zugferdDialect.SpecifiedTradeSettlementMonetarySummation}/ram:ChargeTotalAmount", nsmgr, 0).Value;
            retval.AllowanceTotalAmount = NodeAsDecimal(doc.DocumentElement, $"//ram:{zugferdDialect.SpecifiedTradeSettlementMonetarySummation}/ram:AllowanceTotalAmount", nsmgr, 0).Value;
            retval.TaxBasisAmount = NodeAsDecimal(doc.DocumentElement, $"//ram:{zugferdDialect.SpecifiedTradeSettlementMonetarySummation}/ram:TaxBasisTotalAmount", nsmgr, 0).Value;
            retval.TaxTotalAmount = NodeAsDecimal(doc.DocumentElement, $"//ram:{zugferdDialect.SpecifiedTradeSettlementMonetarySummation}/ram:TaxTotalAmount", nsmgr, 0).Value;
            retval.GrandTotalAmount = NodeAsDecimal(doc.DocumentElement, $"//ram:{zugferdDialect.SpecifiedTradeSettlementMonetarySummation}/ram:GrandTotalAmount", nsmgr, 0).Value;
            retval.TotalPrepaidAmount = NodeAsDecimal(doc.DocumentElement, $"//ram:{zugferdDialect.SpecifiedTradeSettlementMonetarySummation}/ram:TotalPrepaidAmount", nsmgr, 0).Value;
            retval.DuePayableAmount = NodeAsDecimal(doc.DocumentElement, $"//ram:{zugferdDialect.SpecifiedTradeSettlementMonetarySummation}/ram:DuePayableAmount", nsmgr, 0).Value;

            retval.OrderDate = NodeAsDateTime(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeAgreement}/ram:BuyerOrderReferencedDocument/ram:IssueDateTime/udt:DateTimeString", nsmgr) ?? NodeAsDateTime(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeAgreement}/ram:BuyerOrderReferencedDocument/ram:IssueDateTime", nsmgr);
            retval.OrderNo = NodeAsString(doc.DocumentElement, $"//ram:{zugferdDialect.ApplicableTradeAgreement}/ram:BuyerOrderReferencedDocument/ram:ID", nsmgr);

            foreach (XmlNode node in doc.SelectNodes("//ram:IncludedSupplyChainTradeLineItem", nsmgr))
            {
                retval.TradeLineItems.Add(ParseTradeLineItem(node, nsmgr, zugferdDialect));
            }
            return retval;
        }


        #region Additional Methods

        private static TradeLineItem ParseTradeLineItem(XmlNode tradeLineItem, XmlNamespaceManager nsmgr, ZugferdDialect zugferdDialect)
        {
            if (tradeLineItem == null)
            {
                return null;
            }

            TradeLineItem item = new TradeLineItem
            {
                GlobalID = new GlobalID(NodeAsString(tradeLineItem, ".//ram:SpecifiedTradeProduct/ram:GlobalID/@schemeID", nsmgr),
                                        NodeAsString(tradeLineItem, ".//ram:SpecifiedTradeProduct/ram:GlobalID", nsmgr)),
                SellerAssignedID = NodeAsString(tradeLineItem, ".//ram:SpecifiedTradeProduct/ram:SellerAssignedID", nsmgr),
                BuyerAssignedID = NodeAsString(tradeLineItem, ".//ram:SpecifiedTradeProduct/ram:BuyerAssignedID", nsmgr),
                Name = NodeAsString(tradeLineItem, ".//ram:SpecifiedTradeProduct/ram:Name", nsmgr),
                Description = NodeAsString(tradeLineItem, ".//ram:SpecifiedTradeProduct/ram:Description", nsmgr),
                UnitQuantity = NodeAsDecimal(tradeLineItem, ".//ram:BasisQuantity", nsmgr, 1),
                BilledQuantity = NodeAsDecimal(tradeLineItem, ".//ram:BilledQuantity", nsmgr, 0).Value,
                LineTotalAmount = NodeAsDecimal(tradeLineItem, ".//ram:LineTotalAmount", nsmgr, 0),
                TaxCategoryCode = default(TaxCategoryCodes).FromString(NodeAsString(tradeLineItem, ".//ram:ApplicableTradeTax/ram:CategoryCode", nsmgr)),
                TaxType = default(TaxTypes).FromString(NodeAsString(tradeLineItem, ".//ram:ApplicableTradeTax/ram:TypeCode", nsmgr)),
                TaxPercent = NodeAsDecimal(tradeLineItem, $".//ram:ApplicableTradeTax/ram:{zugferdDialect.ApplicablePercent}", nsmgr, 0).Value,
                NetUnitPrice = NodeAsDecimal(tradeLineItem, ".//ram:NetPriceProductTradePrice/ram:ChargeAmount", nsmgr, 0).Value,
                GrossUnitPrice = NodeAsDecimal(tradeLineItem, ".//ram:GrossPriceProductTradePrice/ram:ChargeAmount", nsmgr, 0).Value,
                UnitCode = default(QuantityCodes).FromString(NodeAsString(tradeLineItem, ".//ram:BasisQuantity/@unitCode", nsmgr))
            };

            if (tradeLineItem.SelectSingleNode(".//ram:AssociatedDocumentLineDocument", nsmgr) != null)
            {
                item.AssociatedDocument = new AssociatedDocument
                {
                    LineID = NodeAsInt(tradeLineItem, ".//ram:AssociatedDocumentLineDocument/ram:LineID", nsmgr, int.MaxValue)
                };

                XmlNodeList noteNodes = tradeLineItem.SelectNodes(".//ram:AssociatedDocumentLineDocument/ram:IncludedNote", nsmgr);
                foreach (XmlNode noteNode in noteNodes)
                {
                    item.AssociatedDocument.Notes.Add(new Note(
                                content: NodeAsString(noteNode, ".//ram:Content", nsmgr),
                                subjectCode: default(SubjectCodes).FromString(NodeAsString(noteNode, ".//ram:SubjectCode", nsmgr)),
                                contentCode: ContentCodes.Unknown
                    ));
                }

                if (item.AssociatedDocument.LineID == int.MaxValue) // a bit dirty, but works for now
                {
                    item.AssociatedDocument.LineID = null;
                }
            }

            XmlNodeList appliedTradeAllowanceChargeNodes = tradeLineItem.SelectNodes($".//ram:{zugferdDialect.SpecifiedLineTradeAgreement}/ram:GrossPriceProductTradePrice/ram:AppliedTradeAllowanceCharge", nsmgr);
            foreach (XmlNode appliedTradeAllowanceChargeNode in appliedTradeAllowanceChargeNodes)
            {
                bool chargeIndicator = NodeAsBool(appliedTradeAllowanceChargeNode, "./ram:ChargeIndicator/udt:Indicator", nsmgr);
                decimal basisAmount = NodeAsDecimal(appliedTradeAllowanceChargeNode, "./ram:BasisAmount", nsmgr, 0).Value;
                string basisAmountCurrency = NodeAsString(appliedTradeAllowanceChargeNode, "./ram:BasisAmount/@currencyID", nsmgr);
                decimal actualAmount = NodeAsDecimal(appliedTradeAllowanceChargeNode, "./ram:ActualAmount", nsmgr, 0).Value;
                string actualAmountCurrency = NodeAsString(appliedTradeAllowanceChargeNode, "./ram:ActualAmount/@currencyID", nsmgr);
                string reason = NodeAsString(appliedTradeAllowanceChargeNode, "./ram:Reason", nsmgr);

                item.addTradeAllowanceCharge(!chargeIndicator, // wichtig: das not (!) beachten
                                                default(CurrencyCodes).FromString(basisAmountCurrency),
                                                basisAmount,
                                                actualAmount,
                                                reason);
            }

            if (item.UnitCode == QuantityCodes.Unknown)
            {
                // UnitCode alternativ aus BilledQuantity extrahieren
                item.UnitCode = default(QuantityCodes).FromString(NodeAsString(tradeLineItem, ".//ram:BilledQuantity/@unitCode", nsmgr));
            }

            if (tradeLineItem.SelectSingleNode($".//ram:{zugferdDialect.SpecifiedLineTradeAgreement}/ram:BuyerOrderReferencedDocument/ram:ID", nsmgr) != null)
            {
                item.BuyerOrderReferencedDocument = new BuyerOrderReferencedDocument
                {
                    ID = NodeAsString(tradeLineItem, $".//ram:{zugferdDialect.SpecifiedLineTradeAgreement}/ram:BuyerOrderReferencedDocument/ram:ID", nsmgr),
                    IssueDateTime = NodeAsDateTime(tradeLineItem, $".//ram:{zugferdDialect.SpecifiedLineTradeAgreement}/ram:BuyerOrderReferencedDocument/ram:IssueDateTime", nsmgr)
                };
            }

            if (tradeLineItem.SelectSingleNode($".//ram:{zugferdDialect.SpecifiedLineTradeDelivery}/ram:DeliveryNoteReferencedDocument/ram:ID", nsmgr) != null)
            {
                item.DeliveryNoteReferencedDocument = new DeliveryNoteReferencedDocument
                {
                    ID = NodeAsString(tradeLineItem, $".//ram:{zugferdDialect.SpecifiedLineTradeDelivery}/ram:DeliveryNoteReferencedDocument/ram:ID", nsmgr),
                    IssueDateTime = NodeAsDateTime(tradeLineItem, $".//ram:{zugferdDialect.SpecifiedLineTradeDelivery}/ram:DeliveryNoteReferencedDocument/ram:IssueDateTime", nsmgr)
                };
            }

            if (tradeLineItem.SelectSingleNode($".//ram:{zugferdDialect.SpecifiedLineTradeDelivery}/ram:ActualDeliverySupplyChainEvent/ram:OccurrenceDateTime", nsmgr) != null)
            {
                item.ActualDeliveryDate = NodeAsDateTime(tradeLineItem, $".//ram:{zugferdDialect.SpecifiedLineTradeDelivery}/ram:ActualDeliverySupplyChainEvent/ram:OccurrenceDateTime/udt:DateTimeString", nsmgr);
            }

            if (tradeLineItem.SelectSingleNode($".//ram:{zugferdDialect.SpecifiedLineTradeAgreement}/ram:ContractReferencedDocument/ram:ID", nsmgr) != null)
            {
                item.ContractReferencedDocument = new ContractReferencedDocument
                {
                    ID = NodeAsString(tradeLineItem, $".//ram:{zugferdDialect.SpecifiedLineTradeAgreement}/ram:ContractReferencedDocument/ram:ID", nsmgr),
                    IssueDateTime = NodeAsDateTime(tradeLineItem, $".//ram:{zugferdDialect.SpecifiedLineTradeAgreement}/ram:ContractReferencedDocument/ram:IssueDateTime", nsmgr)
                };
            }

            XmlNodeList referenceNodes = tradeLineItem.SelectNodes($".//ram:{zugferdDialect.SpecifiedLineTradeAgreement}/ram:AdditionalReferencedDocument", nsmgr);
            foreach (XmlNode referenceNode in referenceNodes)
            {
                string _code = NodeAsString(referenceNode, "ram:ReferenceTypeCode", nsmgr);

                item.addAdditionalReferencedDocument(
                    id: NodeAsString(referenceNode, "ram:ID", nsmgr),
                    date: NodeAsDateTime(referenceNode, "ram:IssueDateTim", nsmgr),
                    code: default(ReferenceTypeCodes).FromString(_code)
                );
            }

            if (tradeLineItem.SelectSingleNode($".//ram:{zugferdDialect.SpecifiedLineTradeAgreement}/ram:ContractReferencedDocument/ram:ID", nsmgr) != null)
            {
                item.ContractReferencedDocument = new ContractReferencedDocument
                {
                    ID = NodeAsString(tradeLineItem, $".//ram:{zugferdDialect.SpecifiedLineTradeAgreement}/ram:ContractReferencedDocument/ram:ID", nsmgr),
                    IssueDateTime = NodeAsDateTime(tradeLineItem, $".//ram:{zugferdDialect.SpecifiedLineTradeAgreement}/ram:ContractReferencedDocument/ram:IssueDateTime", nsmgr)
                };
            }

            return item;
        } // !ParseTradeLineItem()

        private static bool NodeAsBool(XmlNode node, string xpath, XmlNamespaceManager nsmgr = null, bool defaultValue = true)
        {
            if (node == null)
            {
                return defaultValue;
            }

            string value = NodeAsString(node, xpath, nsmgr);
            if (value == "")
            {
                return defaultValue;
            }

            if ((value.Trim().ToLower() == "true") || (value.Trim() == "1"))
            {
                return true;
            }

            return false;
        } // !NodeAsBool()


        private static string NodeAsString(XmlNode node, string xpath, XmlNamespaceManager nsmgr = null, string defaultValue = "")
        {
            if (node == null)
            {
                return defaultValue;
            }

            try
            {
                XmlNode _node = node.SelectSingleNode(xpath, nsmgr);
                if (_node == null)
                {
                    return defaultValue;
                }

                return _node.InnerText;
            }
            catch (XPathException)
            {
                return defaultValue;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        } // NodeAsString()


        private static int NodeAsInt(XmlNode node, string xpath, XmlNamespaceManager nsmgr = null, int defaultValue = 0)
        {
            if (node == null)
            {
                return defaultValue;
            }

            string temp = NodeAsString(node, xpath, nsmgr);
            if (int.TryParse(temp, out int retval))
            {
                return retval;
            }

            return defaultValue;
        } // !_nodeAsInt()


        private static decimal? NodeAsDecimal(XmlNode node, string xpath, XmlNamespaceManager nsmgr = null, decimal? defaultValue = null)
        {
            if (node == null)
            {
                return defaultValue;
            }

            string temp = NodeAsString(node, xpath, nsmgr);
            if (decimal.TryParse(temp, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal retval))
            {
                return retval;
            }

            return defaultValue;
        } // !NodeAsDecimal()


        private static DateTime? NodeAsDateTime(XmlNode node, string xpath, XmlNamespaceManager nsmgr = null, DateTime? defaultValue = null)
        {
            if (node == null)
            {
                return defaultValue;
            }

            string format = "";
            XmlNode dateNode = node.SelectSingleNode(xpath, nsmgr);
            if (dateNode == null)
            {
                if (defaultValue.HasValue)
                {
                    return defaultValue.Value;
                }

                return null;
            }

            if (((XmlElement)dateNode).HasAttribute("format"))
            {
                format = dateNode.Attributes["format"].InnerText;
            }

            string rawValue = dateNode.InnerText;

            switch (format)
            {
                case "102":
                    {
                        if (rawValue.Length != 8)
                        {
                            throw new Exception("Wrong length of datetime element");
                        }

                        string year = rawValue.Substring(0, 4);
                        string month = rawValue.Substring(4, 2);
                        string day = rawValue.Substring(6, 2);

                        return new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                    }
                case "610":
                    {
                        if (rawValue.Length != 6)
                        {
                            throw new Exception("Wrong length of datetime element");
                        }

                        string year = rawValue.Substring(0, 4);
                        string month = rawValue.Substring(4, 2);

                        return new DateTime(int.Parse(year), int.Parse(month), 1);
                    }
                case "616":
                    {
                        if (rawValue.Length != 6)
                        {
                            throw new Exception("Wrong length of datetime element");
                        }

                        string year = rawValue.Substring(0, 4);
                        string week = rawValue.Substring(4, 2);

                        // code from https://capens.net/content/get-first-day-given-week-iso-8601
                        DateTime jan4 = new DateTime(int.Parse(year), 1, 4);
                        DateTime day = jan4.AddDays((int.Parse(week) - 1) * 7); // get a day in the requested week                        
                        int dayOfWeek = ((int)day.DayOfWeek + 6) % 7; // get day of week, with [mon = 0 ... sun = 6] instead of [sun = 0 ... sat = 6]

                        return day.AddDays(-dayOfWeek);
                    }
            }

            // if none of the codes above is present, use fallback approach
            if (rawValue.Length == 8)
            {
                string year = rawValue.Substring(0, 4);
                string month = rawValue.Substring(4, 2);
                string day = rawValue.Substring(6, 2);

                return new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
            }

            if (rawValue.Length == 19)
            {
                string year = rawValue.Substring(0, 4);
                string month = rawValue.Substring(5, 2);
                string day = rawValue.Substring(8, 2);

                string hour = rawValue.Substring(11, 2);
                string minute = rawValue.Substring(14, 2);
                string second = rawValue.Substring(17, 2);

                return new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), int.Parse(hour), int.Parse(minute), int.Parse(second));
            }

            throw new UnsupportedException();
        } // !NodeAsDateTime()


        private static Party NodeAsParty(XmlNode baseNode, string xpath, XmlNamespaceManager nsmgr = null)
        {
            if (baseNode == null)
            {
                return null;
            }

            XmlNode node = baseNode.SelectSingleNode(xpath, nsmgr);
            if (node == null)
            {
                return null;
            }

            Party retval = new Party
            {
                ID = NodeAsString(node, "ram:ID", nsmgr),
                GlobalID = new GlobalID(NodeAsString(node, "ram:GlobalID/@schemeID", nsmgr),
                                        NodeAsString(node, "ram:GlobalID", nsmgr)),
                Name = NodeAsString(node, "ram:Name", nsmgr),
                Postcode = NodeAsString(node, "ram:PostalTradeAddress/ram:PostcodeCode", nsmgr),
                City = NodeAsString(node, "ram:PostalTradeAddress/ram:CityName", nsmgr),
                Country = default(CountryCodes).FromString(NodeAsString(node, "ram:PostalTradeAddress/ram:CountryID", nsmgr))
            };

            string lineOne = NodeAsString(node, "ram:PostalTradeAddress/ram:LineOne", nsmgr);
            string lineTwo = NodeAsString(node, "ram:PostalTradeAddress/ram:LineTwo", nsmgr);

            if (!string.IsNullOrEmpty(lineTwo))
            {
                retval.ContactName = lineOne;
                retval.Street = lineOne;
            }
            else
            {
                retval.Street = lineOne;
                retval.ContactName = null;
            }

            return retval;
        } // !NodeAsParty()

        #endregion

    }
}

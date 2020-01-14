using System;
using System.Collections.Generic;

namespace ZUGFeRD
{
    /// <summary>
    /// Wird benutzt um verschiedene Versionen von ZUGFeRD auslesen zu können
    /// Dabei werden nur die Bezeichnung der Tags und die Namespaces überschrieben
    /// Die restliche Funktionalität des Auslesens bleibt gleich
    /// </summary>
    internal abstract class ZugferdDialect
    {
        public abstract List<Tuple<string, string>> GetNamespaces();
        
        public abstract string DocumentId { get; }
        
        public abstract string IndustryDocumentElement { get; }

        public abstract string ExchangedDocumentContext { get; }

        public abstract string ExchangedDocument { get; }

        public abstract string SupplyChainTradeTransaction { get; }

        public abstract string ApplicableTradeAgreement { get; }

        public abstract string ApplicableTradeDelivery { get; }

        public abstract string ApplicableTradeSettlement { get; }

        public abstract string ApplicablePercent { get; }

        public abstract string SpecifiedTradeSettlementMonetarySummation { get; }

        public abstract string SpecifiedLineTradeAgreement { get; }

        public abstract string SpecifiedLineTradeDelivery { get; }

        public abstract string SpecifiedLineTradeSettlement { get; }

        public abstract string SpecifiedTradeSettlementLineMonetarySummation { get; }
    }

    /// <summary>
    /// ZUGFeRD Version 1.0
    /// https://www.ferd-net.de/zugferd/zugferd-1.0/index.html
    /// </summary>
    internal class Zugferd10Dialect : ZugferdDialect
    {
        public override List<Tuple<string, string>> GetNamespaces() => new List<Tuple<string, string>>
        {
            new Tuple<string, string>( "rsm", "urn:ferd:CrossIndustryDocument:invoice:1p0"),
            new Tuple<string, string>( "ram", "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:12"),
            new Tuple<string, string>( "udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:15")
        };

        public override string DocumentId => "urn:ferd:CrossIndustryDocument:invoice:1p0:";
        public override string IndustryDocumentElement => "CrossIndustryDocument";
        public override string ExchangedDocumentContext => "SpecifiedExchangedDocumentContext";
        public override string ExchangedDocument => "HeaderExchangedDocument";
        public override string SupplyChainTradeTransaction => "SpecifiedSupplyChainTradeTransaction";
        public override string ApplicableTradeAgreement => "ApplicableSupplyChainTradeAgreement";
        public override string ApplicableTradeDelivery => "ApplicableSupplyChainTradeDelivery";
        public override string ApplicableTradeSettlement => "ApplicableSupplyChainTradeSettlement";
        public override string ApplicablePercent => "ApplicablePercent";
        public override string SpecifiedTradeSettlementMonetarySummation => "SpecifiedTradeSettlementMonetarySummation";
        public override string SpecifiedLineTradeAgreement => "SpecifiedSupplyChainTradeAgreement";
        public override string SpecifiedLineTradeDelivery => "SpecifiedSupplyChainTradeDelivery";
        public override string SpecifiedLineTradeSettlement => "SpecifiedSupplyChainTradeSettlement";
        public override string SpecifiedTradeSettlementLineMonetarySummation => "SpecifiedTradeSettlementMonetarySummation";
    }

    /// <summary>
    /// Weiterentwicklung von ZUGFeRD Version 1.0
    /// https://www.ferd-net.de/zugferd/zugferd-2.0.1/index.html
    /// </summary>
    internal class Zugferd20Dialect : ZugferdDialect
    {
        public override List<Tuple<string, string>> GetNamespaces() => new List<Tuple<string, string>>
        {
            new Tuple<string, string>("rsm", "urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100"),
            new Tuple<string, string>( "ram", "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100"),
            new Tuple<string, string>( "udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:100")
        };

        public override string DocumentId => "urn:cen.eu:en16931:2017#compliant#urn:zugferd.de:2p0:";
        public override string IndustryDocumentElement => "CrossIndustryInvoice";
        public override string ExchangedDocumentContext => "ExchangedDocumentContext";
        public override string ExchangedDocument => "ExchangedDocument";
        public override string SupplyChainTradeTransaction => "SupplyChainTradeTransaction";
        public override string ApplicableTradeAgreement => "ApplicableHeaderTradeAgreement";
        public override string ApplicableTradeDelivery => "ApplicableHeaderTradeDelivery";
        public override string ApplicableTradeSettlement => "ApplicableHeaderTradeSettlement";
        public override string ApplicablePercent => "RateApplicablePercent";
        public override string SpecifiedTradeSettlementMonetarySummation => "SpecifiedTradeSettlementHeaderMonetarySummation";
        public override string SpecifiedLineTradeAgreement => "SpecifiedLineTradeAgreement";
        public override string SpecifiedLineTradeDelivery => "SpecifiedLineTradeDelivery";
        public override string SpecifiedLineTradeSettlement => "SpecifiedLineTradeSettlement";
        public override string SpecifiedTradeSettlementLineMonetarySummation => "SpecifiedTradeSettlementLineMonetarySummation";
    }

    /// <summary>
    /// Der Deutsche Standard für "Elektronische Rechnungen an Bund, Länder & Gemeinden"
    /// https://www.verband-e-rechnung.org/xrechnung/
    /// </summary>
    internal class XRechnungDialect : Zugferd20Dialect
    {
        public override string DocumentId => "urn:cen.eu:en16931:2017#compliant#urn:xoev-de:kosit:standard:xrechnung";
    }

    /// <summary>
    /// Der Französische Standard für Rechnung (in zusammenarbeit mit ZUGFeRD entsanden)
    /// http://fnfe-mpe.org/factur-x/
    /// </summary>
    internal class FacturXDialect : Zugferd20Dialect
    {
        public override List<Tuple<string, string>> GetNamespaces() => new List<Tuple<string, string>>
        {
            new Tuple<string, string>("qdt","urn:un:unece:uncefact:data:standard:QualifiedDataType:100"),
            new Tuple<string, string>("rsm", "urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100"),
            new Tuple<string, string>( "ram", "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100"),
            new Tuple<string, string>( "udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:100")
        };

        public override string DocumentId => "urn:cen.eu:en16931:2017#compliant#urn:factur-x.eu:";
    }
}

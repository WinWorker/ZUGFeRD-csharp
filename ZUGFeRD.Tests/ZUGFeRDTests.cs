using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using s2industries.ZUGFeRD;

namespace ZUGFeRD.Tests
{
    [TestClass]
    public class ZUGFeRDTests
    {

        [TestMethod]
        public void TestZugFeRDV1()
        {
            string path = Path.GetTempPath() + Guid.NewGuid() + ".xml";
            File.WriteAllText(path, Properties.Resources.zugferd10);
            InvoiceDescriptor descriptor = InvoiceDescriptor.Load(path);
            Assert.IsNotNull(descriptor);
            
            Assert.AreEqual(new DateTime(2018, 03, 05), descriptor.ActualDeliveryDate);
            Assert.AreEqual(CurrencyCodes.EUR, descriptor.Currency);
            Assert.AreEqual("471102", descriptor.InvoiceNo);
            Assert.AreEqual(new DateTime(2018,03,05), descriptor.InvoiceDate);
            Assert.AreEqual(new decimal(235.62), descriptor.GrandTotalAmount);
            Assert.AreEqual(new decimal(198.00), descriptor.TaxBasisAmount);

            Assert.AreEqual(new decimal(19), descriptor.Taxes[0].Percent);
            Assert.AreEqual(new decimal(198), descriptor.Taxes[0].BasisAmount);
            Assert.AreEqual(new decimal(37.62), descriptor.Taxes[0].TaxAmount);

            Assert.AreEqual("201/113/40209", descriptor.SellerTaxRegistration[0].No);
            Assert.AreEqual(TaxRegistrationSchemeID.FC, descriptor.SellerTaxRegistration[0].SchemeID);

            Assert.AreEqual("DE123456789", descriptor.SellerTaxRegistration[1].No);
            Assert.AreEqual(TaxRegistrationSchemeID.VA, descriptor.SellerTaxRegistration[1].SchemeID);

            Assert.AreEqual(QuantityCodes.C62, descriptor.TradeLineItems[0].UnitCode);
            Assert.AreEqual(new decimal(20), descriptor.TradeLineItems[0].BilledQuantity);
            Assert.AreEqual(new decimal(1), descriptor.TradeLineItems[0].UnitQuantity);
            Assert.AreEqual("4012345001235", descriptor.TradeLineItems[0].GlobalID.ID);
            Assert.AreEqual("0160", descriptor.TradeLineItems[0].GlobalID.SchemeID);

            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                //Ignore
            }
        }

        [TestMethod]
        public void TestZugFeRdV2()
        {
            string path = Path.GetTempPath() + Guid.NewGuid() + ".xml";
            File.WriteAllText(path, Properties.Resources.zugferd20);
            InvoiceDescriptor descriptor = InvoiceDescriptor.Load(path);
            Assert.IsNotNull(descriptor);

            Assert.AreEqual(new DateTime(2020,01, 06), descriptor.ActualDeliveryDate);
            Assert.AreEqual(CurrencyCodes.EUR, descriptor.Currency);
            Assert.AreEqual("2001101", descriptor.InvoiceNo);
            Assert.AreEqual(new DateTime(2020, 01, 06), descriptor.InvoiceDate);
            Assert.AreEqual(new decimal(99.67), descriptor.GrandTotalAmount);
            Assert.AreEqual(Profile.Basic, descriptor.Profile);
            Assert.AreEqual(new decimal(83.76), descriptor.TaxBasisAmount);
            Assert.AreEqual(InvoiceType.Invoice, descriptor.Type);

            Assert.AreEqual(new decimal(19), descriptor.Taxes[0].Percent);
            Assert.AreEqual(new decimal(83.76), descriptor.Taxes[0].BasisAmount);
            Assert.AreEqual(new decimal(15.9144), descriptor.Taxes[0].TaxAmount);

            Assert.AreEqual("234567", descriptor.SellerTaxRegistration[0].No);
            Assert.AreEqual(TaxRegistrationSchemeID.FC, descriptor.SellerTaxRegistration[0].SchemeID);

            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                //Ignore
            }
        }

        [TestMethod]
        public void TestXRechnung()
        {
            string path = Path.GetTempPath() + Guid.NewGuid() + ".xml";
            File.WriteAllText(path, Properties.Resources.XRechnung);
            InvoiceDescriptor descriptor = InvoiceDescriptor.Load(path);
            Assert.IsNotNull(descriptor);

            Assert.AreEqual(CurrencyCodes.EUR, descriptor.Currency);
            Assert.AreEqual("123456XX", descriptor.InvoiceNo);
            Assert.AreEqual(new DateTime(2016, 04, 04), descriptor.InvoiceDate);
            Assert.AreEqual(new decimal(336.90), descriptor.GrandTotalAmount);
            Assert.AreEqual(Profile.Unknown, descriptor.Profile);
            Assert.AreEqual(new decimal(314.86), descriptor.TaxBasisAmount);
            Assert.AreEqual(InvoiceType.Invoice, descriptor.Type);
            Assert.AreEqual("04011000-12345-34",descriptor.ReferenceOrderNo);

            Assert.AreEqual(new decimal(7), descriptor.Taxes[0].Percent);
            Assert.AreEqual(new decimal(314.86), descriptor.Taxes[0].BasisAmount);
            Assert.AreEqual(new decimal(22.0402), descriptor.Taxes[0].TaxAmount);

            Assert.AreEqual("DE 123456789", descriptor.SellerTaxRegistration[0].No);
            Assert.AreEqual(TaxRegistrationSchemeID.VA, descriptor.SellerTaxRegistration[0].SchemeID);

            Assert.AreEqual(QuantityCodes.Unknown, descriptor.TradeLineItems[0].UnitCode);
            Assert.AreEqual(new decimal(1), descriptor.TradeLineItems[0].BilledQuantity);
            Assert.AreEqual(new decimal(1), descriptor.TradeLineItems[0].UnitQuantity);

            Assert.AreEqual(QuantityCodes.Unknown, descriptor.TradeLineItems[1].UnitCode);
            Assert.AreEqual(new decimal(1), descriptor.TradeLineItems[1].BilledQuantity);
            Assert.AreEqual(new decimal(1), descriptor.TradeLineItems[1].UnitQuantity);

            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                //Ignore
            }
        }

        [TestMethod]
        public void TestFacturX()
        {
            string path = Path.GetTempPath() + Guid.NewGuid() + ".xml";
            File.WriteAllText(path, Properties.Resources.factur_x);
            InvoiceDescriptor descriptor = InvoiceDescriptor.Load(path);
            Assert.IsNotNull(descriptor);

            Assert.AreEqual(CurrencyCodes.EUR, descriptor.Currency);
            Assert.AreEqual("AV-2017-0005", descriptor.InvoiceNo);
            Assert.AreEqual(new DateTime(2017, 11, 16), descriptor.InvoiceDate);
            Assert.AreEqual(new decimal(-233.47), descriptor.GrandTotalAmount);
            Assert.AreEqual(Profile.Unknown, descriptor.Profile);
            Assert.AreEqual(new decimal(-218.48), descriptor.TaxBasisAmount);
            Assert.AreEqual(InvoiceType.Invoice, descriptor.Type);

            Assert.AreEqual(new decimal(20), descriptor.Taxes[0].Percent);
            Assert.AreEqual(new decimal(5.5), descriptor.Taxes[1].Percent);

            Assert.AreEqual(new decimal(-20.48), descriptor.Taxes[0].BasisAmount);
            Assert.AreEqual(new decimal(-198.00), descriptor.Taxes[1].BasisAmount);

            Assert.AreEqual(new decimal(-4.096), descriptor.Taxes[0].TaxAmount);
            Assert.AreEqual(new decimal(-10.89), descriptor.Taxes[1].TaxAmount);

            Assert.AreEqual("FR19787878784", descriptor.BuyerTaxRegistration[0].No);
            Assert.AreEqual(TaxRegistrationSchemeID.VA, descriptor.BuyerTaxRegistration[0].SchemeID);

            Assert.AreEqual("FR11999999998", descriptor.SellerTaxRegistration[0].No);
            Assert.AreEqual(TaxRegistrationSchemeID.VA, descriptor.SellerTaxRegistration[0].SchemeID);

            Assert.AreEqual(QuantityCodes.C62, descriptor.TradeLineItems[0].UnitCode);
            Assert.AreEqual(new decimal(-5), descriptor.TradeLineItems[0].BilledQuantity);
            Assert.AreEqual(new decimal(1), descriptor.TradeLineItems[0].UnitQuantity);

            Assert.AreEqual(QuantityCodes.LTR, descriptor.TradeLineItems[1].UnitCode);
            Assert.AreEqual(new decimal(-10), descriptor.TradeLineItems[1].BilledQuantity);
            Assert.AreEqual(new decimal(1), descriptor.TradeLineItems[1].UnitQuantity);

            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                //Ignore
            }
        }
    }
}

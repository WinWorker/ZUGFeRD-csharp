using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using s2industries.ZUGFeRD;

using SP.WinWorker.PickupDocuments.PdfDocuments;

namespace ZUGFeRD.Tests
{
    [TestClass]
    public class ZUGFeRDTests
    {

        [TestMethod]
        public void TestZugFeRdv1()
        {
            string path = Path.GetTempPath() + Guid.NewGuid() + ".xml";
            File.WriteAllText(path, Properties.Resources.zugferd10);
            InvoiceDescriptor descriptor = InvoiceDescriptor.Load(path);
            Assert.AreNotEqual(null, descriptor);

            Assert.AreEqual(DateTime.Parse("05-03-2018"), descriptor.ActualDeliveryDate);
            Assert.AreEqual(CurrencyCodes.EUR, descriptor.Currency);
            Assert.AreEqual("471102", descriptor.InvoiceNo);
            Assert.AreEqual(DateTime.Parse("05-03-2018"), descriptor.InvoiceDate);
            Assert.AreEqual(decimal.Parse("235,62"), descriptor.GrandTotalAmount);
            Assert.AreEqual(decimal.Parse("198,00"), descriptor.TaxBasisAmount);

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
        public void CompareVersion10()
        {
            string path = Path.GetTempPath() + Guid.NewGuid() + ".xml";
            File.WriteAllText(path, Properties.Resources.zugferd10);
            InvoiceDescriptor descriptor = InvoiceDescriptor.Load(path);

            ZugferdDocument document = new ZugferdDocument();
            document.LoadFromRawZugferdFile(path);

            Assert.AreEqual(document.Currency, descriptor.Currency.ToString());
            Assert.AreEqual(document.BruttoBetrag, SP.Convert.ConvertToDouble(descriptor.GrandTotalAmount));
            Assert.AreEqual(document.NettoBetrag, SP.Convert.ConvertToDouble(descriptor.TaxBasisAmount));
            Assert.AreEqual(document.DocDate, descriptor.InvoiceDate);
            Assert.AreEqual(document.DocId, descriptor.InvoiceNo);
            Assert.AreEqual(document.Zahlungsziel, descriptor.PaymentTerms.DueDate);
            Assert.AreEqual(document.Positionen.Count, descriptor.TradeLineItems.Count);

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
        public void TestZugFeRdv2()
        {
            string path = Path.GetTempPath() + Guid.NewGuid() + ".xml";
            File.WriteAllText(path, Properties.Resources.zugferd20);
            InvoiceDescriptor descriptor = InvoiceDescriptor.Load(path);
            Assert.AreNotEqual(null, descriptor);

            Assert.AreEqual(DateTime.Parse("06-01-2020"), descriptor.ActualDeliveryDate);
            Assert.AreEqual(CurrencyCodes.EUR, descriptor.Currency);
            Assert.AreEqual("2001101", descriptor.InvoiceNo);
            Assert.AreEqual(DateTime.Parse("06-01-2020"), descriptor.InvoiceDate);
            Assert.AreEqual(decimal.Parse("99,67"), descriptor.GrandTotalAmount);
            Assert.AreEqual(Profile.Basic, descriptor.Profile);
            Assert.AreEqual(decimal.Parse("83,76"), descriptor.TaxBasisAmount);
            Assert.AreEqual(InvoiceType.Invoice, descriptor.Type);

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
        public void CompareVersion20()
        {
            string path = Path.GetTempPath() + Guid.NewGuid() + ".xml";
            File.WriteAllText(path, Properties.Resources.zugferd20);
            InvoiceDescriptor descriptor = InvoiceDescriptor.Load(path);

            ZugferdDocument document = new ZugferdDocument();
            document.LoadFromRawZugferdFile(path);

            Assert.AreEqual(document.Currency, descriptor.Currency.ToString());
            Assert.AreEqual(document.BruttoBetrag, SP.Convert.ConvertToDouble(descriptor.GrandTotalAmount));
            Assert.AreEqual(document.NettoBetrag, SP.Convert.ConvertToDouble(descriptor.TaxBasisAmount));
            Assert.AreEqual(document.DocDate, descriptor.InvoiceDate);
            Assert.AreEqual(document.DocId, descriptor.InvoiceNo);
            Assert.AreEqual(document.Zahlungsziel, descriptor.PaymentTerms?.DueDate ?? DateTime.MinValue);
            Assert.AreEqual(document.Positionen.Count, descriptor.TradeLineItems.Count);

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

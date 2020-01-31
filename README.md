# ZUGFeRD
Now part of the [ZUGFeRD community](https://github.com/zugferd)!  

The ZUGFeRD library allows to create XML files as required by German electronic invoice initiative ZUGFeRD.
## Ease of use
The library is meant to be as simple as possible, however it is not straight forward to use as the resulting XML file contains a complete invoice in XML format. Please take a look at the ZUGFeRD-Test project to find sample creation code. This code creates the same XML file as shipped with the ZUGFeRD information package.  
The code used here is:  
```
InvoiceDescriptor desc = InvoiceDescriptor.CreateInvoice("471102", new DateTime(2013, 6, 5), CurrencyCodes.EUR, "GE2020211-471102");
desc.Profile = Profile.Comfort;
desc.ReferenceOrderNo = "AB-312";
desc.AddNote("Rechnung gemäß Bestellung Nr. 2013-471331 vom 01.03.2013.");
desc.AddNote("Es bestehen Rabatt- und Bonusvereinbarungen.", "AAK");
desc.SetBuyer("Kunden Mitte AG", "69876", "Frankfurt", "Kundenstraße", "15", "DE", "88", "4000001987658");
desc.AddBuyerTaxRegistration("DE234567890", "VA");
desc.SetBuyerContact("Hans Muster");
desc.SetSeller("Lieferant GmbH", "80333", "München", "Lieferantenstraße", "20", "DE", "88", "4000001123452");
desc.AddSellerTaxRegistration("201/113/40209", "FC");
desc.AddSellerTaxRegistration("DE123456789", "VA");
desc.SetBuyerOrderReferenceDocument("2013-471331", new DateTime(2013, 03, 01));
desc.SetDeliveryNoteReferenceDocument("2013-51111", new DateTime(2013, 6, 3));
desc.ActualDeliveryDate = new DateTime(2013, 6, 3);
desc.SetTotals(202.76m, 5.80m, 14.73m, 193.83m, 21.31m, 215.14m, 50.0m, 165.14m);
desc.AddApplicableTradeTax(9.06m, 129.37m, 7m, "VAT", "S");
desc.AddApplicableTradeTax(12.25m, 64.46m, 19m, "VAT", "S");
desc.SetLogisticsServiceCharge(5.80m, "Versandkosten", "VAT", "S", 7m);
desc.setTradePaymentTerms("Zahlbar innerhalb 30 Tagen netto bis 04.07.2013, 3% Skonto innerhalb 10 Tagen bis 15.06.2013", new DateTime(2013, 07, 04));

desc.Save("output.xml");
```
## Compatibility
||ZUGFeRD V 1.0|ZUGFeRD V 2.0|X-Rechnung|Factur-X|
|---|---|---|---|---|
|Documentation|[Documentation](https://www.ferd-net.de/zugferd/zugferd-1.0/index.html)|[Documentation](https://www.ferd-net.de/zugferd/zugferd-2.0.1/index.html)|[Documentation](https://www.verband-e-rechnung.org/xrechnung/)|[Documentation](https://www.ferd-net.de/upload/Dokumente/FACTUR-X_ZUGFeRD_2p0_Teil1_Profil_EN16931_1p03.pdf)|
|Unit-Tests (reading)|✔|✔|✔|✔|
|Reading|✔|✔|✔|✔|
|Writing|✔|✔|❌|❌|
## More information
* A description of the library can be found [here](http://www.s2-industries.com/wordpress/2013/11/creating-zugferd-descriptors-with-c/)
* The solution is used in CKS.DMS and supported by [CKSolution](http://www.cksolution.de)
* You can find more information about ZUGFeRD at [FeRD](http://www.ferd-net.de/)
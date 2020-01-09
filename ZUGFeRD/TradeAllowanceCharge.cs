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

namespace s2industries.ZUGFeRD
{
    /// <summary>
    /// Zu- und Abschlag
    /// 
    /// Beispiel:
    /// <SpecifiedTradeAllowanceCharge>
	///   <ChargeIndicator>false</ChargeIndicator>
    ///      <BasisAmount currencyID="EUR">137.30</BasisAmount>
    ///      <ActualAmount>13.73</ActualAmount>
    ///      <Reason>Sondernachlass</Reason>
    ///      <CategoryTradeTax>
    ///        <TypeCode>VAT</TypeCode>
    ///        <CategoryCode>S</CategoryCode>
    ///        <ApplicablePercent>7</ApplicablePercent>
    ///      </CategoryTradeTax>
    ///    </SpecifiedTradeAllowanceCharge>
    /// </summary>
    public class TradeAllowanceCharge : Charge
    {
        /// <summary>
        /// Schalter für Zu- und Abschlag
        /// 
        /// false: Abschlag
        /// true: Zuschlag
        /// </summary>
        public bool ChargeIndicator { get; set; }
        public string Reason { get; set; }
        public decimal BasisAmount { get; set; }
        public CurrencyCodes Currency { get; set; }
        public decimal ActualAmount { get; set; }
    }
}

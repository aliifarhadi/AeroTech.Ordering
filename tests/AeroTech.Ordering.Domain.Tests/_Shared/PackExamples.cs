namespace AeroTech.Ordering.Domain.Tests._Shared
{
    public static class PackExamples
    {
        public const string NormalizedCandidate = """
{
  "schemaVersion": "3.0",
  "source": {
    "owner": "AirOffer",
    "offerId": "REFERENCE-PRICED-OW-001",
    "providerProfileId": "REFERENCE-OFFER-2.0",
    "ownerBindingRef": "REFERENCE-BINDING-001",
    "sourcePayloadHash": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
  },
  "acceptanceAssurance": "OwnerBound",
  "pricedAt": "2026-10-01T10:00:00Z",
  "capturedAt": "2026-10-01T10:00:00Z",
  "validity": {
    "offer": {
      "state": "Known",
      "value": "2026-10-01T10:10:00Z",
      "owner": "AirOffer",
      "sourceRef": "REFERENCE-OFFER-001",
      "reason": null
    },
    "price": {
      "state": "Known",
      "value": "2026-10-01T10:05:00Z",
      "owner": "AirPrice",
      "sourceRef": "REFERENCE-PRICE-001",
      "reason": null
    },
    "ticketing": {
      "state": "NotSupplied",
      "value": null,
      "owner": "Unresolved owner",
      "sourceRef": null,
      "reason": "Not supplied by source; no implied infinite validity"
    },
    "observedTicketingDeadline": null
  },
  "salesContext": {
    "ownerAirlineId": "1",
    "financialCustomerId": "100",
    "channel": "Backoffice",
    "sellingOfficeId": "10"
  },
  "travelers": [
    {
      "sourceTravellerRef": "PAX-A",
      "passengerTypeCode": "ADT"
    }
  ],
  "journeys": [
    {
      "journeyRef": "BOUND-1",
      "sequence": 1,
      "sourceDirectionRaw": "Outbound",
      "direction": null,
      "originRef": "AIRPORT-A",
      "destinationRef": "AIRPORT-B"
    }
  ],
  "segments": [
    {
      "segmentRef": "SEG-A",
      "journeyRef": "BOUND-1",
      "kind": "ScheduledAir",
      "originRef": "AIRPORT-A",
      "originTerminalRef": "TERMINAL-1",
      "destinationRef": "AIRPORT-B",
      "destinationTerminalRef": "TERMINAL-2",
      "soldDeparture": "2026-10-15T09:00:00Z",
      "soldArrival": "2026-10-15T11:00:00Z",
      "flightRef": "REFERENCE-FLIGHT-A",
      "flightNumber": "RF100",
      "flightVersion": "1",
      "marketingCarrierRef": "CARRIER-M",
      "operatingCarrierRef": "CARRIER-O",
      "sourceCapacityRef": "CAPACITY-A",
      "duration": 120,
      "aircraftRef": "AIRCRAFT-1",
      "legs": [
        {
          "sourceLegRef": "LEG-A",
          "sequence": 1,
          "originRef": "AIRPORT-A",
          "originTerminalRef": "TERMINAL-1",
          "destinationRef": "AIRPORT-B",
          "destinationTerminalRef": "TERMINAL-2",
          "departure": "2026-10-15T09:00:00Z",
          "arrival": "2026-10-15T11:00:00Z"
        }
      ]
    }
  ],
  "items": [
    {
      "itemRef": "ITEM-A",
      "itemKind": "OfferPackage",
      "sourceOfferItemRef": null,
      "serviceRefs": [
        "SERVICE-A"
      ],
      "acceptedTotal": {
        "amount": "120.00",
        "currencyRef": "EUR"
      },
      "product": {
        "sourceSystem": "AirOffer",
        "sourceOfferId": "REFERENCE-PRICED-OW-001",
        "sourceOfferItemRef": null,
        "productCode": null,
        "productName": null,
        "brandCode": null,
        "brandName": null,
        "productVersion": null
      }
    }
  ],
  "services": [
    {
      "serviceRef": "SERVICE-A",
      "type": "AirTransport",
      "serviceCode": null,
      "name": null,
      "priceTreatment": "SupplierOpaque",
      "supplierPartyRef": null,
      "deliveryProviderRef": null,
      "beneficiaryRefs": [
        "PAX-A"
      ],
      "segmentRefs": [
        "SEG-A"
      ],
      "quantity": "1",
      "quantityUnit": "PassengerSegment",
      "detailSchema": "AirTransport",
      "detailSchemaVersion": 2,
      "details": {
        "cabinRef": "ECONOMY",
        "bookingClass": "Y"
      },
      "checkedBaggage": {
        "pieces": 1,
        "weight": "23",
        "weightUnit": "Kg"
      },
      "cabinBaggage": {
        "pieces": 1,
        "weight": null,
        "weightUnit": null
      },
      "soldTerms": {
        "refundable": true,
        "changeable": true,
        "upgradable": false
      },
      "fulfillmentProfile": {
        "profileRef": "REFERENCE-AIR-ETKT",
        "profileVersion": "1",
        "assurance": "Certified",
        "reservationRequirement": "FlightCapacity",
        "documentKind": "ETKT",
        "fundingRequirement": "Required",
        "capacityUnits": 1
      }
    }
  ],
  "pricingLines": [
    {
      "lineRef": "PRICE-FARE",
      "itemRef": "ITEM-A",
      "component": "Fare",
      "effect": "CustomerBalance",
      "direction": "Debit",
      "lineRole": "Original",
      "sourceCode": null,
      "sourceName": null,
      "sourceReference": null,
      "calculationKind": "Amount",
      "originalValue": {
        "amount": "100.00",
        "currencyRef": "EUR"
      },
      "saleValue": {
        "amount": "100.00",
        "currencyRef": "EUR"
      },
      "sourceLineRef": "tickets/0/coupons/0/pricings/0",
      "basisType": "OrderService",
      "basisRef": "SERVICE-A",
      "sourceConversionRef": null,
      "appliedConversion": null
    },
    {
      "lineRef": "PRICE-TAX",
      "itemRef": "ITEM-A",
      "component": "Tax",
      "effect": "CustomerBalance",
      "direction": "Debit",
      "lineRole": "Original",
      "sourceCode": "YQ",
      "sourceName": "Carrier imposed charge",
      "sourceReference": null,
      "calculationKind": "Amount",
      "originalValue": {
        "amount": "20.00",
        "currencyRef": "EUR"
      },
      "saleValue": {
        "amount": "20.00",
        "currencyRef": "EUR"
      },
      "sourceLineRef": "tickets/0/coupons/0/pricings/1",
      "basisType": "OrderService",
      "basisRef": "SERVICE-A",
      "sourceConversionRef": null,
      "appliedConversion": null
    }
  ],
  "customerTotal": {
    "amount": "120.00",
    "currencyRef": "EUR"
  },
  "saleCurrencyCode": "EUR",
  "sourceJourneyTypeRaw": "OneWay",
  "journeyType": null,
  "fareConstruction": {
    "assurance": "Opaque",
    "sourceContextRef": "REFERENCE-CONTEXT-001",
    "pricingUnits": []
  }
}
""";

        public const string IncorrectTotal = """
{
  "schemaVersion": "3.0",
  "source": {
    "owner": "AirOffer",
    "offerId": "REFERENCE-PRICED-OW-001",
    "providerProfileId": "REFERENCE-OFFER-2.0",
    "ownerBindingRef": "REFERENCE-BINDING-001",
    "sourcePayloadHash": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
  },
  "acceptanceAssurance": "OwnerBound",
  "pricedAt": "2026-10-01T10:00:00Z",
  "capturedAt": "2026-10-01T10:00:00Z",
  "validity": {
    "offer": {
      "state": "Known",
      "value": "2026-10-01T10:10:00Z",
      "owner": "AirOffer",
      "sourceRef": "REFERENCE-OFFER-001",
      "reason": null
    },
    "price": {
      "state": "Known",
      "value": "2026-10-01T10:05:00Z",
      "owner": "AirPrice",
      "sourceRef": "REFERENCE-PRICE-001",
      "reason": null
    },
    "ticketing": {
      "state": "NotSupplied",
      "value": null,
      "owner": "Unresolved owner",
      "sourceRef": null,
      "reason": "Not supplied by source; no implied infinite validity"
    },
    "observedTicketingDeadline": null
  },
  "salesContext": {
    "ownerAirlineId": "1",
    "financialCustomerId": "100",
    "channel": "Backoffice",
    "sellingOfficeId": "10"
  },
  "travelers": [
    {
      "sourceTravellerRef": "PAX-A",
      "passengerTypeCode": "ADT"
    }
  ],
  "journeys": [
    {
      "journeyRef": "BOUND-1",
      "sequence": 1,
      "sourceDirectionRaw": "Outbound",
      "direction": null,
      "originRef": "AIRPORT-A",
      "destinationRef": "AIRPORT-B"
    }
  ],
  "segments": [
    {
      "segmentRef": "SEG-A",
      "journeyRef": "BOUND-1",
      "kind": "ScheduledAir",
      "originRef": "AIRPORT-A",
      "originTerminalRef": "TERMINAL-1",
      "destinationRef": "AIRPORT-B",
      "destinationTerminalRef": "TERMINAL-2",
      "soldDeparture": "2026-10-15T09:00:00Z",
      "soldArrival": "2026-10-15T11:00:00Z",
      "flightRef": "REFERENCE-FLIGHT-A",
      "flightNumber": "RF100",
      "flightVersion": "1",
      "marketingCarrierRef": "CARRIER-M",
      "operatingCarrierRef": "CARRIER-O",
      "sourceCapacityRef": "CAPACITY-A",
      "duration": 120,
      "aircraftRef": "AIRCRAFT-1",
      "legs": [
        {
          "sourceLegRef": "LEG-A",
          "sequence": 1,
          "originRef": "AIRPORT-A",
          "originTerminalRef": "TERMINAL-1",
          "destinationRef": "AIRPORT-B",
          "destinationTerminalRef": "TERMINAL-2",
          "departure": "2026-10-15T09:00:00Z",
          "arrival": "2026-10-15T11:00:00Z"
        }
      ]
    }
  ],
  "items": [
    {
      "itemRef": "ITEM-A",
      "itemKind": "OfferPackage",
      "sourceOfferItemRef": null,
      "serviceRefs": [
        "SERVICE-A"
      ],
      "acceptedTotal": {
        "amount": "120.00",
        "currencyRef": "EUR"
      },
      "product": {
        "sourceSystem": "AirOffer",
        "sourceOfferId": "REFERENCE-PRICED-OW-001",
        "sourceOfferItemRef": null,
        "productCode": null,
        "productName": null,
        "brandCode": null,
        "brandName": null,
        "productVersion": null
      }
    }
  ],
  "services": [
    {
      "serviceRef": "SERVICE-A",
      "type": "AirTransport",
      "serviceCode": null,
      "name": null,
      "priceTreatment": "SupplierOpaque",
      "supplierPartyRef": null,
      "deliveryProviderRef": null,
      "beneficiaryRefs": [
        "PAX-A"
      ],
      "segmentRefs": [
        "SEG-A"
      ],
      "quantity": "1",
      "quantityUnit": "PassengerSegment",
      "detailSchema": "AirTransport",
      "detailSchemaVersion": 2,
      "details": {
        "cabinRef": "ECONOMY",
        "bookingClass": "Y"
      },
      "checkedBaggage": {
        "pieces": 1,
        "weight": "23",
        "weightUnit": "Kg"
      },
      "cabinBaggage": {
        "pieces": 1,
        "weight": null,
        "weightUnit": null
      },
      "soldTerms": {
        "refundable": true,
        "changeable": true,
        "upgradable": false
      },
      "fulfillmentProfile": {
        "profileRef": "REFERENCE-AIR-ETKT",
        "profileVersion": "1",
        "assurance": "Certified",
        "reservationRequirement": "FlightCapacity",
        "documentKind": "ETKT",
        "fundingRequirement": "Required",
        "capacityUnits": 1
      }
    }
  ],
  "pricingLines": [
    {
      "lineRef": "PRICE-FARE",
      "itemRef": "ITEM-A",
      "component": "Fare",
      "effect": "CustomerBalance",
      "direction": "Debit",
      "lineRole": "Original",
      "sourceCode": null,
      "sourceName": null,
      "sourceReference": null,
      "calculationKind": "Amount",
      "originalValue": {
        "amount": "100.00",
        "currencyRef": "EUR"
      },
      "saleValue": {
        "amount": "100.00",
        "currencyRef": "EUR"
      },
      "sourceLineRef": "tickets/0/coupons/0/pricings/0",
      "basisType": "OrderService",
      "basisRef": "SERVICE-A",
      "sourceConversionRef": null,
      "appliedConversion": null
    },
    {
      "lineRef": "PRICE-TAX",
      "itemRef": "ITEM-A",
      "component": "Tax",
      "effect": "CustomerBalance",
      "direction": "Debit",
      "lineRole": "Original",
      "sourceCode": "YQ",
      "sourceName": "Carrier imposed charge",
      "sourceReference": null,
      "calculationKind": "Amount",
      "originalValue": {
        "amount": "20.00",
        "currencyRef": "EUR"
      },
      "saleValue": {
        "amount": "20.00",
        "currencyRef": "EUR"
      },
      "sourceLineRef": "tickets/0/coupons/0/pricings/1",
      "basisType": "OrderService",
      "basisRef": "SERVICE-A",
      "sourceConversionRef": null,
      "appliedConversion": null
    }
  ],
  "customerTotal": {
    "amount": "121.00",
    "currencyRef": "EUR"
  },
  "saleCurrencyCode": "EUR",
  "sourceJourneyTypeRaw": "OneWay",
  "journeyType": null,
  "fareConstruction": {
    "assurance": "Opaque",
    "sourceContextRef": "REFERENCE-CONTEXT-001",
    "pricingUnits": []
  }
}
""";

        public const string MissingBeneficiary = """
{
  "schemaVersion": "3.0",
  "source": {
    "owner": "AirOffer",
    "offerId": "REFERENCE-PRICED-OW-001",
    "providerProfileId": "REFERENCE-OFFER-2.0",
    "ownerBindingRef": "REFERENCE-BINDING-001",
    "sourcePayloadHash": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
  },
  "acceptanceAssurance": "OwnerBound",
  "pricedAt": "2026-10-01T10:00:00Z",
  "capturedAt": "2026-10-01T10:00:00Z",
  "validity": {
    "offer": {
      "state": "Known",
      "value": "2026-10-01T10:10:00Z",
      "owner": "AirOffer",
      "sourceRef": "REFERENCE-OFFER-001",
      "reason": null
    },
    "price": {
      "state": "Known",
      "value": "2026-10-01T10:05:00Z",
      "owner": "AirPrice",
      "sourceRef": "REFERENCE-PRICE-001",
      "reason": null
    },
    "ticketing": {
      "state": "NotSupplied",
      "value": null,
      "owner": "Unresolved owner",
      "sourceRef": null,
      "reason": "Not supplied by source; no implied infinite validity"
    },
    "observedTicketingDeadline": null
  },
  "salesContext": {
    "ownerAirlineId": "1",
    "financialCustomerId": "100",
    "channel": "Backoffice",
    "sellingOfficeId": "10"
  },
  "travelers": [
    {
      "sourceTravellerRef": "PAX-A",
      "passengerTypeCode": "ADT"
    }
  ],
  "journeys": [
    {
      "journeyRef": "BOUND-1",
      "sequence": 1,
      "sourceDirectionRaw": "Outbound",
      "direction": null,
      "originRef": "AIRPORT-A",
      "destinationRef": "AIRPORT-B"
    }
  ],
  "segments": [
    {
      "segmentRef": "SEG-A",
      "journeyRef": "BOUND-1",
      "kind": "ScheduledAir",
      "originRef": "AIRPORT-A",
      "originTerminalRef": "TERMINAL-1",
      "destinationRef": "AIRPORT-B",
      "destinationTerminalRef": "TERMINAL-2",
      "soldDeparture": "2026-10-15T09:00:00Z",
      "soldArrival": "2026-10-15T11:00:00Z",
      "flightRef": "REFERENCE-FLIGHT-A",
      "flightNumber": "RF100",
      "flightVersion": "1",
      "marketingCarrierRef": "CARRIER-M",
      "operatingCarrierRef": "CARRIER-O",
      "sourceCapacityRef": "CAPACITY-A",
      "duration": 120,
      "aircraftRef": "AIRCRAFT-1",
      "legs": [
        {
          "sourceLegRef": "LEG-A",
          "sequence": 1,
          "originRef": "AIRPORT-A",
          "originTerminalRef": "TERMINAL-1",
          "destinationRef": "AIRPORT-B",
          "destinationTerminalRef": "TERMINAL-2",
          "departure": "2026-10-15T09:00:00Z",
          "arrival": "2026-10-15T11:00:00Z"
        }
      ]
    }
  ],
  "items": [
    {
      "itemRef": "ITEM-A",
      "itemKind": "OfferPackage",
      "sourceOfferItemRef": null,
      "serviceRefs": [
        "SERVICE-A"
      ],
      "acceptedTotal": {
        "amount": "120.00",
        "currencyRef": "EUR"
      },
      "product": {
        "sourceSystem": "AirOffer",
        "sourceOfferId": "REFERENCE-PRICED-OW-001",
        "sourceOfferItemRef": null,
        "productCode": null,
        "productName": null,
        "brandCode": null,
        "brandName": null,
        "productVersion": null
      }
    }
  ],
  "services": [
    {
      "serviceRef": "SERVICE-A",
      "type": "AirTransport",
      "serviceCode": null,
      "name": null,
      "priceTreatment": "SupplierOpaque",
      "supplierPartyRef": null,
      "deliveryProviderRef": null,
      "beneficiaryRefs": [
        "MISSING"
      ],
      "segmentRefs": [
        "SEG-A"
      ],
      "quantity": "1",
      "quantityUnit": "PassengerSegment",
      "detailSchema": "AirTransport",
      "detailSchemaVersion": 2,
      "details": {
        "cabinRef": "ECONOMY",
        "bookingClass": "Y"
      },
      "checkedBaggage": {
        "pieces": 1,
        "weight": "23",
        "weightUnit": "Kg"
      },
      "cabinBaggage": {
        "pieces": 1,
        "weight": null,
        "weightUnit": null
      },
      "soldTerms": {
        "refundable": true,
        "changeable": true,
        "upgradable": false
      },
      "fulfillmentProfile": {
        "profileRef": "REFERENCE-AIR-ETKT",
        "profileVersion": "1",
        "assurance": "Certified",
        "reservationRequirement": "FlightCapacity",
        "documentKind": "ETKT",
        "fundingRequirement": "Required",
        "capacityUnits": 1
      }
    }
  ],
  "pricingLines": [
    {
      "lineRef": "PRICE-FARE",
      "itemRef": "ITEM-A",
      "component": "Fare",
      "effect": "CustomerBalance",
      "direction": "Debit",
      "lineRole": "Original",
      "sourceCode": null,
      "sourceName": null,
      "sourceReference": null,
      "calculationKind": "Amount",
      "originalValue": {
        "amount": "100.00",
        "currencyRef": "EUR"
      },
      "saleValue": {
        "amount": "100.00",
        "currencyRef": "EUR"
      },
      "sourceLineRef": "tickets/0/coupons/0/pricings/0",
      "basisType": "OrderService",
      "basisRef": "SERVICE-A",
      "sourceConversionRef": null,
      "appliedConversion": null
    },
    {
      "lineRef": "PRICE-TAX",
      "itemRef": "ITEM-A",
      "component": "Tax",
      "effect": "CustomerBalance",
      "direction": "Debit",
      "lineRole": "Original",
      "sourceCode": "YQ",
      "sourceName": "Carrier imposed charge",
      "sourceReference": null,
      "calculationKind": "Amount",
      "originalValue": {
        "amount": "20.00",
        "currencyRef": "EUR"
      },
      "saleValue": {
        "amount": "20.00",
        "currencyRef": "EUR"
      },
      "sourceLineRef": "tickets/0/coupons/0/pricings/1",
      "basisType": "OrderService",
      "basisRef": "SERVICE-A",
      "sourceConversionRef": null,
      "appliedConversion": null
    }
  ],
  "customerTotal": {
    "amount": "120.00",
    "currencyRef": "EUR"
  },
  "saleCurrencyCode": "EUR",
  "sourceJourneyTypeRaw": "OneWay",
  "journeyType": null,
  "fareConstruction": {
    "assurance": "Opaque",
    "sourceContextRef": "REFERENCE-CONTEXT-001",
    "pricingUnits": []
  }
}
""";

        public const string SettlementTax = """
{
  "schemaVersion": "3.0",
  "source": {
    "owner": "AirOffer",
    "offerId": "REFERENCE-PRICED-OW-001",
    "providerProfileId": "REFERENCE-OFFER-2.0",
    "ownerBindingRef": "REFERENCE-BINDING-001",
    "sourcePayloadHash": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
  },
  "acceptanceAssurance": "OwnerBound",
  "pricedAt": "2026-10-01T10:00:00Z",
  "capturedAt": "2026-10-01T10:00:00Z",
  "validity": {
    "offer": {
      "state": "Known",
      "value": "2026-10-01T10:10:00Z",
      "owner": "AirOffer",
      "sourceRef": "REFERENCE-OFFER-001",
      "reason": null
    },
    "price": {
      "state": "Known",
      "value": "2026-10-01T10:05:00Z",
      "owner": "AirPrice",
      "sourceRef": "REFERENCE-PRICE-001",
      "reason": null
    },
    "ticketing": {
      "state": "NotSupplied",
      "value": null,
      "owner": "Unresolved owner",
      "sourceRef": null,
      "reason": "Not supplied by source; no implied infinite validity"
    },
    "observedTicketingDeadline": null
  },
  "salesContext": {
    "ownerAirlineId": "1",
    "financialCustomerId": "100",
    "channel": "Backoffice",
    "sellingOfficeId": "10"
  },
  "travelers": [
    {
      "sourceTravellerRef": "PAX-A",
      "passengerTypeCode": "ADT"
    }
  ],
  "journeys": [
    {
      "journeyRef": "BOUND-1",
      "sequence": 1,
      "sourceDirectionRaw": "Outbound",
      "direction": null,
      "originRef": "AIRPORT-A",
      "destinationRef": "AIRPORT-B"
    }
  ],
  "segments": [
    {
      "segmentRef": "SEG-A",
      "journeyRef": "BOUND-1",
      "kind": "ScheduledAir",
      "originRef": "AIRPORT-A",
      "originTerminalRef": "TERMINAL-1",
      "destinationRef": "AIRPORT-B",
      "destinationTerminalRef": "TERMINAL-2",
      "soldDeparture": "2026-10-15T09:00:00Z",
      "soldArrival": "2026-10-15T11:00:00Z",
      "flightRef": "REFERENCE-FLIGHT-A",
      "flightNumber": "RF100",
      "flightVersion": "1",
      "marketingCarrierRef": "CARRIER-M",
      "operatingCarrierRef": "CARRIER-O",
      "sourceCapacityRef": "CAPACITY-A",
      "duration": 120,
      "aircraftRef": "AIRCRAFT-1",
      "legs": [
        {
          "sourceLegRef": "LEG-A",
          "sequence": 1,
          "originRef": "AIRPORT-A",
          "originTerminalRef": "TERMINAL-1",
          "destinationRef": "AIRPORT-B",
          "destinationTerminalRef": "TERMINAL-2",
          "departure": "2026-10-15T09:00:00Z",
          "arrival": "2026-10-15T11:00:00Z"
        }
      ]
    }
  ],
  "items": [
    {
      "itemRef": "ITEM-A",
      "itemKind": "OfferPackage",
      "sourceOfferItemRef": null,
      "serviceRefs": [
        "SERVICE-A"
      ],
      "acceptedTotal": {
        "amount": "120.00",
        "currencyRef": "EUR"
      },
      "product": {
        "sourceSystem": "AirOffer",
        "sourceOfferId": "REFERENCE-PRICED-OW-001",
        "sourceOfferItemRef": null,
        "productCode": null,
        "productName": null,
        "brandCode": null,
        "brandName": null,
        "productVersion": null
      }
    }
  ],
  "services": [
    {
      "serviceRef": "SERVICE-A",
      "type": "AirTransport",
      "serviceCode": null,
      "name": null,
      "priceTreatment": "SupplierOpaque",
      "supplierPartyRef": null,
      "deliveryProviderRef": null,
      "beneficiaryRefs": [
        "PAX-A"
      ],
      "segmentRefs": [
        "SEG-A"
      ],
      "quantity": "1",
      "quantityUnit": "PassengerSegment",
      "detailSchema": "AirTransport",
      "detailSchemaVersion": 2,
      "details": {
        "cabinRef": "ECONOMY",
        "bookingClass": "Y"
      },
      "checkedBaggage": {
        "pieces": 1,
        "weight": "23",
        "weightUnit": "Kg"
      },
      "cabinBaggage": {
        "pieces": 1,
        "weight": null,
        "weightUnit": null
      },
      "soldTerms": {
        "refundable": true,
        "changeable": true,
        "upgradable": false
      },
      "fulfillmentProfile": {
        "profileRef": "REFERENCE-AIR-ETKT",
        "profileVersion": "1",
        "assurance": "Certified",
        "reservationRequirement": "FlightCapacity",
        "documentKind": "ETKT",
        "fundingRequirement": "Required",
        "capacityUnits": 1
      }
    }
  ],
  "pricingLines": [
    {
      "lineRef": "PRICE-FARE",
      "itemRef": "ITEM-A",
      "component": "Fare",
      "effect": "CustomerBalance",
      "direction": "Debit",
      "lineRole": "Original",
      "sourceCode": null,
      "sourceName": null,
      "sourceReference": null,
      "calculationKind": "Amount",
      "originalValue": {
        "amount": "100.00",
        "currencyRef": "EUR"
      },
      "saleValue": {
        "amount": "100.00",
        "currencyRef": "EUR"
      },
      "sourceLineRef": "tickets/0/coupons/0/pricings/0",
      "basisType": "OrderService",
      "basisRef": "SERVICE-A",
      "sourceConversionRef": null,
      "appliedConversion": null
    },
    {
      "lineRef": "PRICE-TAX",
      "itemRef": "ITEM-A",
      "component": "Tax",
      "effect": "SettlementOnly",
      "direction": "Debit",
      "lineRole": "Original",
      "sourceCode": "YQ",
      "sourceName": "Carrier imposed charge",
      "sourceReference": null,
      "calculationKind": "Amount",
      "originalValue": {
        "amount": "20.00",
        "currencyRef": "EUR"
      },
      "saleValue": {
        "amount": "20.00",
        "currencyRef": "EUR"
      },
      "sourceLineRef": "tickets/0/coupons/0/pricings/1",
      "basisType": "OrderService",
      "basisRef": "SERVICE-A",
      "sourceConversionRef": null,
      "appliedConversion": null
    }
  ],
  "customerTotal": {
    "amount": "120.00",
    "currencyRef": "EUR"
  },
  "saleCurrencyCode": "EUR",
  "sourceJourneyTypeRaw": "OneWay",
  "journeyType": null,
  "fareConstruction": {
    "assurance": "Opaque",
    "sourceContextRef": "REFERENCE-CONTEXT-001",
    "pricingUnits": []
  }
}
""";
    }
}

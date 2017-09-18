# @payjs/core

## About

PaymentsJS is a JavaScript library that facilitates client-side payment processing. This document provides low-level reference material for the `@payjs/core` library.

The [Quick Start Guide](https://github.com/SagePayments/PaymentsJS/blob/master/README.md) provides a high-level overview of working with PaymentsJS, as well as links to additional resources, and is the recommended starting point.

## Table of Contents

1. [Introduction](#Introduction)
    - [Use Cases](#UseCases)
    - [Modules](#Modules)
1. [Request](#Request)
    - [.Send()](#Send)
    - [.parseCallback()](#ParseCallback)
1. [Methods](#Methods)
    - [Credit Card](#CreditCard)
    - [Virtual Check](#ACH)
    - [Vault Token](#Token)
    - [Card Reader](#Device)
    - [Masterpass](#Masterpass)
1. [Operations](#Operations)
    - [Payment](#Payment)
    - [Vault](#Vault)
1. [Features](#Features)
    - [Bank](#mAll)
    - [Billing](#mAll)
    - [Config](#mAll)
    - [Custom](#mAll)
    - [Customer](#mAll)
    - [FSA](#mAll)
    - [Level2](#mAll)
    - [Level3](#mAll)
    - [Postback](#mAll)
    - [Recurring](#mAll)
    - [Shipping](#mAll)

<a name="Introduction"></a>
# Introduction

<a name="UseCases"></a>
## Use Cases

The [`@payjs/ui`](https://www.npmjs.com/package/@payjs/ui) library exposes a pre-built, configurable interface that processes payments through `@payjs/core`. This is the recommended point of entry for most scenarios.

In some applications, however, it makes more sense to integrate directly to the `core` library -- when working with a pre-existing payment form, for instance; or when the requirements for an application are specific enough to rule out use of `@payjs/ui`.

<a name="Modules"></a>
## Modules

`@payjs/core` exposes a set of modules that each manage a single piece of functionality. There's three types of module:

1. `Methods` are ways of running a payment -- eg, `CreditCard` or `ACH`.
1. `Operations` are things you can do *with* a `method` -- eg, `Payment` or `Vault`.
1. `Features` are optional extras -- eg, `Billing` or `Recurring`.

You can think of `methods` as nouns and `operations` as verbs. By this analogy, you create requests by composing phrases like "credit card payment" or "vault ach".

Every `method`, `operation`, and `feature` implements the same interface:

```javascript
import { Module } from '@payjs/foo/bar/module';

console.log(Module);
// ==> Object {
//    name: 'module'
//    data: Object {
//      something: undefined,
//      somethingElse: undefined,
//      yetAnotherThing: undefined,
//   }
// }
```
To configure a module, you simply tweak its `data` property.

<a name="Request"></a>
# Request

`@payjs/core/api/request`

*The Request module is used to send requests to the PaymentsJS API.*

<a name="Send"></a>
## .Send()

`Send` is a function that is used to initiate requests to the PaymentsJS API.

Pass in a single configuration object as a parameter:

```javascript
import { Send } from '@payjs/core/api/request';
import { CreditCard } from '@payjs/core/methods/card';
import { Payment } from '@payjs/core/operations/payment';
import { Billing } from '@payjs/core/features/billing';

// ...[ configure modules here ]...

Send({
  methods: [ CreditCard ],
  operations: [ Payment ],
  features: [ Billing ],
  options: { /* ... */ },
  callback: (err, ...args) => { /* ... */ }
});
```

The properties of the configuration object are documented below.

### methods, operations, & features

The `methods`, `operations`, and `features` properties each receive an array of their respective module-type.

- If multiple `methods` are provided, only the first one is used.
- If multiple `operations` are provided, the first is considered the "main" operation; subsequent modules are used to add additional functionality *when applicable*. For instance, `[ Payment, Vault ]` will add vault functionality to a payment, but `[ Vault, Payment ]` is identical to just `[ Vault ]`.
- If multiple `features` are provided, each of them will be processed individually. The order in which they are provided has no effect.

### options

The `options` property is an object with `auth` and `debug` properties:

```javascript
Send({
  // ...
  options: {
      auth: { /*...*/ },
      debug: { /*...*/ }
  },
  // ...
});
```

### options.auth

The `options.auth` object contains information used to authenticate and authorize a request to the PaymentsJS API:

```javascript
Send({
  // ...
  options: {
        // ...
        auth: {
            url,
            requestId,
            environment,
            authKey,
            salt,
            clientId,
            merchantId,
            merchantKey,
        }
  },
  // ...
});
```
- The `auth.url` triggers "Client-First Authentication" -- please see the Authentication section of the [Developer Guide](https://github.com/SagePayments/PaymentsJS/blob/master/README.md) for more information.
- The `requestId` can be any unique or sufficiently-unique alphanumeric string. It's echoed back in the response.
- `environment` defaults to `cert`; switch it to `prod` when you go live.
- The `authKey` and `salt` authorize the request -- please see the Authentication section of the [Developer Guide](https://github.com/SagePayments/PaymentsJS/blob/master/README.md) for more information.
- The `clientId` authenticates the calling application.
- The `merchantId` and `merchantKey` identify the gateway account. **The `merchantKey` should be included in the server-side `authKey` calculation, but never exposed to the browser.** The `merchantId` is necessary for certain client-side features -- eg, logging and Kount -- but is *required* only in the `authKey`.

### options.debug

The `options.debug` object is used for general developer QoL:

```javascript
Send({
  // ...
  options: {
      // ...
      debug: {
          verbose: false,
          logInterval: 10000,
      }
  },
  // ...
});
```

- `options.debug.verbose` is a boolean that defaults to `false`. Setting it to `true` causes PaymentsJS to output logs to the browser console.
- `options.debug.logInterval` is a number that defaults to `10000`. This value controls the frequency at which the client library flushes its internal logs. Set it to `0` to disable log flushing.

### callback

The `callback` property is a function that executes after the API request completes:

```javascript
Send({
  // ...
  callback: (err, xhr, data, hash) => { /*...*/ }
  // ...
});
```

- The `err` parameter contains a boolean that denotes a processing error. Its value is `true` when the API returns a `4XX` or `5XX`, or when the library handles a client-side exception. (A declined transaction is not an error!)
- The `xhr` parameter contains the [`XmlHttpRequest`](https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest) associated with the request.
- The `data` parameter contains the response from the gateway (`xhr.responseText`), or any exception that prevented the request from being sent.
- The `hash` parameter contains a SHA512 hash (hmac) of `data`, using your `clientKey` to sign. This should be used to verify response integrity.

The `callback` function pairs well with `.parseCallback()` (below)

<a name="ParseCallback"></a>
## .parseCallback()

`parseCallback` is a convenience function that can be used in conjunction with your `callback` (above).

Its parameters are the same as `callback`:

```javascript
import { Send, parseCallback } from '@payjs/core/api/request';
Send({
  // ...
  callback: (err, xhr, data, hash) => {
      const parsedResponse = parseCallback(err, xhr, data, hash)
  }
  // ...
});
```

This returns an object that contains and extends the callback parameters:

```javascript
{
    err,
    xhr,
    data,
    hash,
    parsed,
    processingResult,
    avsResult,
    cvvResult,
}
```

- `err`, `xhr`, `data`, and `hash` are passed straight through from the callback
- `parsed` contains the result of `JSON.parse(data)`
- `processingResult` contains `'APPROVED'`, `'DECLINED'`, or `'ERROR'`. In rare cases, it may contain `'UNKNOWN'`.
- `avsResult` contains `'EXACT'`, `'PARTIAL'`, or `'NONE'`. When processing a `CreditCard` with `Billing` data, this represents the result of address verification.
- `cvvResult` contains `'EXACT'` or `'NONE'`. When processing a `CreditCard`, this represents whether the security code (cvv) was a match.

(Note: the `request` module's `parseCallback` function is a reexport from `@payjs/core/api/response`. They're the same function, exported twice purely for convenience.)


<a name="Methods"></a>
# Methods

`@payjs/core/methods/...`

*Methods are things you can use to run a payment.*

<a name="mAll"></a>
## All

The `all` module is simply a convenience object that reexports the other `methods`. You can use this to reduce boilerplate when importing multiple `methods`:

```diff
- import { CreditCard } from '@payjs/core/methods/card'
- import { ACH } from '@payjs/core/methods/ach'
- import { VaultToken } from '@payjs/core/methods/token'
+ import { CreditCard, ACH, VaultToken } from '@payjs/core/methods/all'
```

<a name="CreditCard"></a>
## Credit Card

The `CreditCard` method is used to process -- drumroll -- credit cards!

```javascript
{
    name: 'card',
    data: {
        number,
        expiration,
        cvv
    }
}
```

- The `data.number` property is a string containing the primary account number; eg, `'5454545454545454'`.
- The `data.expiration` property is a string containing the card expiration date in `MMYY` format; eg, `'0928'`.
- The `data.cvv` property is a string containing the card security code; eg, `'123'` or `'6789'`.

<a name="ACH"></a>
## Virtual Check

The `ACH` method is used to process bank information:

```javascript
{
    name: 'ach',
    data: {
        routingNumber,
        accountNumber,
        type,
        secCode
    }
}
```

- The `data.routingNumber` property is a string that identifies the banking institution; eg, `'123456789'`.
- The `data.accountNumber` property is a string that identifies the specific account at the banking institution; eg, `''`.
- The `data.type` property is a string that identifies the type of bank account; eg, ``Checking'` or `'Savings'`.
- The `data.secCode` property is a string that identifies the way in which the payment has been authorized; eg, `'WEB'`, `'PPD'`, `'CCD'`, `'ARC'`, `'TEL'`, `'RCK'`.

Note that ACH transactions typically require additional `Billing` and `Customer` information.

<a name="Token"></a>
## Vault Token

The `VaultToken` method is used to process other payment methods that were previously stored in the vault:

```javascript
{
    name: 'token',
    data: {
        token,
        cvv,
        secCode
    }
}
```

- The `data.token` property is a string that contains the vault identifier; eg, `'84a14c1f853d4c0dabbaa9dad49913b5'`.
- The `data.cvv` and `data.secCode` properties are equivalent to their `CreditCard` and `ACH` counterparts, respectively. Use the appropriate one depending on the type of the original `method` that was stored/referenced.

<a name="Device"></a>
## Card Reader

**`IMPORTANT: this method is not ready for production use.`**

The `Device` method is used to swipe cards through a physical reader:

```javascript
{
    name: 'device',
    data: {
        type,
        data
    }
}
```

- The `data.type` property is a string that identifies the encryption type of the card reader; potential values include:
    - `'IDTech'`
    - `'MagTek'`
    - `'MagTekCenturion'`
- The `data.data` property is a string that contains the encrypted output from the reader; eg, `'02D201801F442800839B%*5454...'`.


<a name="Masterpass"></a>
## Masterpass

**`NOTE: this method should only be used with @payjs/ui.`**

The `MasterPass` method is used to retrieve data from MasterCard's digital wallet service:

```javascript
{
    name: 'masterpass',
    data: {
        version,
        callbackUrl,
        successCallback,
        failureCallback,
        cancelCallback,
        allowedCardTypes
    }
}
```

- The `data.version` property is a string that should be set to `'v6'`.
- The `data.callbackUrl` property is a string that should be set to the page that's running PaymentsJS; eg, `'https://www.example.com/shop/cart.html'`.

The remaining properties of `data` are documented in [the "Lightbox" section of MasterCard's Masterpass documentation](https://developer.mastercard.com/documentation/masterpass-merchant-integration#lightbox-implementation). Parameters that are listed there but not mentioned here -- eg, `requestToken` -- are handled for you by PaymentsJS.

<a name="Operations"></a>
# Operations

`@payjs/core/operations/...`

*Operations are things you can do with a payment method.*

<a name="oAll"></a>
## All

The `all` module is simply a convenience object that reexports the other `operations`. You can use this to reduce boilerplate when importing multiple `operations`:

```diff
- import { Payment } from '@payjs/core/operations/payment'
- import { Vault } from '@payjs/core/operations/vault'
+ import { Payment, Vault } from '@payjs/core/operations/all'
```

<a name="Payment"></a>
## Payment

The `Payment` operation is used to process a transaction:

```javascript
{
    name: 'payment',
    data: {
        totalAmount,
        orderNumber,
        preAuth,
        taxAmount,
        shippingAmount,
        tipAmount,
    }
}
```

- The `data.totalAmount` property is a string containing the total amount to be charged; eg, `'100.00'`.
- The `data.orderNumber` property is a string containing an order number for the transaction; eg, `'Invoice123'`.
- The `data.preAuth` property is a boolean that can be set to `true` to run a `Preauthorization`, instead of the default `Sale`. Preauth transactions must be manually captured before they expire. This setting only affects the `CreditCard` method.
- The `data.taxAmount`, `data.shippingAmount`, and `data.tipAmount` properties contain strings that contain the relevant dollar amounts; eg, `'5.00'`.


<a name="Vault"></a>
## Vault

The `Vault` operation is used to process a transaction:

```javascript
{
    name: 'vault',
    data: {
        token,
        operation
    }
}
```

- The `data.token` property is a string that contains the vault identifier; eg, `'84a14c1f853d4c0dabbaa9dad49913b5'`.
- The `data.operation` property is a string that contains the CRUD operation to perform on the `token`; ie, `'Create'`, `'Read'`, `'Update'`, or `'Delete'`.


<a name="Features"></a>
# Features

`@payjs/core/features/...`

*Features are optional add-ons for your requests.*

<a name="fAll"></a>
## All

The `all` module is simply a convenience object that reexports the other `features`. You can use this to reduce boilerplate when importing multiple `features`:

```diff
- import { Billing } from '@payjs/core/features/billing'
- import { Shipping } from '@payjs/core/features/shipping'
- import { Customer } from '@payjs/core/features/customer'
+ import { Billing, Shipping, Customer } from '@payjs/core/features/all'
```

<a name="Bank"></a>
## Bank

The `Bank` feature is used to retrieve information about the `CreditCard` that was used to run a transaction:

```javascript
{
    name: 'bank',
    data: {
        retrieve
    }
}
```

The `data.retrieve` property is a boolean that defaults to `false`. When set to `true`, the gateway response will include additional information about the `CreditCard` that was used:

```javascript
{
    "BIN": "511491",
    "ARDF": {
        "Usage": " ",
        "Combo": " ",
        "FundingSource": "D",
        "ProductID": "E ",
        "ProductDescription": "Proprietary ATM"
    },
    "GCMCs": [
        {
            "Code": "MCG",
            "Description": "Gold MasterCard Card"
        }
    ]
}
```

Please contact developer support for assistance interpreting this data. Usage of this feature may be subject to approval and/or quotas.

<a name="Billing"></a>
## Billing

The `Billing` feature is used to add a customer's billing address to a transaction:

```javascript
{
    name: 'billing',
    data: {
        name,
        address,
        city,
        state,
        postalCode,
        country
    }
}
```

Each property of `data` is a string that contains the relevant piece of address information. The `data.name` property can be given as a string *or* an object:

```javascript
{
    first,
    middle,
    last,
    suffix,
]
```

<a name="Config"></a>
## Config

The `Config` feature is used to configure certain aspects of a gateway request:

```javascript
{
    name: 'config',
    data: {
        allowPartialAuthorization,
        cardPresent,
        authorizationCode,
        deviceId,
        terminalNumber,
        serialNumber,
        kernelVersion,
    }
}
```

Use of this feature is relatively rare.

<a name="Custom"></a>
## Custom

The `Custom` feature is used to pass arbitrary business data:

```javascript
{
    name: 'custom',
    data: {
        foo: 'bar'
    }
}
```

The contents of `data` have no effect on the request, but they are echoed back in the response (and included in the response hash).


<a name="Customer"></a>
## Customer

The `Customer` feature is used to pass identity and contact information:

```javascript
{
    name: 'customer',
    data: {
        email,
        telephone,
        fax,
        ein,
        ssn,
        dateOfBirth,
        license: {
            number,
            stateCode
        }
    }
}
```

<a name="Debit"></a>
## Debit

**`IMPORTANT: this feature is not ready for production use.`**

The `Debit` feature is used for debit card processing:

```javascript
{
    name: 'debit',
    data: {
        pin,
        cashback
    }
}
```

- The `data.pin` property is a string containing the PIN for the debit card; eg, `'1234'`.
- The `data.cashback` property is a string containing the cashback amount; eg, `'1.00'`.

<a name="FSA"></a>
## FSA

The `FSA` feature is used for healthcare processing:

```javascript
{
    name: 'fsa',
    data: {
        iiasVerification,
        amounts: {
            healthCare,
            prescription,
            clinic,
            dental,
            vision
        }
    }
}
```

- The `data.iiasVerification` property is a string that Iidentifies if the purchase items were verified against an Inventory Information Approval System; eg, `'Verified'`, `'NotVerified'`, or `'Exempt'`.
- The properties of the `data.amounts` property are strings that denote the dollar amounts associated with their respective categories; eg, `'100.00'`.

<a name="Level2"></a>
## Level2

The `Level2` feature is used to send the data needed to qualify for level2 rates:

```javascript
{
    name: 'level2',
    data: {
        customerNumber
    }
}
```

The `data.customerNumber` property is a string that contains the additional identifier that is present on purchase cards.


<a name="Level3"></a>
## Level3

The `Level3` feature is used to send the data needed to qualify for level3 rates:

```javascript
{
    name: 'level3',
    data: {
        customerNumber,
        destinationCountryCode,
        amounts: {
            discount,
            duty,
            nationalTax, 
        },
        vat: {
            idNumber,
            invoiceNumber,
            amount,
            rate
        }
    }
}
```

- The `data.customerNumber` property is a string that contains the additional identifier that is present on purchase cards.
- The `data.destinationCountryCode` property is a string that represents the abbreviation or numeric code of the country to which the goods will be shipped; eg, `'840'` or `'USA'`.
- The properties of the `data.amounts` property are strings that denote the dollar amounts associated with their respective categories; eg, `'100.00'`.
- The properties of the `data.vat` property are used to configure the data required for government purchase cards.

Note that Level 3 processing requires [an additional API call](https://developer.sagepayments.com/bankcard-ecommerce-moto/apis/post/charges/%7Breference%7D/lineitems) to append line item details to a transaction.

<a name="Postback"></a>
## Postback

The `Postback` feature is used to automatically forward API responses to a specified URL:

```javascript
{
    name: 'postback',
    data: {
        url,
        callback
    }
}
```

- The `data.url` property is a string that denotes the URL at which you want to receive the response.
- The `data.callback` is a function that is called after the client forwards its copy of the response.

The specified URL will receive the response *twice*: one directly from the PaymentsJS API, and another forwarded from the client library.

The callback receives the associated `XmlHttpRequest` as its single argument. This callback executes asynchronously; do not rely on it completing before or after any the general callback.


<a name="Recurring"></a>
## Recurring

The `Recurring` feature is used to set up repeat payments that process automatically per a defined schedule:

```javascript
{
    name: 'recurring',
    data: {
        isRecurring,
        recurringSchedule: {
            amount,
            interval,
            frequency,
            nonBusinessDaysHandling,
            startDate,
            totalCount,
            groupId,
        }
    }
}
```

- The `isRecurring` property is a boolean that indicates that a transaction is *part of a recurring series*. This is not required to create a recurring transaction.
- The `recurringSchedule.amount` property is a string that denotes the amount that is charged each time the transaction automatically processes.
- The `recurringSchedule.interval` and `recurringSchedule.frequency` properties combine to define how often the transaction automatically processes. For instance, an `interval` of `'Monthly'` and a frequency of `3` result in a transasction that processes every 3 months (ie, quarterly).
- The `recurringSchedule.startDate` property is a timestamp-like string that represents the day from which to start the automatic processing.
- The `recurringSchedule.nonBusinessDaysHandling` property is a string that tells the schedule how to handle non-business days (holidays, weekends); eg, `'Before'`, `'ThatDay'`, or `'After'`.
- The `recurringSchedule.totalCount` property is a number that tells the schedule how many times to process before disabling itself. If not provided, the schedule runs indefinitely.
- The `recurringSchedule.groupId` is a string that identifies the collection to which the recurring schedule should be added.

<a name="Shipping"></a>
## Shipping

The `Shipping` feature is used to add a shipping address to a transaction:

```javascript
{
    name: 'shipping',
    data: {
        name,
        address,
        city,
        state,
        postalCode,
        country
    }
}
```

This feature's `data` is identical to `Billing.data`, except `name` should *only* be given as a string.

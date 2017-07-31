**`IMPORTANT: this is draft documentation for an upcoming release of PaymentsJS. The contents of this repository should be considered entirely ephemeral, subject to change, unreliable, poisonous, etc. If you're reading this, you're probably in the wrong place.`**

# PaymentsJS Developer Guide

*PaymentsJS is a JavaScript library that brings credit card processing out of the server and into the browser.*

## About

This document is intended to give developers an overview of the PaymentsJS library. Samples are typically given in ES6 syntax, and assume a workflow that involves npm and a module bundler (although neither are required).

The [GitHub repository](https://github.com/SagePayments/PaymentsJS) includes additional guides and sample code, tailored to more specific scenarios -- importing the UI as a React component, for instance, or using a `<script>` tag and the `PayJS` global instead of `npm` and a bundler. It also contains authentication samples in various server-side languages.

The [npm packages](https://www.npmjs.com/org/payjs) dive into more detail on their respective features.

## Table of Contents

1. [Introduction](#Introduction)
    - [Modules](#Modules)
    - [Running a Payment](#Payment)
    - [Using the UI](#UI)
    - [Next Steps](#Next)
1. [Authentication](#Authentication)
    - [Overview](#Overview)
    - [Client-First Auth](#ClientFirst)
    - [Server-First Auth](#ServerFirst)
    - [Response Integrity](#Response)
    - [Next Steps](#NextAuth)
1. [Frequently Asked Questions](#FAQ)

<a name="Introduction"></a>
# Introduction

<a name="Modules"></a>
## Modules

PaymentsJS exposes a set of modules that each manage a single piece of gateway functionality. There's three basic types of module:

1. `methods` are *things you can use to run a payment* -- eg, `CreditCard` or `ACH`.
1. `operations` are things you can do *with* a method -- eg, `Payment` or `Vault`.
1. `features` are optional extras, like `Billing` or `Recurring`.

You can think of `methods` as nouns and `operations` as verbs (and `features` as adjectives, maybe). By this analogy, you create requests by composing *sentences* -- "a recurring credit card payment", for example.

A typical PaymentsJS flow goes something like this:

1. Import at least one module from `methods`, at least one from `operations`, and none/any/all of the `features`.
1. Configure each module.
1. Pass them to `Request.Send()` or `UI.Render()`.

We also need to sign the request, but we'll cover that later.

<a name="Payment"></a>
## Running a Payment

Suppose we want to charge $1.00 to a credit card.

We'll grab the modules:
```javascript
import { Send } from '@payjs/core/api/request';
import { CreditCard } from '@payjs/core/methods/card';
import { Payment } from '@payjs/core/operations/payment';
```

Configure them:
```javascript
CreditCard.data = {
  number: '5454545454545454',
  expiration: '0928',
  cvv: '123'
};

Payment.data = {
  totalAmount: '1.00'
};
```

And send the request:
```javascript
Send({
  methods: [ CreditCard ],
  operations: [ Payment ],
  options: { /* ... */ },
  callback: (err, ...args) => { /* ... */ }
});
```


<a name="UI"></a>
## Using the UI

Let's make some updates:

- the user should be able to pay with card *or* check
- we'll collect the billing address while we're at it
- instead of building a form and wiring it up with `Request.Send()`, we'll use the `UI` module

First, some new modules:
```diff
- import { Send } from '@payjs/core/api/request';
+ import { Render } from '@payjs/ui';

- import { CreditCard } from '@payjs/core/methods/card';
+ import { CreditCard, ACH } from '@payjs/core/methods/all';

import { Payment } from '@payjs/core/operations/payment';
+ import { Billing } from '@payjs/core/features/all'
```

We're going to let PaymentsJS collect the billing & payment data, so let's *de*configure a little:
```diff
- CreditCard.data = {
-   number: '5454545454545454',
-   expiration: '0928',
-   cvv: '123'
- };

Payment.data = {
  totalAmount: '1.00'
};
```

And then switch to our new `Render` import:
```diff
- Send({
+ Render({
-   methods: [ CreditCard ],
+   methods: [ CreditCard, ACH ], // <-- let the user decide
    operations: [ Payment ],
+   features: [ Billing ],
    options: { /* ... */ },
+   uiOptions: {
+     target: 'someElementId' // <-- the ui renders here
+     show: [ Billing ], // <-- collect billing
+   },
    callback: (err, ...args) => { /* ... */ }
  });
```

<a name="Next"></a>
## Next Steps

That's pretty much it -- grab some modules, configure their `data`, and then `send` or `render` them!

For more information on how to authenticate requests, or verify responses, keep reading this document.

For more detailed documentation:

- the [core library docs](https://www.npmjs.com/package/@payjs/core) cover `methods`, `operations`, and `features`
- the [ui library docs](https://www.npmjs.com/package/@payjs/ui) cover customization via `uiOptions`

If you're done reading documentation and ready to play with some code:

1. Clone the repository: `git clone https://github.com/SagePayments/PaymentsJS && cd PaymentsJS`
1. Pick a client sample and run it, eg: `cd client/reactjs && npm install && npm start`
1. Run a server sample in a new shell, eg: `cd server/expressjs && npm install && npm start`


<a name="Authentication"></a>
# Authentication

<a name="Overview"></a>
## Overview

A PaymentsJS request is authorized by creating an `authKey` and passing it through `options.auth`:
```javascript
Send({
  // ...
  options: {
    auth: {
      authKey: 'hi',
    }
  },
  // ...
});
```

The `authKey` is created by taking the JSON request to the PaymentsJS API, adding an `auth` object, and encrypting it with our `clientKey`:
```javascript
function getAuthKey(jsonForApi) {
  const randomBytes = getRandomBytes();
  const apiRequest = JSON.parse(jsonForApi);
  apiRequest.auth = {
    merchantId: '123456789123',
    merchantKey: 'A1B2C3D4E5F6',
    clientId: 'myClientId',
    requestId: 'someOrderNumber',
    salt: base64(randomBytes)
  };
  const authKey = encrypt(JSON.stringify(apiRequest), 'myClientKey', randomBytes);
  return authKey;
}
```

*(That snippet glosses over some important details -- ie, `getRandomBytes()`, `base64()`, and `encrypt()`. This is intended to be a conceptual overview; the [server samples](#) provide more fleshed-out demonstrations, in multiple languages.)*

When the API receives the request, it decrypts the `authKey` -- if decryption fails, or if anything looks tampered-with, the request is rejected.

<a name="ClientFirst"></a>
## Client-First Auth

A "client-first" integration defers `authKey` generation until *after* the user submits their payment details:

```
  BROWSER         SERVER
 +───┬───+      +───┬────+
     └───[Submit]───┐
                    │ [Verify & Authorize]
     ┌───[ Auth ]───┘
     └───────────────────────────────> ...
```

This option is best for situations where you don't know all the details when the page is rendered -- for example, a fundraiser page where users can specify a donation amount.

To trigger client-first auth, set `options.auth.url` in the configuration object:
```javascript
SendOrRender({
  // ...
  options: {
    auth: {
      url: 'https://www.example.com/payjs/authorize'
    }
  },
  // ...
});
```

This tells PaymentsJS to send the request to our server first. We generate the `authKey` and send back some JSON containing the `auth` object:

```json
{ "auth": { "clientId": "myClientId", "requestId": "someOrderNumber", "authKey": "AbCd==", "salt": "EfGh==" }}
```

PaymentsJS waits to receive that back before plugging it into the request and shooting it off to the API.

Here's some things to keep in mind when using client-first auth:

1. *Your merchant credentials should not be included in the response;* we'll pull those from the decrypted `authKey`.
1. The `method` data -- eg, the credit card number -- is *not* included in the auth request.
1. **Always verify the request content before returning any auth data.** 


<a name="ServerFirst"></a>
## Server-First Auth

A "server-first" integration includes the `authKey` as part of the page that's initially rendered to the user:

```
  BROWSER         SERVER
 +───┬───+      +───┬────+
     ┌───[ Auth ]───┘
     └───[Submit]────────────────────> ...
```

This is a good option when you already know the details of the transaction -- for example, on the final checkout page of a shopping cart, where you already know the total amount / etc. -- and just need to collect the payment.

To trigger server-first auth, *don't* configure `options.auth.url`. Instead, plug in the pregenerated `auth` data:
```javascript
SendOrRender({
  // ...
  options: {
    auth: {
      clientId: "myClientId",
      requestId: "someOrderNumber",
      authKey: "AbCd==",
      salt: "EfGh=="
    }
  },
  // ...
});
```

Using server-first auth still allows for a degree of flexibility:

1. The `method` data -- eg, the credit card number -- doesn't need to be part of the `authKey`.
1. Certain features -- `billing`, `shipping`, and `customer` -- don't need to be part of the `authKey`.

Note that this flexibility is itself optional; if you want to enforce the `billing` data, for example, simply include it in the `authKey` and the API will verify it.

<a name="Response"></a>
## Response Integrity

The PaymentsJS API signs gateway responses with a `SHA512` hash (hmac) of the response message, using your `clientKey` as the shared secret. **Always calculate and compare the hash to verify the integrity of the response.**

You can configure a `callback` function to send the response to your server when the client receives it:
```javascript
SendOrRender({
  // ...
  callback: (err, xhr, data, hash) => { /*...*/ },
  // ...
});
```

Or import the `Postback` module and configure a `url`:
```javascript
import { Postback } from '@payjs/core/features/all';

Postback.data = {
  url: 'https://www.example.com/payjs/verify',
};

SendOrRender({
  // ...
  features: [ Postback ],
  // ...
});
```

This automatically sends *two* copies of the response to the specified url: one directly from the API, and one from the client browser.

<a name="NextAuth"></a>
## Next Steps

- If your server is implemented on node, we have [a package](https://www.npmjs.com/package/@payjs/node) that handles creating the `authKey` for you.
- The [server samples](#) demonstrate how to create an `authKey` in other languages.


<a name="FAQ"></a>
# Frequently Asked Questions

## How can I request support?

Use the [Developer Portal](https://developer.sagepayments.com/) to send a message or post on the forums.

## How do I get developer credentials?

For sandbox credentials, register on the [Developer Portal](https://developer.sagepayments.com/). For production credentials, please contact support to schedule a (quick and painless!) certification call.

## How do I get merchant credentials?

The sandbox environment only accepts certain sets of merchant credentials; you can pull these from the server samples. Merchants are responsible for knowing their production credentials, but support can help track them down if needed -- please ask the point of contact on the merchant account to contact their sales rep or the support desk for assistance.

## I've imported and configured a module. Why isn't it being recognized?

Make sure you're passing it into your call to `Request.Send` or `UI.Render` after configuring it!

## How do I configure module *X*?

Import the module you want to inspect, then log it to the console:

```javascript
import { Payment } from '@payjs/core/operations/payment';

console.log(Payment);
// ==> Object {
//    name: 'payment'
//    data: Object {
//      totalAmount: undefined,
//      shippingAmount: undefined,
//      taxAmount: undefined,
//      tipAmount: undefined,
//      orderNumber: undefined,
//      preAuth: undefined,
//   }
// }
```

The packages also include typescript declaration (`*.d.ts`) files:
```typescript
export interface paymentData {
    totalAmount: string;
    shippingAmount?: string;
    taxAmount?: string;
    tipAmount?: string;
    orderNumber?: string;
    preAuth?: boolean;
}
```

For more detail, review the documentation for the package that exposes the module.

## When doing server-first auth, how do I know what the API request is going to look like?

Every module contains `name` and `data` fields (see *"How do I configure module X?"*, above). An API request is a composition of your modules, in the following format:

```javascript
JSON.stringify({ 
  moduleA.name.toLowerCase(): moduleA.data,
  moduleB.name.toLowerCase(): moduleB.data,
  // ...
})
```

For instance, this configuration:

```javascript
import { Foo } from '@payjs/someLib/foo';
import { Bar } from '@payjs/someLib/bar';

Foo.data = {
  a: '123456'
};

Bar.data = {
  b: 'ABCDEF'
};
```

Would produce this JSON:

```json
{ "foo": { "a": "123456" }, "bar": { "b": "ABCDEF" } }
```

The PaymentsJS API verifies the *deserialized* data, so you don't need to worry about order or spacing in the JSON that you encrypt.

For users of the `@payjs/node` package, the `auth/payload` module exposes a `getPayload` function that accepts your configuration object and returns the payload to be encrypted.

## How do I use PaymentsJS UI in a React app?

The UI module exports a React component. Pass in the configuration via `props.config`.

## Can I contribute a sample or recipe?

Yeah, definitely! Please feel free to submit pull requests with any languages, frameworks, or scenarios that aren't already covered by the existing samples.

Server samples should run on `localhost:3001`, and include at least an `/auth` endpoint. You can use the existing samples' merchant and client credentials. Client samples can run at any address, but should have their `options.auth.url` set to `http://localhost:3001/auth`. This allows users to pick up any two samples and run them together without any additional configuration.

## How is PaymentsJS distributed?

PaymentsJS is distributed as packages on NPM. These packages expose CommonJS modules and utilize ES6 syntax -- the assumption is that environments that leverage NPM already have their transpilation and bundling figured out.

There is a static version of the library that can be imported via a `<script>` tag. This version exports window globals, and comes transpiled to ES5.

## Do the PaymentsJS packages have any dependencies?

- `@payjs/core` has no dependencies.
- `@payjs/ui` has the following dependencies:
  - `@payjs/core`
  - `react`
  - `react-dom`
- `@payjs/node` has the following dependencies:
  - `@payjs/core`
  - `crypto-js`

## So, this authKey... tell me all the nerdy details.

That's not a question, but sure!

- The encryption requires an `initialization vector` (or `iv`) that consists of `16` random bytes.
- The `salt` is a base64-encoded version of the `iv`.
- The `password` is derived with `1500` iterations of a `pbkdf2` function, using your `clientKey` and `salt`.
- The `authKey` is an `AES-256-CBC` encryption using the `password` and `iv`.

## How do I report a bug or request a change?

Please submit these through GitHub. For bugs, please include enough information to be able to reliably reproduce the problem.
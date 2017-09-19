# PaymentsJS Quick Start Guide

## About

PaymentsJS is a JavaScript library that facilitates client-side payment processing. This document provides developers an overview of the library by first manually running a payment, then using the built-in UI. It also provides resources for additional information.

## Table of Contents

1. [How it works](#HowItWorks)
1. [Getting started](#GettingStarted)
1. [Running a payment](#Payment)
1. [Using the UI](#UI)
1. [Next steps](#Next)
1. [Frequently Asked Questions](#FAQ)

<a name="HowItWorks"></a>
# How it works

PaymentsJS exposes a set of modules that each manage a single piece of functionality. There's three types of module:

1. `Methods` are ways of running a payment -- eg, `CreditCard` or `ACH`.
1. `Operations` are things you can do *with* a `method` -- eg, `Payment` or `Vault`.
1. `Features` are optional extras -- eg, `Billing` or `Recurring`.

You can think of `methods` as nouns and `operations` as verbs. By this analogy, you create requests by composing phrases like "credit card payment" or "vault ach".

Client-side, a typical PaymentsJS flow goes something like this:

1. Configure some modules: 1+ `methods`, 1+ `operations`, and 0+ `features`.
1. Pass the modules to `Request.Send()` or `UI.Render()`.
1. Handle the response with a callback function.

<a name="GettingStarted"></a>
# Getting Started

Add PaymentsJS to your page:

```html
<script src="https://www.sagepayments.net/pay/dist/2.0.0/pay-core.js"></script> 
```

This exports the `PayJS` variable to the window.

You can prop up a simple authentication back-end by running [any of the server-side samples](https://github.com/SagePayments/PaymentsJS/tree/master/Samples/server). The samples below assume this in `options.auth.url`, but it is not necessary in order to see the client-side behavior.

<a name="Payment"></a>
# Running a Payment

Suppose we want to charge $1.00 to a credit card.

We'll configure our modules:

```javascript
// method:
PayJS.Methods.CreditCard.data = {
  number: '5454545454545454',
  expiration: '0928',
  cvv: '123'
};

// operation:
PayJS.Operations.Payment.data = {
  totalAmount: '1.00'
};
```

And send the request:
```javascript
PayJS.API.Request.Send({
  methods: [ PayJS.Methods.CreditCard ],
  operations: [ PayJS.Operations.Payment ],
  features: [ ],
  options: { auth: { url: 'http://localhost:3001/auth' } },
  callback: (err, xhr, raw, data) => { alert(raw) }
});
```

<a name="UI"></a>
# Using the UI

Let's make some updates:

- the user should be able to pay with card *or* check
- we'll collect the billing address while we're at it

First of all, we need the full library:
```diff
- <script src="https://www.sagepayments.net/pay/dist/2.0.0/pay-core.js"></script> 
+ <script src="https://www.sagepayments.net/pay/dist/2.0.0/pay.js"></script> 
```

The UI uses `react`, `react-dom`, and `react-bootstrap`, so add those too:
```diff
+ <script crossorigin src="https://unpkg.com/react@15/dist/react.min.js"></script>
+ <script crossorigin src="https://unpkg.com/react-dom@15/dist/react-dom.min.js"></script>
+ <script crossorigin src="https://unpkg.com/react-bootstrap@0.31.2/dist/react-bootstrap.min.js"></script>
<script src="https://www.sagepayments.net/pay/dist/2.0.0/pay.js"></script> 
```

We're going to let PaymentsJS collect the user data, so let's *de*configure a little:
```diff
- PayJS.Methods.CreditCard.data = {
-   number: '5454545454545454',
-   expiration: '0928',
-   cvv: '123'
- };

Payment.data = {
  totalAmount: '1.00'
};
```

Add a container to hold the UI:
```html
<div id="myPaymentDiv" style="width: 800px; height:800px"></div>
```

And then switch from `Send` to `Render`:
```diff
- PayJS.API.Request.Send({
+ PayJS.UI.Render({
-   methods: [ PayJS.Methods.CreditCard ],
+   methods: [ PayJS.Methods.CreditCard, PayJS.Methods.ACH ],
    operations: [ PayJS.Operations.Payment ],
-   features: [ ],
+   features: [ PayJS.Features.Billing ],
    options: { auth: { url: 'http://localhost:3001/auth' } },
+   uiOptions: {
+     target: 'myPaymentDiv',
+     show: [ PayJS.Features.Billing ],
+   },
    callback: (err, xhr, raw, data) => { alert(raw) }
  });
```

Add `uiOptions.modal` to switch from an inline form to a pop-up window:
```diff
PayJS.UI.Render({
  methods: [ PayJS.Methods.CreditCard, PayJS.Methods.ACH ],
  operations: [ PayJS.Operations.Payment ],
  features: [ PayJS.Features.Billing ],
  options: { auth: { url: 'http://localhost:3001/auth' } },
  uiOptions: {
    target: 'myPaymentDiv',
    show: [ PayJS.Features.Billing ],
+    modal: { },
  },
  callback: (err, xhr, raw, data) => { alert(raw) }
});
```

<a name="Next"></a>
# Next Steps

That's pretty much it -- grab some modules, configure their `data` property, and then `send` or `render` them!

For more documentation:

- the [core readme](#) covers `Request.Send()` and `options`, as well as the various `methods`, `operations`, and `features`
- the [ui readme](#) covers `UI.Render()` and `uiOptions`
- the [auth readme](#) covers `options.auth`

If you're done reading docs and ready to play with some code:

1. Clone the repository: `git clone https://github.com/SagePayments/PaymentsJS && cd PaymentsJS`
1. Pick a client sample and run it, eg: `cd client/classic && npm install && npm start`
1. Pick a server sample and run it in a new shell, eg: `cd server/expressjs && npm install && npm start`

<a name="FAQ"></a>
# Frequently Asked Questions

## How can I request support?

Use the [Developer Portal](https://developer.sagepayments.com/) to send a message or post on the forums.

## How do I get developer credentials for testing?

Register on the [Developer Portal](https://developer.sagepayments.com/) to obtain sandbox credentials.

## How do I get developer credentials for production?

Please contact support to schedule a certification call. Certification is painless and involves:

1. making sure everything is working as expected, transaction data is being processed correctly, etc.
1. confirming that your application is appropriately verifying all request and response data
1. reviewing the security features of the site that limit access to the payment form (login, Captcha, etc.)

## How do I get merchant credentials?

The sandbox environment only accepts certain sets of merchant credentials; you can pull these from the server samples. Merchants are responsible for knowing their production credentials, but support can help track them down if needed -- please ask the point of contact on the merchant account to contact their sales rep or the support desk for assistance.

## I've imported and configured a module. Why isn't it being recognized?

Make sure you're passing it into your call to `Request.Send` or `UI.Render` after configuring it!

## How do I configure module *X*?

Each module is documented in the [core library docs](#).

You can also log the module you want to inspect to the console:

```javascript
console.log(PayJS.Operations.Payment);
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
PayJS.Foo.data = {
  a: '123456'
};

PayJS.Bar.data = {
  b: 'ABCDEF'
};
```

Would produce this JSON:

```json
{ "foo": { "a": "123456" }, "bar": { "b": "ABCDEF" } }
```

The PaymentsJS API verifies the *deserialized* data, so you don't need to worry about order or spacing in the JSON that you encrypt.

## How do I use PaymentsJS UI in a React app?

The UI module exports two React components: `UI` and `ModalUI`. Pass in the configuration via `props.config`.

## How is PaymentsJS distributed?

PaymentsJS is distributed as a single static file, [`pay.js`](https://www.sagepayments.net/pay/dist/2.0.0/pay.js).  and . 

Both files export are minified and transpiled to ES5.


[`pay-core.js`](https://www.sagepayments.net/pay/dist/2.0.0/pay-core.js)

PaymentsJS is distributed as packages on NPM. These packages expose CommonJS modules and utilize ES6 syntax -- the assumption is that environments that leverage NPM already have their transpilation and bundling figured out.

There is a static version of the library that can be imported via a `<script>` tag. This version exports window globals, and comes transpiled to ES5.

## Does PaymentsJS have any dependencies?

- `@payjs/core` has no dependencies.
- `@payjs/ui` has the following dependencies:
  - `@payjs/core`
  - `react`
  - `react-dom`
  - `react-bootstrap`
- `@payjs/node` has the following dependencies:
  - `@payjs/core`
  - `crypto-js`

## So, this authKey... tell me all the nerdy details.

That's not a question, but sure!

- The encryption requires an `initialization vector` (or `iv`) that consists of `16` random bytes.
- The `salt` is a base64-encoded version of the `iv`.
- The `password` is derived with `1500` iterations of a `pbkdf2` function, using your `clientKey` and `salt`.
- The `authKey` is an `AES-256-CBC` encryption using the `password` and `iv`.

## Can I contribute a sample or recipe?

Yeah, definitely! Please feel free to submit pull requests with any languages, frameworks, or scenarios that aren't already covered by the existing samples.

Server samples should run on `localhost:3001`, and include at least an `/auth` endpoint. You can use the existing samples' merchant and client credentials. Client samples can run at any address, but should have their `options.auth.url` set to `http://localhost:3001/auth`. This allows users to pick up any two samples and run them together without any additional configuration.

## How do I report a bug or request a change?

Please submit these through GitHub. For bugs, please include enough information to be able to reliably reproduce the problem.
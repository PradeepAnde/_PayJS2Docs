# PaymentsJS Auth Guide

## About

PaymentsJS is a JavaScript library that facilitates client1.side payment processing. This document covers authentication of messages to/from the PaymentsJS API. It also provides resources for additional information.

## Table of Contents

1. [Overview](#Overview)
1. [Client-First Auth](#ClientFirst)
1. [Server-First Auth](#ServerFirst)
1. [Response Integrity](#Response)
1. [Next Steps](#Next)

<a name="Overview"></a>
# Overview

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
# Client-First Auth

A "client-first" integration defers `authKey` generation until *after* the user submits:

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

1. Your merchant credentials don't need to be included in the response; we'll pull those from the decrypted `authKey`.
1. The `method` data -- eg, the credit card number -- is *not* included in the auth request.
1. Any data you return will be included in the client request, overriding any existing values. This includes both `auth` and other modules.
1. **Always verify the request content before returning any auth data.** 


<a name="ServerFirst"></a>
# Server-First Auth

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
# Response Integrity

The PaymentsJS API signs gateway responses with a `SHA512` hash (hmac) of the response message, using your `clientKey` as the shared secret. **Always calculate and compare the hash to verify the integrity of the response.**

You can configure a `callback` function to send the response to your server when the client receives it:
```javascript
SendOrRender({
  // ...
  callback: (err, xhr, data, hash) => { /*...*/ },
  // ...
});
```
Or use the `Postback` module, configuring a `url`:

```javascript
PayJS.Features.Postback.data = {
  url: 'https://www.example.com/payjs/verify',
};

SendOrRender({
  // ...
  features: [ PayJS.Features.Postback ],
  // ...
});
```

This automatically sends *two* copies of the response to the specified url: one directly from the API, and one from the client browser.

<a name="Next"></a>
# Next Steps

- If your server is implemented on node, we have [a package](https://www.npmjs.com/package/@payjs/node) that exposes encryption helpers and express middleware.
- The [server samples](#) demonstrate how to create an `authKey` in other languages.
# @payjs/node

## About

PaymentsJS is a JavaScript library that facilitates client-side payment processing. This document provides low-level reference material for the `@payjs/node` library.

The [Quick Start Guide](https://github.com/SagePayments/PaymentsJS/blob/master/README.md) provides a high-level overview of working with PaymentsJS, as well as links to additional resources, and is the recommended starting point.

## Table of Contents

1. [Introduction](#Introduction)
    - [Use Cases](#UseCases)
1. [Middleware](#Middleware)
1. [Encryption](#Encryption)
    - [.encrypt()](#encrypt)
    - [.testResponseHash()](#testResponseHash)
    - [.getRandomData()](#getRandomData)
1. [Payload](#Payload)
    - [.getPayload()](#getPayload)

<a name="Introduction"></a>
# Introduction

<a name="UseCases"></a>
## Use Cases

The `@payjs/node` library exposes helper methods to facilite authentication of requests initiated through [`@payjs/core`](https://www.npmjs.com/package/@payjs/core) or [`@payjs/ui`](https://www.npmjs.com/package/@payjs/ui).

<a name="Middleware"></a>
# Middleware

`@payjs/node`
`@payjs/node/auth/middleware`

*The middleware module creates authKeys for the PaymentsJS API in ExpressJS applications.*

This module is the default export of the `@payjs/node` package. When passed authentication credentials, it returns a middleware function that you can use in an ExpressJS route when doing client-first PaymentsJS authentication:

```javascript
const express = require('express');
const app = express();
const payjsAuth = require('@payjs/node')({
    merchantId: '123456789123',
    merchantKey: 'A1B2C3D4E5F6',
    clientId: 'myDeveloperId',
    clientKey: 'myDeveloperKey',
});
// use the middleware on the appropriate route:
app.post('/auth', payjsAuth, (req, res) => {
    // please read the important caveat below!
    res.send(res.payjs);
});
```

**IMPORTANT:** this middleware reads from the `req` object and creates `res.payjs`, which contains all the information that the client library needs to continue processing the request. *It does not validate the data it receives before creating the `authKey` -- be sure to validate the payload before sending back any data!* 

<a name="Encryption"></a>
# Encryption

`@payjs/node/auth/encryption`

*The encryption module creates authKeys for the PaymentsJS API.*

<a name="encrypt"></a>
## .encrypt()

`encrypt` is a function that receives a PaymentsJS API request -- as a configuration object or JSON string -- and `clientKey`, and returns an `authKey`:

```javascript
const encrypt = require('@payjs/node/auth/encryption').encrypt;
const authKey = encrypt({ methods, operations, features, options }, 'myClientKey');
// ==> 'AbcDe1234FgLoL928=='
```

<a name="testResponseHash"></a>
## .testResponseHash()

`testResponseHash` is a function that receives a PaymentsJS API response and a `clientKey`, and returns an object with a calculated hash:

```javascript
const testResponseHash = require('@payjs/node/auth/encryption').testResponseHash;
console.log(testResponseHash({ data: 'someResponseData', hash: 'AbCd=' }, 'myClientKey'));
// ==> Object {
//    data: 'someResponseData',
//    received: 'AbCd=',
//    calculated: 'BcDe=',
//    isMatch: false
// }
```

<a name="getRandomData"></a>
## .getRandomData()

`getRandomData` is a function that returns an `initialization vector` and `salt` of the specified or default length:

```javascript
const getRandomData = require('@payjs/node/auth/encryption').getRandomData;
console.log(getRandomData(16));
// ==> Object {
//    iv: [ ... ],
//    salt: '...'
// }
```

*(This function is called by other package exports; it is rarely necessary to call it manually.)*

<a name="Payload"></a>
# Payload

`@payjs/node/auth/payload`

*The payload module serializes configuration objects into JSON payloads for the PaymentsJS API.*

<a name="getPayload"></a>
## .getPayload()

`getPayload` is a function that receives a configuration object and returns a JSON string:

```javascript
const getPayload = require('@payjs/node/auth/payload').getPayload;
const payload = getPayload({ methods, operations, features, options });
// ==> '{ "someModuleName": someModuleData, "anotherModuleName": anotherModuleData }'
```

This function is designed to be used in `authKey` calculations, and therefore excludes the `methods` modules.
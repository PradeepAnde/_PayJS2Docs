# PaymentsJS UI Library Reference

## About

PaymentsJS is a JavaScript library that facilitates client-side payment processing. This document provides low-level reference material for the UI library.

The [Quick Start Guide](https://github.com/SagePayments/PaymentsJS/blob/master/README.md) provides a high-level overview of working with PaymentsJS, as well as links to additional resources, and is the recommended starting point.

## Table of Contents

1. [Introduction](#Introduction)
    - [Use Cases](#UseCases)
    - [Modules](#Modules)
1. [UI](#UI)
    - [.Render()](#Render)
    - [&lt;UI&gt;](#React)

<a name="Introduction"></a>
# Introduction

<a name="UseCases"></a>
## Use Cases

The [UI library](#) library exposes a pre-built, configurable interface that processes payments through [the core library](#). This is the recommended point of entry for most scenarios.

In some applications, however, it makes more sense to integrate directly to the core library -- when working with a pre-existing payment form, for instance; or when the requirements for an application are specific enough to rule out use of the UI library.

<a name="Modules"></a>
## Modules

PaymentsJS exposes a set of modules that each manage a single piece of functionality. There's three types of module:

1. `Methods` are ways of running a payment -- eg, `CreditCard` or `ACH`.
1. `Operations` are things you can do *with* a `method` -- eg, `Payment` or `Vault`.
1. `Features` are optional extras -- eg, `Billing` or `Recurring`.

You can think of `methods` as nouns and `operations` as verbs. By this analogy, you create requests by composing phrases like "credit card payment" or "vault ach".

Every `method`, `operation`, and `feature` implements the same interface:

```javascript
console.log(PayJS.Foo.Module);
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

<a name="UI"></a>
# UI

`@payjs/ui`

*The UI module is used to render a pre-built UI that collects payment information and sends it to the Payments API.*

<a name="Render"></a>
## .Render()

`Render` is a function that is used to inject the PaymentsJS interface onto a page.

Pass in a single configuration object as a parameter:

```javascript
// ...[ configure modules here ]...

PayJS.UI.Render({
  methods: [ PayJS.Methods.CreditCard, PayJS.Methods.ACH ],
  operations: [ PayJS.Operations.Payment ],
  features: [ PayJS.Features.Billing ],
  options: { /* ... */ },
  uiOptions: { /* ... */ },
  callback: (err, ...args) => { /* ... */ }
});
```

The properties of the configuration object are documented below. This configuration object is an extension of the configuration object passed into the `Send` function from the core library; only the *differences* are documented here.

### methods, operations, & features

- In the core library, all but the first `method` are ignored; in the UI, the user can choose between each of the specified `methods`. Reorder the array to reorder their presentation in the UI.

### uiOptions

The `uiOptions` object is used to configure the user interface:

```javascript
{
    target,
    modal: {
        autoShow,
        titleText,
        buttonText,
        buttonClass,
        buttonStyle,
    },
    show,
    hide,
    disable,
    disableStyles,
    mockMethodData,
    doKount,
    delimiter,
    allowedCards: { 
        amex,
        disc,
        visa,
        mc,
        error
    },
    suppressResultPage,
}
```

The `target` property is a string that contains the `id` -- no `'#'` needed -- of the element to which the UI should attach.

The `modal` property is an object that defaults to:
```javascript
{
    autoShow: false,
    titleText: 'Payment Details',
    buttonText: 'Pay Now',
    buttonClass: '',
    buttonStyle: {},
}
```
When `modal` is present, the UI is rendered inside a pop-up dialog. The `target` element receives a button that triggers the pop-up, rather than the UI itself. The `titleText` is used in the modal header. The `buttonText` displays on the trigger button. Use `buttonClass` to format the button with an external stylesheet, or `buttonStyle` to override/extend the [inline styles](https://facebook.github.io/react/docs/dom-elements.html#style).

The `show` property is an array of `operations` and `features`. Some modules have UI components associated with them; use this option to display them. (You *don't* need to include your `methods` here.)

The `hide` and `disable` properties are used to remove/disable specific fields of modules that were included in `show`. For instance:

```javascript
{
    // ...
    show: [ Billing ],
    hide: {
        'billing': [ 'country' ]
    },
    disable: {
        'billing': [ 'name' ],
    }
    // ...
}
```

The `disableStyles` property is a boolean that defaults to `false`. Setting it to `true` purges all CSS so you can prettify it to your own requirements.

The `mockMethodData` property is a boolean that defaults to `false`. Setting it to `true` plugs dummy data into the provided modules. This is purely a dev QoL feature.

The `doKount` property is a boolean that defaults to `false`. Setting it to `true` tells the interface to use the Kount service for additional security. (Kount must be enabled on your merchant account.)

The `delimiter` property is a string that defaults to `'-'`. This character is used to format the credit card number field. To disable formatting entirely, set this option to `false`.

The `allowedCards` property is an object that defaults to:
```javascript
{ 
    amex: true,
    disc: true,
    visa: true,
    mc: true,
    error: 'Sorry, we do not accept #type# at this time'
}
```
Setting `amex`, `disc`, `visa`, or `mc` to `false` results in the UI rejecting the corresponding cards. When this occurs the field displays `error`, with any instance of `'#type#'` replaced with the rejected type (eg, `'American Express'`).

The `suppressResultPage` property is a boolean that defaults to `false`. Setting it to `true` prevents the UI from displaying the result page; ie, it remains on the "Processing" page until acted upon.

<a name="React"></a>
## &lt;UI&gt;

The `ui` module also exports the user interface as a React component. Import this when integrating PaymentsJS into a React app, using `props.config` to pass the configuration object:

```javascript
import { UI as PayJS } from '@payjs/ui';

//...

render() {
    <PayJS config={{
        // methods, operations, etc.
    }}>
}
```

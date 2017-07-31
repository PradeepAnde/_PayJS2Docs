# @payjs/ui

*PaymentsJS is a JavaScript library that brings credit card processing out of the server and into the browser.*

## About

This document is intended to be low-level reference material for the `@payjs/ui` library.

The [Developer Guide](https://github.com/SagePayments/PaymentsJS/blob/master/README.md) provides a high-level overview of working with PaymentsJS, and is the recommended starting point.

The [GitHub repository](https://github.com/SagePayments/PaymentsJS) includes additional guides and sample code, tailored to more specific scenarios.

The [npm packages](https://www.npmjs.com/org/payjs) dive into more detail on their respective features.

## Table of Contents

1. [Introduction](#Introduction)
    - [Use Cases](#UseCases)
    - [Modules](#Modules)
1. [UI](#UI)
    - [.Render()](#Render)
    - [<UI>](#React)
1. [Recipes](#Recipes)
    - [All](#rAll)
    - [Point of Sale](#POS)

<a name="Introduction"></a>
# Introduction

<a name="UseCases"></a>
## Use Cases

The [`@payjs/ui`](https://www.npmjs.com/package/@payjs/ui) library exposes a pre-built, configurable interface that processes payments through `@payjs/core`. This is the recommended point of entry for most scenarios.

In some applications, however, it makes more sense to integrate directly to the `core` library -- when working with a pre-existing payment form, for instance; or when the requirements for an application are specific enough to rule out use of `@payjs/ui`.

<a name="Modules"></a>
## Modules

`@payjs/core` exposes a set of modules that each manage a single piece of gateway functionality. There's three basic types of module:

1. `methods` are *things you can use to run a payment* -- eg, `CreditCard` or `ACH`.
1. `operations` are things you can do *with* a method -- eg, `Payment` or `Vault`.
1. `features` are optional extras, like `Billing` or `Recurring`.

You can think of `methods` as nouns and `operations` as verbs (and `features` as adjectives, maybe). By this analogy, you create requests by composing *sentences* -- "a recurring credit card payment", for example.

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

<a name="UI"></a>
# UI

`@payjs/ui`

*The UI module is used to render a pre-built UI that collects payment information and sends it to the Payments API.*

<a name="Render"></a>
## .Render()

`Render` is a function that is used to inject the PaymentsJS interface onto a page.

Pass in a single configuration object as a parameter:

```javascript
import { Render } from '@payjs/ui';
import { CreditCard, ACH } from '@payjs/core/methods/all';
import { Payment } from '@payjs/core/operations/payment';
import { Billing } from '@payjs/core/features/billing';

// ...[ configure modules here ]...

Render({
  methods: [ CreditCard, ACH ],
  operations: [ Payment ],
  features: [ Billing ],
  options: { /* ... */ },
  uiOptions: { /* ... */ },
  callback: (err, ...args) => { /* ... */ }
});
```

The properties of the configuration object are documented below. This configuration object is an extension of the configuration object passed into the `Send` function from `@payjs/core/request`; only the *differences* are documented here.

### methods, operations, & features

- In `@payjs/core`, all but the first `method` are ignored; in `@payjs/ui`, the user can choose between each of the specified `methods`. Reorder the array to reorder their presentation in the UI.

### uiOptions

The `uiOptions` object is used to configure the user interface:

```javascript
{
    target,
    show,
    hide,
    disable,
    disableStyles,
    addFakeData,
    doKount,
    delimiter
}
```

The `target` property is a string that contains the `id` -- no `'#'` needed -- of the element to which the UI should attach. If it's something clickable, like a `button`, the UI renders as a modal window that appears when the target is clicked. Otherwise, if it's a container like a `div` or `span`, the UI renders inside the target.

The `show` property is an array of `operations` and `features`. Some modules have UI components associated with them; use this option to display them. (You *don't* need to include your `methods` here.)

The `hide` and `disable` properties are used to remove/disable specific fields of modules that were included in `show`. For instance:

```javascript
{
    // ...
    show: [ Payment, Billing, SomeModule ],
    hide: {
        'billing': [ 'name', 'country' ]
    },
    disable: {
        'payment': [ 'totalAmount' ],
        'someModule': [ 'someField', 'someOtherField' ]
    }
    // ...
}
```

The `disableStyles` property is a boolean that defaults to `false`. Setting it to `true` purges all CSS so you can prettify it to your own requirements.

The `addFakeData` property is a boolean that defaults to `false`. Setting it to `true` plugs dummy data into the provided modules. This is purely a dev QoL feature.

The `doKount` property is a boolean that defaults to `false`. Setting it to `true` tells the interface to use the Kount service for additional security. (Kount must be enabled on your merchant account.)

The `delimiter` property is a string that defaults to `'-'`. This character is used to format the credit card number field. To disable formatting entirely, set this option to `false`.

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

<a name="Recipes"></a>
# Recipes

`@payjs/ui/recipes`

*Recipes are preconfigured combinations of methods, operations and features.*

Recipes can be imported, adjusted as needed, and then used as a replacement for the configuration object that is passed into `UI.Render`:

```javascript
import { Render } from '@payjs/ui'
import { SomeAdditionalMethod } from '@payjs/core/methods/all'
import { SomeRecipe } from '@payjs/ui/recipes/somerecipe'

SomeRecipe.options.auth = {
    // ...
}

SomeRecipe.methods.push(SomeAdditionalMethod);

UI.Render(SomeRecipe)
```

<a name="rAll"></a>
## All

The `all` module is simply a convenience object that reexports the other `recipes`. You can use this to reduce boilerplate when importing multiple `recipes`:

```diff
- import { PointOfSale } from '@payjs/ui/recipes/pointofsale'
+ import { PointOfSale} from '@payjs/ui/recipes/all'
```

<a name="POS"></a>
## Point of Sale

The `PointOfSale` recipe is configured to mimic a typical retail environment:

**`IMPORTANT: this recipe is not ready for production use.`**

```javascript
{
    methods: [ Device, CreditCard ],
    operations: [ Payment ],
    features: [],
    options: {},
    uiOptions: {
        show: [ Payment ],
        hide: {
            'payment': ['preAuth', 'orderNumber', 'shippingAmount']
        },
        disable: {
           'payment': ['totalAmount', 'taxAmount', 'tipAmount'] 
        }
    }
}
```


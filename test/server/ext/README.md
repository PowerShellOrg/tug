# Testing `TugDSC.Ext`

This folder contains a collection of projects supporting testing of the TugDSC extension model.

We define a *fake* extension hierarchy sample in the `Sample.TestExt` namespace.

There are two supporting library projects:

* `Sample.TestExt.Thingy` - defines the extension interface and its corresponding provider;
  also defines a default implementation (`basic`)
* `Sample.TestExt.DynamicThingy` - defines a second implementation of the extension interface
  and associated provider to be used for dynamic discovery and loading (`dynaThingy`).

Across these two projects, the hierarchy of components is as follows:

* `Sample.TestExt.Thingy`:
  * `IThingy`
  * `IThingyProvider`
  * `SimpleThingyProviderManager` - a provider manager that tests out static
    references (build-time) to extension
  * `DynamicThingyProviderManager` - a provider manager that tests out dynamic
    discovery and loading of extensions

* `Sample.TestExt.Thingy.Impl`:
  * `BasicThingy`
  * `BasicThingyProvider`

* `Sample.TestExt.DynamicThingy.Impl`:
  * `DynamicThingy`
  * `DynamicThingyProvider`


The main testing project `TugDSC.Ext-tests` has an explicit, build-time project reference
to the `Sample.TestExt.Thingy` library project and no reference at all to the
`Sample.TestExt.DynamicThingy` library project.

The first set of tests all focus on verifying the core functions of discovery and resolution
against the statically referenced provider (`basic`).

The latter set of tests add dynamic paths and library references to the second implementation
(`dynaThingy`) to test out dynamic discovery and load features.

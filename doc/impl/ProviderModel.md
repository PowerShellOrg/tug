
# Provider Model

TODO:
Tug exposes a number of extension points to allow customizations.  These extension points follow
a general provider model that is based on MEF 2 to allow for automatic and dynamic discovery of
components that satisfy the interfaces and requires of *providers* that can further be used to
obtain implementations of particular domain-specific logic.

## References

### ASP.NET Core and .NET Core DI
* https://msdn.microsoft.com/en-us/magazine/mt707534.aspx?f=255&MSPPError=-2147217396

### MEF:
* https://blogs.msdn.microsoft.com/bclteam/2011/11/01/getting-started-with-convention-based-part-registration-in-mef-2-nick/
* https://blogs.msdn.microsoft.com/bclteam/2011/11/03/overriding-part-registration-conventions-with-the-mef-attributes-nick/
* https://weblogs.asp.net/ricardoperes/using-mef-in-net-core
* http://blogs.microsoft.co.il/bnaya/2016/02/06/microsoft-composition-portable-mef-attribute-model/

### Cooperation between MEF & DI
* For example, Autofac:
  * http://docs.autofac.org/en/latest/integration/mef.html#providing-autofac-components-to-mef-extensions
  * https://kalcik.net/2014/02/09/cooperation-between-the-autofac-and-the-microsoft-extensibility-framework/


### Dynamic Assembly Loading
* For .NET framework
  * [MSDN Best Practices for Assembly Loading: ](https://msdn.microsoft.com/en-us/library/dd153782.aspx?f=255&MSPPError=-2147217396)
  * [Switching to the Load Context](https://blogs.msdn.microsoft.com/suzcook/2003/06/13/switching-to-the-load-context/)
  * Recent - [Loading .NET Assemblies out of Seperate Folders](https://weblog.west-wind.com/posts/2016/Dec/12/Loading-NET-Assemblies-out-of-Seperate-Folders)
    * Comments have lots of good references, some for .NET Core which are reproduced below
  * [Developing a plugin framework in ASP.NET MVC with medium trust](http://shazwazza.com/post/developing-a-plugin-framework-in-aspnet-with-medium-trust/) - avoids MEF due to assembly locking

* For .NET Core
  * [Custom Assembly Loading with ASP.NET Core](http://shazwazza.com/post/custom-assembly-loading-with-aspnet-core/)
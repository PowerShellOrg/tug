
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

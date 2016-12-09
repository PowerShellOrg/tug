namespace Tug.Ext
{
    /// <summary>
    /// Derivative attributes are used to decorate
    /// <see cref="IExtension">extension</see>
    /// implementations and provide meta data, such as unique name or description.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class,
        Inherited = false, AllowMultiple = false)]
    public abstract class ExtensionAttribute : System.Attribute
    {
        /// <summary>
        /// Returns the name of the target class implementing an associated extension.
        /// </summary>
        protected abstract string Name
        { get; }
    }
}
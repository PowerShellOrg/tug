using System;
using System.Net;

namespace Tug.Client.Configuration
{
    /// <summary>
    /// A helper class that can be used to bind configuration property
    /// values to a concrete type instance when the property type is an
    /// interface or abstract class, or the value is a subclass.
    /// </summary>
    /// <remarks>
    /// This helper class is meant to be used to allow the
    /// <see cref="Microsoft.Extensions.Configuration.ConfigurationBinder"
    /// >Configuration Binder</a> to resolve the concrete value of a configuration
    /// model instance property when that property's type is an interface or
    /// abstract class, or to a subclass of that property's type.  Normally, this
    /// isn't possible as the Binder simply attempts to create an instance of the
    /// type of the property.
    /// <para>
    /// To use this class, create an <i>adjacent, read-only</i> property to the target
    /// property that you're trying to target in your configuration model class, and assign
    /// it the type of this class.  By convention, you name it by appending <c>_UseType</c>
    /// suffix to the name of the target property.  For example, if you target property
    /// is named <c>Foo</c>, then the Use-Type property would be named <c>Foo_UseType</c>.
    /// </para><para>
    /// Then in the constructor of the model class, assign an instance of this class
    /// to the Use-Type property, and give it an action argument that assigns the actions
    /// parameter to the target property.  You have to give this class a type parameter
    /// that is compatible with the target property's type.
    /// </para><para>
    /// Now in the configuration, instead of specifying a value for the target property,
    /// you specify a value for the Use-Type property of a type name that can be constructed
    /// using a no-argument constructor, and then it will be assigned to the target property.
    /// On the target property, you can assign nested properties to configure your target
    /// property value instance.
    /// </para>
    /// </remarks>
    public class UseType<T>
    {
        private Action<T> _action;

        private string _TypeName;

        public UseType(Action<T> action)
        {
            _action = action;
        }

        public string TypeName
        {
            get { return _TypeName; }
            set
            {
                _TypeName = value;
                var t = System.Type.GetType(_TypeName);
                _action((T)Activator.CreateInstance(t));
            }
        }
    }
}
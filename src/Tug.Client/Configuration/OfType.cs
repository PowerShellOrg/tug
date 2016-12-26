using System;

namespace Tug.Client.Configuration
{
    /// <summary>
    /// A helper class that can be used to bind configuration property
    /// values to a concrete type instance (i.e. a configuration model)
    /// when the target property type is an interface or abstract class,
    /// or the target value is a subclass of the target property.
    /// </summary>
    /// This helper class is meant to be used to allow the
    /// <see cref="Microsoft.Extensions.Configuration.ConfigurationBinder"
    /// >Configuration Binder</a> to resolve the concrete value of a configuration
    /// model instance property when that property's type is an interface or
    /// abstract class, or to a subclass of that property's type.  Normally, this
    /// isn't possible as the Binder simply attempts to create an instance of the
    /// type of the property.
    /// <para>
    /// To use this class, replace the type of target property with a wrapper type
    /// of this class and a generic type parameter of the original target type.
    /// Then make that property a <i>read-only</i> property by removing the setter
    /// or simply making it inaccessible (non-public).
    /// </para><para>
    /// Now you can assign to the property an instance of the target type (it will
    /// be implicitly converted) or you can read the target type from the property
    /// by explicitly casting it to the target type.  You can configure the target
    /// property by setting it's child <see
    /// cref="OfType(T).Instance"><c>TypeName</c></see> property to a full typename
    /// (assembly-qualified) and you can further configure the instance that is
    /// wrapped within by specifying configuration properties on
    /// <see cref="OfType(T).Instance"><c>Instance</c></see>.
    /// </para>
    /// </remarks>
    public class OfType<T>
    {
        private string _TypeName;

        private Type _Type;

        private T _Instance;

        private bool _InstanceCreated;

        public string TypeName
        {
            get
            {
                return _TypeName;
            }
            set
            {
                _TypeName = value;
                _Type = System.Type.GetType(_TypeName, true);
            }
        }

        public T Instance
        {
            get
            {
                if (!_InstanceCreated)
                {
                    if (_Type == null)
                        throw new InvalidOperationException("type has not been resolved");

                    _Instance = (T)Activator.CreateInstance(_Type);
                    _InstanceCreated = true;
                }
                return _Instance;
            }
        }

        public bool IsCreated
        {
            get { return _InstanceCreated; }
        }

        public bool IsNull
        {
            get { return !_InstanceCreated || _Instance == null; }
        }

        public override string ToString()
        {
            return Instance?.ToString();
        }

        // These operator overloads are meant to make working with this wrapper
        // type more natural as a surrogate for the target object contained within

        public override int GetHashCode()
        {
            return Instance == null ? 0 : Instance.GetHashCode();
        }

        public override bool Equals(object other)
        {
            return Instance == null ? other == null : Instance.Equals(other);
        }

        public static bool operator ==(OfType<T> tot, T o)
        {
            return object.Equals(tot, o);
        }

        public static bool operator ==(T o, OfType<T> tot)
        {
            return object.Equals(o, tot);
        }

        public static bool operator !=(OfType<T> tot, T o)
        {
            return !object.Equals(tot, o);
        }

        public static bool operator !=(T o, OfType<T> tot)
        {
            return !object.Equals(o, tot);
        }

        public static implicit operator T(OfType<T> tot)
        {
            return tot.Instance;
        }

        public static implicit operator OfType<T>(T o)
        {
            OfType<T> tot = new OfType<T>();
            tot._TypeName = o.GetType().AssemblyQualifiedName;
            tot._Instance = o;
            return tot;
        }
    }
}
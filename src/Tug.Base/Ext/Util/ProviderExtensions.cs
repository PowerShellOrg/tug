/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Tug.Ext.Util
{
    public static class ProviderExtensions
    {
        /// <summary>
        /// This extension function provides a utiliity to support application of parameter
        /// values to an instance of a <see cref="IProviderProduct">Provider Product</see>.
        /// </summary>
        /// <param name="prod">the target product instance to apply settings to</param>
        /// <param name="prodParams">the collection of parameter details to search for</param>
        /// <param name="paramValues">a dictionary of values to be applied as parameters,
        ///    keyed by the unique name given in the parameter details</param>
        /// <param name="strictNames">if true, will throw an error if there are
        ///    parameter values specified for unknown parameter names</param>
        /// <param name="widenToCollections">if true, when matching parameter
        ///    values to the corresponding parameter, if the property type of the
        ///    parameter on an instance is a collection of elements compatible with
        ///    the supplied value type, the value will be assigned after being wrapped
        ///    in a compatible collection</param>
        /// <param name="tryConversion">if true, when the value type is not
        ///    directly assignable to the property type of a parameter, a type conversion
        ///    will be attempted</param>
        /// <param name="requiredEnforced">if true, will throw an exception if any of
        ///    the required parameters are missing from the given values</param>
        /// <param name="filter">an optional function to invoke upon any found parameters
        ///    which would return a tuple indicating if the parameter should be applied
        ///    and if so, an opportunity to transform the supplied value</param>
        /// <exception cref="ArugmentException">
        /// Thrown if there are any failures to apply the supplied parameter values
        /// to the target product instance in the context of the supplied paramter details.
        /// </exception>
        /// <returns>
        /// Returns the same argument product instance that was provided as an argument
        /// in support of a fluid interface.
        /// </returns>
        /// <remarks>
        /// This routine would typically be used by a <see cref="IProvider">Provider
        /// Implementation</see> to apply settings to product instances that it produces.
        /// <para>
        /// If there are any failures to apply any parameter values, an <c>ArgumentException</c>
        /// is thrown and it will contain <see cref="ArgumentException#Data">Data</see> populated
        /// with the parameter names that caused the failure(s).  In the associated Data collection
        /// the following keys will be populated with an enumeration of strings of parameter names
        /// or exceptions parameter that were associated with the corresponding error category:
        /// <list type="bullet">
        /// <item>
        /// <term><c>missingParams</c></term>
        /// <description>parameter names that were missing from the supplied parameter values;
        ///    these will only be checked if <c>requiredEnforced</c> is true</description>
        /// </item>
        /// <item>
        /// <term><c>unknownParams</c></term>
        /// <description>parameter names that were supplied in the parameter values but were not
        ///    found in the supplied parameter info details; these will only be checked if
        ///    <c>strictNames</c> is true</description>
        /// </item>
        /// <item>
        /// <term><c>missingProps</c></term>
        /// <description>parameter names that could not be mapped to properties on the target
        ///    product type; parameter names are case-sensitive and likewise matched against
        ///    property names</description>
        /// </item>
        /// <item>
        /// <term><c>applyFailed</c></term>
        /// <description>parameters that could not be applied and generated exceptions; unlike
        ///    the other Data entries, this collection holds instances of <c>ArgumentException</c>s
        ///    that provides details of the parameter name and the exception encountered when
        ///    the attempt was made to assign to the mapped property</description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        public static Prod ApplyParameters<Prod>(this Prod prod, 
                IEnumerable<ProviderParameterInfo> prodParams,
                IDictionary<string, object> paramValues,
                bool strictNames = false,
                bool requiredEnforced = true,
                bool widenToCollections = true,
                bool tryConversion = true,
                Func<ProviderParameterInfo, object, Tuple<bool, object>> filter = null)
            where Prod : IProviderProduct
        {
            var prodTypeInfo = typeof(Prod).GetTypeInfo();

            var missingParams = new List<string>();
            var unknownParams = new List<string>();
            var unknownProps = new List<string>();
            var applyFailed = new List<ArgumentException>();

            int foundParamValues = 0;
            foreach (var p in prodParams)
            {
                if (!paramValues.ContainsKey(p.Name))
                {
                    if (requiredEnforced && p.IsRequired)
                        missingParams.Add(p.Name);
                    continue;
                }

                // We keep a count so we know if we may
                // have had any unexpected params
                ++foundParamValues;

                // Start with the value given
                var value = paramValues[p.Name];

                // See if a filter was supplied
                if (filter != null)
                {
                    var f = filter(p, value);

                    // Skip the value if the filter indicated so
                    if (!f.Item1)
                        continue;

                    // Get the possibly transformed value
                    value = f.Item2;
                }

                var prop = prodTypeInfo.GetProperty(p.Name, BindingFlags.Public | BindingFlags.Instance);
                var propType = prop.PropertyType;
                var valueType = value?.GetType();
                
                if (prop == null)
                {
                    unknownProps.Add(p.Name);
                    continue;
                }

                if (valueType != null && !propType.IsAssignableFrom(valueType))
                {
                    // Check if we can wrap the value as a collection
                    if (widenToCollections)
                    {
                        // Test for compatible value array
                        if (propType.IsArray && propType.GetElementType().IsAssignableFrom(valueType))
                        {
                            var arr = Array.CreateInstance(valueType, 1);
                            valueType = arr.GetType();
                            arr.SetValue(value, 0);
                            value = arr;
                        }
                        // Test for compatible generic collection
                        else if (propType.IsAssignableFrom(typeof(ICollection<>)
                                .MakeGenericType(valueType)))
                        {
                            var list = Activator.CreateInstance(typeof(List<>)
                                    .MakeGenericType(valueType));
                            valueType = list.GetType();
                            valueType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance)
                                    .Invoke(list, new[] { value });
                            value = list;
                        }
                        // Test for untyped collection
                        else if (propType.IsAssignableFrom(typeof(ICollection)))
                        {
                            var list = new ArrayList(1);
                            valueType = list.GetType();
                            list.Add(value);
                            value = list;
                        }
                    }

                    // Check if we should/can try to convert the value
                    if (!propType.IsAssignableFrom(valueType) && tryConversion)
                    {
                        var typeConv = TypeDescriptor.GetConverter(prop.PropertyType);
                        value = typeConv.ConvertFrom(value);
                        valueType = value?.GetType();
                    }
                }

                try
                {
                    // Best effort to assign the value
                    prop.SetValue(prod, value);
                }
                catch (Exception ex)
                {
                    applyFailed.Add(new ArgumentException(ex.Message, p.Name, ex));
                }
            }

            if (strictNames && foundParamValues < paramValues.Count)
            {
                // Uh oh, there are some parameters we didn't know about
                var paramNames = prodParams.Select(x => x.Name);
                unknownParams.AddRange(paramValues.Keys.Where(x => !paramNames.Contains(x)));
            }

            if (missingParams.Count > 0 || unknownParams.Count > 0
                    || unknownProps.Count > 0 || applyFailed.Count > 0)
            {
                var ex = new ArgumentException("one or more parameters cannot be applied");
                if (missingParams.Count > 0)
                    ex.Data.Add(nameof(missingParams), missingParams);
                if (unknownParams.Count > 0)
                    ex.Data.Add(nameof(unknownParams), unknownParams);
                if (unknownProps.Count > 0)
                    ex.Data.Add(nameof(unknownProps), unknownProps);
                if (applyFailed.Count > 0)
                    ex.Data.Add(nameof(applyFailed), applyFailed);

                throw ex;
            }

            return prod;
        }
    }
}
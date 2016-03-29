using System;
using System.Dynamic;

namespace PersistedDocDemo.Data
{
	/// <summary>
    /// When applied to a property persisted by the SqlRepository the property will be mapped to an SQL column
    /// rather than serialised into JSON in the data column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple=false)]
    public class SqlColumnAttribute : Attribute
    {
     
    }
}
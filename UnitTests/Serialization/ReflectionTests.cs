using System;
using System.Reflection;
using NUnit.Framework;

namespace UnitTests.Serialization
{
    public class ReflectionTests
    {
        public class MyTypeClass
        {
            public String MyProperty1
            {
                get
                {
                    return "hello";
                }
            }
            public String MyProperty2
            {
                get
                {
                    return "hello";
                }
            }
            protected String MyProperty3
            {
                get
                {
                    return "hello";
                }
            }
        }

        [Test]    
        public void Main()
            {
                Type myType = (typeof(MyTypeClass));
                // Get the public properties.
                PropertyInfo[] myPropertyInfo = myType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                System.Diagnostics.Trace.WriteLine($"The number of public properties is {myPropertyInfo.Length}.");
                // Display the public properties.
                DisplayPropertyInfo(myPropertyInfo);
                // Get the nonpublic properties.
                PropertyInfo[] myPropertyInfo1 = myType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
                System.Diagnostics.Trace.WriteLine($"The number of protected properties is {myPropertyInfo1.Length}.");
                // Display all the nonpublic properties.
                DisplayPropertyInfo(myPropertyInfo1);

                try
                {
                    // Get Type object of MyClass.
                    myType = typeof(MyTypeClass);
                    // Get the PropertyInfo by passing the property name and specifying the BindingFlags.
                    PropertyInfo myPropInfo = myType.GetProperty("MyProperty1", BindingFlags.Public | BindingFlags.Instance);
                    // Display Name propety to System.Diagnostics.Trace.
                    System.Diagnostics.Trace.WriteLine("{0} is a property of MyClass.", myPropInfo.Name);
                }
                catch (NullReferenceException e)
                {
                    System.Diagnostics.Trace.WriteLine("MyProperty does not exist in MyClass." + e.Message);
                }

            }
            public static void DisplayPropertyInfo(PropertyInfo[] myPropertyInfo)
            {
                // Display information for all properties. 
                for (int i = 0; i < myPropertyInfo.Length; i++)
                {
                    PropertyInfo myPropInfo = (PropertyInfo)myPropertyInfo[i];
                    System.Diagnostics.Trace.WriteLine($"The property name is {myPropInfo.Name}." );
                    System.Diagnostics.Trace.WriteLine($"The property type is {myPropInfo.PropertyType}.");
                }
            }

        public static object GetFieldValue(object item, string fieldName, 
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            var type = item.GetType();
            return GetFieldValue(type, item, fieldName, flags);
        }

        private static object GetFieldValue(Type type, object item, string fieldName, 
            BindingFlags flags)
        {
            var fld = type.GetField(fieldName, flags);
            if (fld == null)
            {
                if (type.BaseType != null)
                    return GetFieldValue(type.BaseType, fieldName, flags);
                throw new ArgumentException("Unknown field or improper flags");
            }
            return fld.GetValue(item);
        }

        public static object GetPropertyValue(object item, string propertyName, 
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            var type = item.GetType();
            return GetPropertyValue(type, item, propertyName, flags);
        }

        public static object GetPropertyValue(Type type, object item, string propertyName,
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            var prp = type.GetProperty(propertyName, flags);
            if (prp == null)
            {
                if (type.BaseType != null)
                    GetPropertyValue(type.BaseType, item, propertyName, flags);
                throw new ArgumentException("Unknown field or improper flags");
            }

            return prp.GetValue(item, null);
        }
    }
}

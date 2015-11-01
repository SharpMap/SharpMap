using System;
using System.Reflection;
using NUnit.Framework;

namespace BruTile.Tests.Serialization
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
                Console.WriteLine("The number of public properties is {0}.", myPropertyInfo.Length);
                // Display the public properties.
                DisplayPropertyInfo(myPropertyInfo);
                // Get the nonpublic properties.
                PropertyInfo[] myPropertyInfo1 = myType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
                Console.WriteLine("The number of protected properties is {0}.", myPropertyInfo1.Length);
                // Display all the nonpublic properties.
                DisplayPropertyInfo(myPropertyInfo1);

                try
                {
                    // Get Type object of MyClass.
                    myType = typeof(MyTypeClass);
                    // Get the PropertyInfo by passing the property name and specifying the BindingFlags.
                    PropertyInfo myPropInfo = myType.GetProperty("MyProperty1", BindingFlags.Public | BindingFlags.Instance);
                    // Display Name propety to console.
                    Console.WriteLine("{0} is a property of MyClass.", myPropInfo.Name);
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine("MyProperty does not exist in MyClass." + e.Message);
                }

            }
            public static void DisplayPropertyInfo(PropertyInfo[] myPropertyInfo)
            {
                // Display information for all properties. 
                for (int i = 0; i < myPropertyInfo.Length; i++)
                {
                    PropertyInfo myPropInfo = (PropertyInfo)myPropertyInfo[i];
                    Console.WriteLine("The property name is {0}.", myPropInfo.Name);
                    Console.WriteLine("The property type is {0}.", myPropInfo.PropertyType);
                }
            }
    }
}
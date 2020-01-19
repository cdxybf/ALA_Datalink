using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wiring
{
    public static class Wiring
    {
        /// <summary>
        /// if object A (this) has a private field of an interface, and object B implements the interface, then wire them together. Returns this for fluent style programming.
        /// </summary>
        /// <param name="A">
        /// The object on which the method is called is the object being wired from
        /// </param> 
        /// <param name="B">The object being wired to (must implement the interface)</param> 
        /// <returns></returns>
        /// <remarks>
        /// If A has two private fields of the same interface, the first compatible B object wired goes to the first one and the second compatible B object wired goes to the second.
        /// If A has multiple private interfaces of different types, only the first matching interface that B implements will be wired.
		/// In other words, by default, only one interface is wired between A and B
        /// To override this behaviour you can get give multiple interfaces in A a prefix "Pn_" where n is 0..9:
        /// Then a single wiring operation will wire all fieldnames with a consistent port prefix to the same B.
        /// These remarks apply only to single fields, not Lists.
        /// e.g.
        /// private IOneable client1Onabale;
        /// private ITwoable client1Twoable;
        /// private IThreeable client2;
        /// Clearly we want to wire two different clients. But if the first client wired implements all three interfaces, it will be wired to all three fields.
        /// So name the field like this:
        /// private IOneable P1_clientOnabale;
        /// private ITwoable P1_clientTwoable;
        /// private IThreeable P2_client;
        /// When WireTo wires one field starting with Pn_, it will only wire other interfaces of the same B object to fields starting with the same numbered prefix.
        /// If you have fields intended to be wired to different objects, it is best to prefix them with port names, both to make this clear to the reader, and to avoid unexpected bugs should an interface later be added to some abstraction. 
        /// </remarks>
        public static T WireTo<T>(this T A, object B, string APortName = null, bool reverse = false)
        {
            if (A == null)
            {
                throw new ArgumentException("A cannot be null");
            }
            if (B == null)
            {
                throw new ArgumentException("B cannot be null");
            }

            // achieve the following via reflection
            // A.field = (<type of interface>)B;
            // A.list.Add( (<type of interface>)B );

            // Get the twi instance name first for the Debug Output WriteLines
            var AinstanceName = A.GetType().GetProperties().FirstOrDefault(f => f.Name == "instanceName")?.GetValue(A);
            if (AinstanceName == null) AinstanceName = A.GetType().GetFields().FirstOrDefault(f => f.Name == "instanceName")?.GetValue(A);
            var BinstanceName = B.GetType().GetProperties().FirstOrDefault(f => f.Name == "instanceName")?.GetValue(B);
            if (BinstanceName == null) BinstanceName = B.GetType().GetFields().FirstOrDefault(f => f.Name == "instanceName")?.GetValue(B);

            var BType = B.GetType();
            var AfieldInfos = A.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Where(f => (APortName == null || f.Name == APortName) && (!reverse ^ EndsIn_B(f.Name))).ToList(); // find the fields that the name meets all criteria // TBD when not reverse ports ending in _B should be excluded

            var wiredSomething = false;

            firstPortName = null;
            foreach (var BimplementedInterface in BType.GetInterfaces()) // consider every interface implemented by B 
            {
                var AfieldInfo = AfieldInfos.FirstOrDefault(f => f.FieldType == BimplementedInterface && f.GetValue(A) == null); // find the first field in A that matches the interface type of B // TBD the list case below should have the SamePort constraint as well

                // look for normal private fields first
                if (AfieldInfo != null)  // there is a match
                {
                    if (SamePort(AfieldInfo.Name))
                    {
                        AfieldInfo.SetValue(A, B);  // do the wiring
                        wiredSomething = true;
                        WriteLine($"{A.GetType().Name}[{AinstanceName}].{AfieldInfo.Name} wired to {BType.Name}[{BinstanceName}]");
                    }
                    continue;  // could be more than one interface to wire
                }

                // do the same as above for private fields that are a list of the interface of the matching type
                foreach (var AlistFieldInfo in AfieldInfos)
                {
                    if (!AlistFieldInfo.FieldType.IsGenericType)
                    {
                        continue;
                    }
                    var AListFieldValue = AlistFieldInfo.GetValue(A);

                    var AListGenericArguments = AlistFieldInfo.FieldType.GetGenericArguments();
                    if (AListGenericArguments.Length != 1) continue;    // A list should only have one type anyway 
                    if (AListGenericArguments[0].IsAssignableFrom(BimplementedInterface)) // JRS: There was some case where == didn't work, maybe in the gamescoring application
                    {
                        if (AListGenericArguments[0] != BimplementedInterface)
                        {
                            var g = AListGenericArguments[0];
                            if (g != typeof(object)) throw new Exception("Different types");
                            continue;
                        }
                        if (AListFieldValue == null)
                        {
                            var listType = typeof(List<>);
                            Type[] listParam = { BimplementedInterface };
                            AListFieldValue = Activator.CreateInstance(listType.MakeGenericType(listParam));
                            AlistFieldInfo.SetValue(A, AListFieldValue);
                        }

                        AListFieldValue.GetType().GetMethod("Add").Invoke(AListFieldValue, new[] { B });
                        wiredSomething = true;
                        WriteLine($"{A.GetType().Name}[{AinstanceName}].{AlistFieldInfo.Name} wired to {BType.Name}[{BinstanceName}]");
                        break;
                    }

                }
            }

            if (APortName == null && !reverse)
            {
                B.WireTo(A, reverse: true);
            }

            if (!reverse && !wiredSomething)
            {
                if (APortName != null)
                {
                    // a specific port was specified so see if the port was already wired
                    var AfieldInfo = AfieldInfos.FirstOrDefault();
                    if (AfieldInfo.GetValue(A) != null) throw new Exception($"Port already wired {A.GetType().Name}[{AinstanceName}].{APortName} to {BType.Name}[{BinstanceName}]");
                }
                throw new Exception($"Failed to wire {A.GetType().Name}[{AinstanceName}].{APortName} to {BType.Name}[{BinstanceName}]");
            }

            var method = A.GetType().GetMethod("PostWiringInitialize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                InitializeDelegate handler = (InitializeDelegate)Delegate.CreateDelegate(typeof(InitializeDelegate), A, method);
                Initialize -= handler;  // instances can be wired to/from more than once, so only register their PostWiringInitialize once
                Initialize += handler;
            }

            return A;
        }

        /// <summary>
        /// wire B to A and returns A. Used to wire objects to input ports of A.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="A">The object being wire to</param>
        /// <param name="B">The object being wired from</param>
        /// <returns>A</returns>
        public static object WireFrom<T>(this object A, T B, string APortName = null)
        {
            B.WireTo(A, APortName);
            return A;
        }

        // This function always returns true the first time it is called (for a given A and B) but on the second and subsequent calls it only returns true if the name has the same Px_ prefix as the first.
        private static string firstPortName;
        private static bool SamePort(string name)
        {
            if (name.Length >= 3 && name[0] == 'P' && name[2] == '_' && name[1] >= '0' && name[1] <= '9')
            {
                string portName = name.Substring(0, 3);
                if (firstPortName == null)
                {
                    firstPortName = portName;
                }
                return portName.Equals(firstPortName);
            }
            return true;
        }

        private static bool EndsIn_B(string s)
        {
            if (s == null) return false;
            var rv = s.Length > 2 && s.EndsWith("_B");
            if (rv)
            {
            }
            return s.Length > 2 && s.EndsWith("_B");
        }

        public static T IfWireTo<T>(this T A, bool condition, object B)
        {
            if (condition) A.WireTo(B);
            return A;
        }

        private delegate void InitializeDelegate();
        private static event InitializeDelegate Initialize;

        public static void PostWiringInitialize()
        {
            Initialize?.Invoke();
        }

        public delegate void OutputDelegate(string output);
        public static event OutputDelegate Output;

        private static void WriteLine(string output)
        {
            Output?.Invoke(output);
        }
    }
}

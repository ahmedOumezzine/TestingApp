using System;
using System.Reflection;
using System.Collections;

/// <summary>
/// The attribute to use to mark methods as being
/// the targets of benchmarking.
/// </summary>

[AttributeUsage(AttributeTargets.Method)]
public class BenchmarkAttribute : Attribute
{
}

/// <summary>
/// Very simple benchmarking framework. Looks for all types
/// in the current assembly which have public static parameterless
/// methods marked with the Benchmark attribute. In addition, if 
/// there are public static Init, Reset and Check methods with
/// appropriate parameters (a string array for Init, nothing for
/// Reset or Check) these are called at appropriate times.
/// </summary>
public class Benchmark
{
    /// <summary>
    /// Number of times to run the methods in each type.
    /// </summary>
    static int runIterations = 1;

    public void Main()
    {

        // We're only ever interested in public static methods. This variable
        // just makes it easier to read the code...
        BindingFlags publicStatic = BindingFlags.Public | BindingFlags.Static;

        foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
        {
            // Find an Init method taking string[], if any
            MethodInfo initMethod = type.GetMethod("Init", publicStatic, null,
                                                  new Type[] { typeof(string[]) },
                                                  null);

            // Find a parameterless Reset method, if any
            MethodInfo resetMethod = type.GetMethod("Reset", publicStatic,
                                                   null, new Type[0],
                                                   null);

            // Find a parameterless Check method, if any
            MethodInfo checkMethod = type.GetMethod("Check", publicStatic,
                                                  null, new Type[0],
                                                  null);

            // Find all parameterless methods with the [Benchmark] attribute
            ArrayList benchmarkMethods = new ArrayList();
            foreach (MethodInfo method in type.GetMethods(publicStatic))
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters != null && parameters.Length != 0)
                {
                    continue;
                }

                if (method.GetCustomAttributes
                    (typeof(BenchmarkAttribute), false).Length != 0)
                {
                    benchmarkMethods.Add(method);
                }
            }

            // Ignore types with no appropriate methods to benchmark
            if (benchmarkMethods.Count == 0)
            {
                continue;
            }

            Console.WriteLine("Benchmarking type {0}", type.Name);

            // If we've got an Init method, call it once
            try
            {
                if (initMethod != null)
                {
                    initMethod.Invoke(null,null);
                }
            }
            catch (TargetInvocationException e)
            {
                Exception inner = e.InnerException;
                string message = (inner == null ? null : inner.Message);
                if (message == null)
                {
                    message = "(No message)";
                }
                Console.WriteLine("Init failed ({0})", message);
                continue; // Next type
            }

            for (int i = 0; i < runIterations; i++)
            {
                if (runIterations != 1)
                {
                    Console.WriteLine("Run #{0}", i + 1);
                }

                foreach (MethodInfo method in benchmarkMethods)
                {
                    try
                    {
                        // Reset (if appropriate)
                        if (resetMethod != null)
                        {
                            resetMethod.Invoke(null, null);
                        }

                        // Give the test as good a chance as possible
                        // of avoiding garbage collection
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();

                        // Now run the test itself
                        DateTime start = DateTime.Now;
                        method.Invoke(null, null);
                        DateTime end = DateTime.Now;

                        // Check the results (if appropriate)
                        // Note that this doesn't affect the timing
                        if (checkMethod != null)
                        {
                            checkMethod.Invoke(null, null);
                        }

                        // If everything's worked, report the time taken, 
                        // nicely lined up (assuming no very long method names!)
                        Console.WriteLine("  {0,-20} {1}", method.Name, end - start);
                    }
                    catch (TargetInvocationException e)
                    {
                        Exception inner = e.InnerException;
                        string message = (inner == null ? null : inner.Message);
                        if (message == null)
                        {
                            message = "(No message)";
                        }
                        Console.WriteLine("  {0}: Failed ({1})", method.Name, message);
                    }
                }
            }
        }
    }

}
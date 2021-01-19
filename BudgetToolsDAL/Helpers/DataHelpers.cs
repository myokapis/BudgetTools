// TODO: verify that this is no longer needed to keep EF from being ignorant

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;

//namespace BudgetToolsDAL.Helpers
//{
//    public class DataHelpers
//    {
//        protected static List<string> namespaces = new List<string>()
//        {
//            "BudgetToolsDAL.Contexts"
//        };

//        // disables initializers on all contexts to ensure that EntityFramework does not attempt to migrate database objects
//        public static void DisableInitializers()
//        {
//            var assembly = Assembly.GetExecutingAssembly();
//            var types = assembly.GetTypes().Where(t => namespaces.Contains(t.Namespace)).ToList();

//            MethodInfo methodInfo = typeof(Database).GetMethod("SetInitializer");

//            foreach (Type type in types)
//            {
//                if (type.IsSubclassOf(typeof(DbContext)))
//                {
//                    Type[] arrTypes = { type };
//                    MethodInfo generic = methodInfo.MakeGenericMethod(arrTypes);
//                    generic.Invoke(null, new object[] { null });
//                }
//            }
//        }

//    }
//}
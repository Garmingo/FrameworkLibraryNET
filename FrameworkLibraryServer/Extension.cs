﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkLibraryServer
{
    public static class Extension
    {
        public static void Print(this ExpandoObject dynamicObject)
        {
            var dynamicDictionary = dynamicObject as IDictionary<string, object>;

            foreach (KeyValuePair<string, object> property in dynamicDictionary)
            {
                Console.WriteLine("{0}: {1}", property.Key, property.Value.ToString());
            }
            Console.WriteLine();
        }

        public static object GetPropertyValue(object target, string name)
        {
            var site = System.Runtime.CompilerServices.CallSite<Func<System.Runtime.CompilerServices.CallSite, object, object>>.Create(Microsoft.CSharp.RuntimeBinder.Binder.GetMember(0, name, target.GetType(), new[] { Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(0, null) }));
            return site.Target(site, target);
        }
    }
}

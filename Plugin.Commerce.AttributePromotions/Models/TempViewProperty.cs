using Sitecore.Commerce.EntityViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Commerce.AttributePromotions.Models
{
    public class TempViewProperty : ViewProperty
    {
        public static dynamic CreateAnonymousObject(Type type, dynamic val)
        {
            AssemblyName dynamicAssemblyName = new AssemblyName("TempAssembly");
            AssemblyBuilder dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("TempAssembly");

            TypeBuilder dynamicAnonymousType = dynamicModule.DefineType("dynamic", TypeAttributes.Public);

            dynamicAnonymousType.DefineField("TempRawValue", type, FieldAttributes.Public);

            var dynamicType =  dynamicAnonymousType.CreateType();

            dynamic obj =  Activator.CreateInstance(dynamicType);

            obj.TempRawValue = val;

            return obj;
        }
    }
}

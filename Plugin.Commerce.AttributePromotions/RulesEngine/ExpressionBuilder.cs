using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Plugin.Commerce.AttributePromotions.RulesEngine
{
    public class ExpressionBuilder
    {
        private static string[] OperatorTokens => new[]
        {
            "=",
            "!=",
            ">",
            "<",
            ">=",
            "<="
        };

        public static dynamic CreateAnonymousObject(Type type, dynamic val)
        {
            var dynamicAssemblyName = new AssemblyName("TempAssembly");
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule("TempAssembly");

            var dynamicAnonymousType = dynamicModule.DefineType("dynamic", TypeAttributes.Public);

            dynamicAnonymousType.DefineField("TempRawValue", type, FieldAttributes.Public);

            var dynamicType = dynamicAnonymousType.CreateType();

            dynamic obj = Activator.CreateInstance(dynamicType);

            obj.TempRawValue = val;

            return obj;
        }

        public static Expression BuildExpr(Rule r, ParameterExpression param, dynamic obj)
        {
            var left = Expression.Field(param, "TempRawValue");

            ExpressionType tBinary;
//             is the operator a known .NET operator?
            if (Enum.TryParse(r.Operator, out tBinary))
            {
                var right = Expression.Constant(Convert.ChangeType(r.TargetValue, obj.TempRawValue.GetType()));
                // use a binary operation, e.g. 'Equal' -> 'u.Age == 15'
                return Expression.MakeBinary(tBinary, left, right);
            }

            return Expression.Empty();
        }

        public static bool CompileRule(Rule r, dynamic obj)
        {
            var paramUser = Expression.Parameter(obj.GetType());


            r = AssignOperator(r);

            var expr = BuildExpr(r, paramUser, obj);
            // build a lambda function User->bool and compile it
            var compiled = Expression.Lambda(expr, paramUser).Compile();
            return compiled(obj);
        }

        // refer to https://msdn.microsoft.com/en-us/library/bb361179.aspx for more operators
        private static Rule AssignOperator(Rule rule)
        {
            switch (rule.Operator)
            {
                case "=":
                {
                    rule.Operator = "Equal";
                    break;
                }
                case "!=":
                {
                    rule.Operator = "NotEqual";
                    break;
                }
                case ">":
                {
                    rule.Operator = "GreaterThan";
                    break;
                }
                case "<":
                {
                    rule.Operator = "LessThan";
                    break;
                }
                case ">=":
                {
                    rule.Operator = "GreaterThanOrEqual";
                    break;
                }
                case "<=":
                {
                    rule.Operator = "LessThanOrEqual";
                    break;
                }
            }

            return rule;
        }
    }
}
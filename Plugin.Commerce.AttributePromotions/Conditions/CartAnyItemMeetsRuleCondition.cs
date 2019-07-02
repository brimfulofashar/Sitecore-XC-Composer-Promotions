using System;
using System.Linq;
using System.Text.RegularExpressions;
using Plugin.Commerce.AttributePromotions.Arguments;
using Plugin.Commerce.AttributePromotions.RulesEngine;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Views;
using Sitecore.Framework.Rules;
using Rule = Plugin.Commerce.AttributePromotions.RulesEngine.Rule;

namespace Plugin.Commerce.AttributePromotions.Conditions
{
    [EntityIdentifier("Cart Any Item meets [rule]")]
    public class CartAnyItemMeetsRuleCondition : ICartsCondition, ISellableItemCondition, ICondition
    {
        public IRuleValue<string> Rule { get; set; }
        public IRuleValue<int> QuantityFrom { get; set; }
        public IRuleValue<int> QuantityTo { get; set; }

        public bool Evaluate(IRuleExecutionContext context)
        {
            var ruleInput = Rule.Yield(context);
            var quantityFrom = QuantityFrom.Yield(context);
            var quantityTo = QuantityTo.Yield(context);

            var commerceContext = context.Fact<CommerceContext>(null);

            var cart = commerceContext != null ? commerceContext.GetObject<Cart>() : null;

            var qualifiedItemsArgument = commerceContext.GetObject<QualifiedItemsArgument>();

            if (string.IsNullOrEmpty(ruleInput) || commerceContext == null || cart == null || !cart.Lines.Any())
                return false;

            var virtualQualifications = ruleInput.Split('&');

            if (qualifiedItemsArgument.QualifyQuantityThresholds)
            {
                var cartLinesByProductGroup = cart.Lines.GroupBy(x =>
                    x.CartLineComponents.First(y => y.GetType() == typeof(CartProductComponent)).Id);

                if (cartLinesByProductGroup.All(x =>
                    x.Sum(y => y.Quantity) < quantityFrom || x.Sum(y => y.Quantity) > quantityTo))
                    return false;
            }

            var regex = new Regex("[a-zA-Z0-9 ]+");

            var originalQualifiedLinesCount = qualifiedItemsArgument.QualifiedCartLines.Count;

            foreach (var line in cart.Lines)
            {
//                var productId = string.Format("Entity-SellableItem-{0}", line.Id);
                var product = commerceContext.GetObjects<FoundEntity>().First(x =>
                        x.Entity != null && x.Entity.GetType() == typeof(SellableItem) && x.EntityId == line.Id)
                    ?.Entity as SellableItem;
                var productQualified = true;

                var entityViewComponets = product.Components.Where(x => x.GetType() == typeof(EntityViewComponent));
                foreach (EntityViewComponent entityViewComponent in entityViewComponets)
                foreach (EntityView view in entityViewComponent.View.ChildViews)
                foreach (var viewProperty in view.Properties)
                foreach (var virtualQualification in virtualQualifications)
                {
                    var tokens = regex.Matches(virtualQualification);
                    if (viewProperty.Name == tokens[0].Value)
                    {
                        var memberName = "RawValue";
                        var value = tokens[1].Value;
                        var op = virtualQualification.Replace(tokens[0].Value, string.Empty)
                            .Replace(tokens[1].Value, string.Empty);
                        var rule = new Rule(memberName, op, value);


                        var type = Type.GetType(viewProperty.OriginalType);

                        var tempObject = ExpressionBuilder.CreateAnonymousObject(type, viewProperty.RawValue);

                        productQualified = ExpressionBuilder.CompileRule(rule, tempObject);

                        if (!productQualified) break;
                    }
                }

                if (productQualified)
                {
                    qualifiedItemsArgument.QualifiedCartLines.Add(line);

                    var ruleMetArguement = new RuleMetArgument
                    {
                        ItemId = line.ItemId,
                        QuantityFrom = quantityFrom,
                        QuantityTo = quantityTo,
                        PromotionId = context.Fact<CommerceContext>(null)?.GetObject<PropertiesModel>()
                            .GetPropertyValue("PromotionId")?.ToString()
                    };

                    qualifiedItemsArgument.RuleMetArguments.Add(ruleMetArguement);
                }
            }


            var result = qualifiedItemsArgument.QualifiedCartLines.Count > originalQualifiedLinesCount;

            return result;
        }
    }
}
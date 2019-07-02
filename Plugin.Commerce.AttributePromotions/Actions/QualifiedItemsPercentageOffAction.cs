using System;
using System.Linq;
using Plugin.Commerce.AttributePromotions.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Rules;

namespace Plugin.Commerce.AttributePromotions.Actions
{
    [EntityIdentifier("QualifiedItemsPercentageOffAction")]
    public class QualifiedItemsPercentageOffAction : ICartLineAction, ICartsAction, IAction, IMappableRuleEntity
    {
        public IRuleValue<decimal> PercentOff { get; set; }

        public void Execute(IRuleExecutionContext context)
        {
            var commerceContext = context.Fact<CommerceContext>(null);
            var cart = commerceContext?.GetObject<Cart>();
            var totals = commerceContext?.GetObject<CartTotals>();

            var qualifiedItemsArgument = commerceContext?.GetObject<QualifiedItemsArgument>();

            if (cart == null || qualifiedItemsArgument == null || !qualifiedItemsArgument.QualifiedCartLines.Any() ||
                !cart.Lines.Any() || totals == null || !totals.Lines.Any())
                return;
            var list = cart.Lines.Where(x =>
            {
                var productId = x.GetComponent<CartProductComponent>().Id;


                return qualifiedItemsArgument.QualifiedCartLines.Any(y =>
                    y.GetComponent<CartProductComponent>().Id == productId);
            }).ToList();


            if (!list.Any())
                return;

            var percentage = PercentOff.Yield(context);

            var name = commerceContext.GetPolicy<KnownCartAdjustmentTypesPolicy>().Discount;
            var propertiesModel = commerceContext.GetObject<PropertiesModel>();
            list.ForEach(line =>
            {
                var d = percentage * totals.Lines[line.Id].SubTotal.Amount;
                if (commerceContext.GetPolicy<GlobalPricingPolicy>().ShouldRoundPriceCalc)
                    d = decimal.Round(d, commerceContext.GetPolicy<GlobalPricingPolicy>().RoundDigits,
                        commerceContext.GetPolicy<GlobalPricingPolicy>().MidPointRoundUp
                            ? MidpointRounding.AwayFromZero
                            : MidpointRounding.ToEven);
                var amount = d * decimal.MinusOne;
                var adjustments = line.Adjustments;

                var adjustment = new CartLineLevelAwardedAdjustment
                {
                    Name = propertiesModel?.GetPropertyValue("PromotionText") as string ?? name,
                    DisplayName = propertiesModel?.GetPropertyValue("PromotionCartText") as string ?? name,
                    Adjustment = new Money(commerceContext.CurrentCurrency(), amount),
                    AdjustmentType = name,
                    IsTaxable = false,
                    AwardingBlock = nameof(CartAnyItemSubtotalPercentOffAction)
                };

                adjustments.Add(adjustment);

                var currentPromotionId = context.Fact<CommerceContext>(null)?.GetObject<PropertiesModel>()
                    .GetPropertyValue("PromotionId")?.ToString();

                qualifiedItemsArgument.RuleMetArguments
                    .First(x => x.PromotionId == currentPromotionId && x.ItemId == line.ItemId &&
                                x.PercentageOff == null)
                    .PercentageOff = percentage;

                totals.Lines[line.Id].SubTotal.Amount = totals.Lines[line.Id].SubTotal.Amount + amount;
                line.GetComponent<MessagesComponent>().AddMessage(
                    commerceContext.GetPolicy<KnownMessageCodePolicy>().Promotions,
                    string.Format("PromotionApplied: {0}",
                        propertiesModel?.GetPropertyValue("PromotionId") ??
                        nameof(CartAnyItemSubtotalPercentOffAction)));
            });
        }
    }
}
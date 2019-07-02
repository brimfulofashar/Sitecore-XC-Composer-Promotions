using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Commerce.AttributePromotions.Arguments;
using Plugin.Commerce.AttributePromotions.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Availability;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Framework.Pipelines;

namespace Plugin.Commerce.AttributePromotions.Blocks
{
    public class GetSellableItemPromotionsBlock :  PipelineBlock<SellableItem, SellableItem, CommercePipelineExecutionContext>
    {

        private readonly ICalculateCartLinesPipeline _calculateCartPipeline;
        private readonly IAddCartLinePipeline _addCartLinePipeline;

        private readonly IFindEntityPipeline _getCartPipeline;
        private readonly IServiceProvider _serviceProvider;

        public GetSellableItemPromotionsBlock(IAddCartLinePipeline addCartLinePipeline, ICalculateCartLinesPipeline calculateCartPipeline, IFindEntityPipeline getCartPipeline, IServiceProvider serviceProvider)
        {
            _calculateCartPipeline = calculateCartPipeline;
            _addCartLinePipeline = addCartLinePipeline;
            _getCartPipeline = getCartPipeline;
            _serviceProvider = serviceProvider;

        }

        public override Task<SellableItem> Run(SellableItem sellableItem, CommercePipelineExecutionContext context)
        {
            if (context.CommerceContext != null)
            {
                var tempCart = new Cart(QualifiedItemsArgument.TempCartId);

                CartLineComponent line = new CartLineComponent()
                {
                    ItemId = "TestCatalog|TestProduct|Variant2",
                    Quantity = 1,
                    Id = sellableItem.Id
                };
                line.CartLineComponents.Add(new ItemAvailabilityComponent
                {
                    AvailableDate = DateTimeOffset.MinValue,
                    AvailabilityExpires = DateTimeOffset.MaxValue,
                    AvailableQuantity = 1,
                    IsAvailable = true,
                    ItemId = line.ItemId
                });
                tempCart.Lines.Add(line);

                var cart = context.CommerceContext.GetObject<Cart>();
                if (cart == null)
                {
                    var entities = context.CommerceContext.GetObjects<FoundEntity>();
                    cart = entities?.FirstOrDefault(x =>
                        x.Entity != null && x.Entity.GetType() == typeof(Cart))?.Entity as Cart;
                }

                if (cart == null)
                {
                    context.CommerceContext.AddObject(tempCart);

                    var qualifiedItemsArgument = context.CommerceContext.GetObject<QualifiedItemsArgument>();
                    if (qualifiedItemsArgument != null)
                    {
                        qualifiedItemsArgument.QualifyQuantityThresholds = false;
                        context.CommerceContext.RemoveObject(qualifiedItemsArgument);
                        context.CommerceContext.AddObject(qualifiedItemsArgument);
                    }
                    else
                    {
                        qualifiedItemsArgument = new QualifiedItemsArgument(false);
                        context.CommerceContext.AddObject(qualifiedItemsArgument);
                        var promotions = _calculateCartPipeline.Run(tempCart, context).Result;


                        var qualifiedBenefits = qualifiedItemsArgument.RuleMetArguments
                            .Where(x => x.PercentageOff != null).ToList();

                        foreach (var qualifiedBenefit in qualifiedBenefits)
                        {
                            sellableItem.Policies.Add(new QualifiedItemPercentageDiscountPolicy
                            {
                                QuantityFrom = qualifiedBenefit.QuantityFrom,
                                QuantityTo = qualifiedBenefit.QuantityTo,
                                PercentageOff = Convert.ToDecimal(qualifiedBenefit.PercentageOff)
                            });
                        }

                    }


                }
            }


            return Task.FromResult(sellableItem);
        }
    }
}

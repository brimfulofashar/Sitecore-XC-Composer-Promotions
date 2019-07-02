using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Commerce.AttributePromotions.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Framework.Pipelines;

namespace Plugin.Commerce.AttributePromotions.Blocks
{
    public class InjectQualifiedItemsArgumentBlock : PipelineBlock<IEnumerable<Promotion>, IEnumerable<Promotion>, CommercePipelineExecutionContext>
    {
        public override async Task<IEnumerable<Promotion>> Run(IEnumerable<Promotion> arg,
            CommercePipelineExecutionContext context)
        {
            var cart = context.CommerceContext != null ? context.CommerceContext.GetObject<Cart>() : null;

            if (cart == null)
            {
                cart = context.CommerceContext != null ? (Cart)context.CommerceContext.GetObjects<FoundEntity>()?.FirstOrDefault(x => x.Entity.GetType() == typeof(Cart))?.Entity : null;
            }

            if (cart != null && cart.Id != QualifiedItemsArgument.TempCartId)
            {
                var qualifiedItemsArgument = context.CommerceContext.GetObject<QualifiedItemsArgument>();
                if (qualifiedItemsArgument == null)
                {
                    qualifiedItemsArgument = new QualifiedItemsArgument(true);


                }
                else
                {
                    qualifiedItemsArgument.QualifyQuantityThresholds = true;
                    context.CommerceContext.RemoveObject(qualifiedItemsArgument);

                }

                context.CommerceContext.AddObject(qualifiedItemsArgument);
            }


            return await Task.FromResult(arg);
        }
    }
}
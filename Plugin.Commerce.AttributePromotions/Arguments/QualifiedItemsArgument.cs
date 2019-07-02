using System.Collections.Generic;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;

namespace Plugin.Commerce.AttributePromotions.Arguments
{
    public class QualifiedItemsArgument : PipelineArgument
    {
//        public readonly CommercePipelineExecutionContext CommercePipelineExecutionContext;
//        public readonly IFindEntityPipeline FindEntityPipeline;

        public QualifiedItemsArgument(bool qualifyQuantityThresholds)
        {
            QualifyQuantityThresholds = qualifyQuantityThresholds;
        }

        public List<CartLineComponent> QualifiedCartLines = new List<CartLineComponent>();

        public bool QualifyQuantityThresholds { get; set; }

        public static readonly string TempCartId = "TempCart";

        public List<RuleMetArgument> RuleMetArguments = new List<RuleMetArgument>();
    }
}
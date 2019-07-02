using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Pricing;

namespace Plugin.Commerce.AttributePromotions.Arguments
{
    public class RuleMetArgument : PipelineArgument
    {
        public string PromotionId { get; set; }
        public int QuantityFrom { get; set; }
        public int QuantityTo { get; set; }
        public string ItemId { get; set; }
        public decimal? PercentageOff { get; set; }
    }
}
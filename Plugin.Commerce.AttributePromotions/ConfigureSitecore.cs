// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Commerce.AttributePromotions.Blocks;
using Sitecore.Commerce.Plugin.Availability;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Promotions;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions.Extensions;
using Sitecore.Framework.Rules;

namespace Plugin.Commerce.AttributePromotions
{
    /// <summary>
    ///     The configure sitecore class.
    /// </summary>
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        ///     The configure services.
        /// </summary>
        /// <param name="services">
        ///     The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.Sitecore().Pipelines(config =>
                config.ConfigurePipeline<IEvaluatePromotionsQualificationsPipeline>(d =>
                {
                    d.Add<InjectQualifiedItemsArgumentBlock>().After<GetPromotionsToEvaluateBlock>();
                }));
            services.Sitecore().Pipelines(config =>
                config.ConfigurePipeline<IGetSellableItemPipeline>(d =>
                {
                    d.Add<GetSellableItemPromotionsBlock>().After<EnsureSellableItemAvailabilityPoliciesBlock>();
                }));
            services.Sitecore().Rules(config => config.Registry(registry => registry.RegisterAssembly(assembly)));
        }
    }
}
using AutoMapper;
using TokenPriceUpdate.Price.ETOs;

namespace TokenPriceUpdate.EventHandler.BackgroundJob
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            /* You can configure your AutoMapper mapping configuration here.
             * Alternatively, you can split your mapping configurations
             * into multiple profile classes for a better organization. */
            
            CreateMap<Price.Entities.Ef.Price, PriceEto>();
        }
    }
}
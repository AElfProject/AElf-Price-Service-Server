using AutoMapper;
using TokenPriceUpdate.Price.Entities.Es;

namespace TokenPriceUpdate
{
    public class TokenPriceUpdateApplicationAutoMapperProfile : Profile
    {
        public TokenPriceUpdateApplicationAutoMapperProfile()
        {
            /* You can configure your AutoMapper mapping configuration here.
             * Alternatively, you can split your mapping configurations
             * into multiple profile classes for a better organization. */
            CreateMap<Price.Entities.Es.Price, PriceRecord>();
        }
    }
}

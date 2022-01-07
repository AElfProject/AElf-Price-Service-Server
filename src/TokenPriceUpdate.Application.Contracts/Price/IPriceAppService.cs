using System.Threading.Tasks;
using TokenPriceUpdate.Price.DTOs;
using Volo.Abp.Application.Dtos;

namespace TokenPriceUpdate.Price
{
    public interface IPriceAppService
    {
        Task CreateOrUpdateTokenPriceAsync(CreateOrUpdateTokenPriceInput input);
        Task CreateOrUpdateTokenPriceOnEsAsync(CreateOrUpdateTokenPriceOnEsInput input);
        public Task<CurrentTokenPriceDto> GetSwapTokenPriceAsync(GetTokenPriceInput input);
        public Task<CurrentTokenPriceDto> GetExchangePriceAsync(GetTokenPriceInput input);
        public Task<ListResultDto<CurrentTokenPriceDto>> BatchGetSwapTokenPriceAsync(BatchGetTokenPriceInput input);
        public Task<ListResultDto<CurrentTokenPriceDto>> BatchGetExchangePriceAsync(BatchGetTokenPriceInput input);
        public Task<TokenPriceRecordDto> GetExchangePriceRecordsAsync(GetTokenPriceRecordInput input);
        public Task<TokenPriceRecordDto> GetSwapTokenPriceRecordsAsync(GetTokenPriceRecordInput input);
        public Task<TokenPriceDto> GetSwapTokenPriceByDatetimeAsync(GetTokenPriceByDatetimeInput input);
        public Task<TokenPriceDto> GetExchangePriceByDatetimeAsync(GetTokenPriceByDatetimeInput input);
        public void UpdateQueryIndexAndCachePrice(UpdateQueryIndexAndCachePriceInput input);
    }
}
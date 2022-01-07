using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TokenPriceUpdate.Localization;
using TokenPriceUpdate.Price;
using TokenPriceUpdate.Price.DTOs;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace TokenPriceUpdate.Controllers
{
    [RemoteService]
    [Area("app")]
    [ControllerName("Price")]
    [Route("api/app")]
    public class PriceController : AbpController
    {
        private readonly IPriceAppService _priceAppService;

        public PriceController(IPriceAppService priceAppService)
        {
            _priceAppService = priceAppService;
            LocalizationResource = typeof(TokenPriceUpdateResource);
        }

        [HttpGet]
        [Route("swap/price")]
        public virtual async Task<CurrentTokenPriceDto> GetSwapTokenPriceAsync(GetTokenPriceInput input)
        {
            return await _priceAppService.GetSwapTokenPriceAsync(input);
        }

        [HttpGet]
        [Route("exchange/price")]
        public virtual async Task<CurrentTokenPriceDto> GetExchangePriceAsync(GetTokenPriceInput input)
        {
            return await _priceAppService.GetExchangePriceAsync(input);
        }

        [HttpPost]
        [Route("swap/prices")]
        public virtual async Task<ListResultDto<CurrentTokenPriceDto>> BatchGetSwapTokenPriceAsync(
            BatchGetTokenPriceInput input)
        {
            return await _priceAppService.BatchGetSwapTokenPriceAsync(input);
        }

        [HttpPost]
        [Route("exchange/prices")]
        public virtual async Task<ListResultDto<CurrentTokenPriceDto>> BatchGetExchangePriceAsync(
            BatchGetTokenPriceInput input)
        {
            return await _priceAppService.BatchGetExchangePriceAsync(input);
        }

        [HttpGet]
        [Route("exchange/price-record")]
        public virtual async Task<TokenPriceRecordDto> GetExchangePriceRecordsAsync(GetTokenPriceRecordInput input)
        {
            return await _priceAppService.GetExchangePriceRecordsAsync(input);
        }

        [HttpGet]
        [Route("swap/price-record")]
        public virtual async Task<TokenPriceRecordDto> GetSwapTokenPriceRecordsAsync(GetTokenPriceRecordInput input)
        {
            return await _priceAppService.GetSwapTokenPriceRecordsAsync(input);
        }

        [HttpGet]
        [Route("swap/price-by-datetime")]
        public virtual async Task<TokenPriceDto> GetSwapTokenPriceByDatetimeAsync(GetTokenPriceByDatetimeInput byDatetimeInput)
        {
            return await _priceAppService.GetSwapTokenPriceByDatetimeAsync(byDatetimeInput);
        }

        [HttpGet]
        [Route("exchange/price-by-datetime")]
        public virtual async Task<TokenPriceDto> GetExchangePriceByDatetimeAsync(GetTokenPriceByDatetimeInput byDatetimeInput)
        {
            return await _priceAppService.GetExchangePriceByDatetimeAsync(byDatetimeInput);
        }
    }
}
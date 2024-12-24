using AllupPraktika.ViewModels;

namespace AllupPraktika.Services.Implementations
{
    public interface IBasketService
    {
        public Task<List<BasketItemVM>> GetBasketAsync();
    }
}

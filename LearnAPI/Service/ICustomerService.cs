using LearnAPI.Helper;
using LearnAPI.Modal;
using LearnAPI.Repos.Models;

namespace LearnAPI.Service
{
    public interface ICustomerService
    {
        Task<List<CustomerModal>> GetAll();
        Task<CustomerModal> GetByCode(string code);
        Task<APIResponse> Remove(string code);
        Task<APIResponse> Create(CustomerModal customer);

        Task<APIResponse> Update(CustomerModal customer, string code);

    }
}

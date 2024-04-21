using LearnAPI.Modal;
using LearnAPI.Repos.Models;

namespace LearnAPI.Service
{
    public interface ICustomerService
    {
        List<CustomerModal> GetAll();
    }
}

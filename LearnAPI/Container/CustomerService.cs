using LearnAPI.Modal;
using LearnAPI.Repos;
using LearnAPI.Repos.Models;
using LearnAPI.Service;

namespace LearnAPI.Container
{
    public class CustomerService : ICustomerService
    {
        private readonly LearndataContext context;
        public CustomerService(LearndataContext context) 
        {
            this.context = context;
        }
        public List<CustomerModal> GetAll()
        {
            List<CustomerModal> _response = new List<CustomerModal>();
            var _data = this.context.TblCustomers.ToList();
            return _response;
        }
    }
}

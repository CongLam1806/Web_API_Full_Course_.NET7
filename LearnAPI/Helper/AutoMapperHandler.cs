using AutoMapper;
using LearnAPI.Modal;
using LearnAPI.Repos.Models;

namespace LearnAPI.Helper
{
    public class AutoMapperHandler:Profile
    {
        public AutoMapperHandler()
        {
            //CreateMap<TblCustomer, CustomerModal>().ForMember(dest => dest.Statusname, opt => opt.MapFrom(
            //    item => ((bool)item.IsActive && item.IsActive.Value) ? "Active" : "In active"));

            CreateMap<TblCustomer, CustomerModal>().ForMember(dest => dest.Statusname, opt => opt.MapFrom(item => (item.IsActive != null && (bool)item.IsActive!) ? "Active" : "In active")).ReverseMap();

            //CreateMap<TblCustomer, CustomerModal>();
        }
    }

   
}

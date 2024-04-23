﻿using AutoMapper;
using LearnAPI.Helper;
using LearnAPI.Modal;
using LearnAPI.Repos;
using LearnAPI.Repos.Models;
using LearnAPI.Service;
using Microsoft.EntityFrameworkCore;

namespace LearnAPI.Container
{
    public class CustomerService : ICustomerService
    {
        private readonly LearndataContext context;
        private readonly IMapper mapper;
        public CustomerService(LearndataContext context, IMapper mapper) 
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<APIResponse> Create(CustomerModal customer)
        {
            APIResponse response = new APIResponse();
            try
            {
                TblCustomer _customer = this.mapper.Map<CustomerModal, TblCustomer>(customer);
                await this.context.TblCustomers.AddAsync(_customer);
                await this.context.SaveChangesAsync();
                response.ResponseCode = 201;
                response.Result = customer.Code;
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<List<CustomerModal>> GetAll()
        {
            List<CustomerModal> _response = new List<CustomerModal>();
            var _data = await this.context.TblCustomers.ToListAsync();
            if(_data != null )
            {
              
                _response = this.mapper.Map<List<TblCustomer>, List<CustomerModal>>(_data);
            }
            return _response;
        }

        public async Task<CustomerModal> GetByCode(string code)
        {
            CustomerModal _response = new CustomerModal();
            var _data = await this.context.TblCustomers.FindAsync(code);
            if (_data != null)
            {
                _response = this.mapper.Map<TblCustomer, CustomerModal>(_data);
               
            }
            return _response;
        }

        public async Task<APIResponse> Remove(string code)
        {
            APIResponse response = new APIResponse();
            try
            {
                //TblCustomer _customer = this.mapper.Map<CustomerModal, TblCustomer>(customer);
                //await this.context.TblCustomers.AddAsync(_customer);
                //await this.context.SaveChangesAsync();
                var _customer = await this.context.TblCustomers.FindAsync(code);
                if(_customer != null)
                {
                    this.context.TblCustomers.Remove(_customer);
                    await this.context.SaveChangesAsync();
                    response.ResponseCode = 200;
                    response.Result = code;
                }
                else
                {
                    response.ResponseCode = 404;
                    response.ErrorMessage = "Data not found";
                }
               
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<APIResponse> Update(CustomerModal customer, string code)
        {
            APIResponse response = new APIResponse();
            try
            {
                
                var _customer = await this.context.TblCustomers.FindAsync(code);
                if (_customer != null)
                {
                    _customer.Name = customer.Name;
                    _customer.Email = customer.Email;
                    _customer.Phone = customer.Phone;
                    _customer.IsActive = customer.IsActive;
                    _customer.Creditlimit = customer.Creditlimit;
                    await this.context.SaveChangesAsync();
                    response.ResponseCode=200;
                    response.Result = code;
                    

                }
                else
                {
                    response.ResponseCode = 404;
                    response.ErrorMessage = "Data not found";
                }

            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }
    }
}

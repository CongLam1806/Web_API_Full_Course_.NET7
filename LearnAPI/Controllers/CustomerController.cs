using LearnAPI.Modal;
using LearnAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LearnAPI.Controllers
{
    [Authorize]
    //[DisableCors]
    [EnableRateLimiting("fixedwindow")]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService service;
        public CustomerController(ICustomerService service)
        {
            this.service = service;
        }


        //[EnableCors("corspolicy1")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> Get()
        {
            var data = await this.service.GetAll();
            if(data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }


        [DisableRateLimiting]
        [HttpGet("GetByCode")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var data = await this.service.GetByCode(code);
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(CustomerModal customerDTO)
        {
            var data = await this.service.Create(customerDTO);
            return Ok(data);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update(CustomerModal customerDTO, string code)
        {
            var data = await this.service.Update(customerDTO, code);
            return Ok(data);
        }

        [HttpDelete("Remove")]
        public async Task<IActionResult> Remove(string code)
        {
            var data = await this.service.Remove(code);
            return Ok(data);
        }
    }
}

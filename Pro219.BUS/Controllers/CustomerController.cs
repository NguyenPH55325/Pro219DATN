using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pro219.API.DTOs;
using Pro219.DAL.Models;
using Pro219.DAL.Repository;
using static Pro219.DAL.Repository.ProductRepository;
using System.Security.Claims;

namespace Pro219.API.Controllers
{
    [Route("customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        CustomerRepository _customerRepository;
        AddressRepository _addressRepository;

        public CustomerController()
        {
            _customerRepository = new CustomerRepository();
            _addressRepository = new AddressRepository();
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<List<Customer>>> GetAllCustomers()
        {
            try
            {
                var result = await _customerRepository.GetAllCustomers();
                if (result == null || result.Count() == 0)
                {
                    return Ok(new List<Customer>());
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpGet("get-by-id/{id}")]
        public async Task<ActionResult<Customer>> GetCustomerById(int id)
        {
            try
            {
                var result = await _customerRepository.GetByIdCustomer(id);
                if (result == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }

        [HttpPost("add")]
        public async Task<ActionResult<Customer>> CreateCustomer([FromBody] CreateCustomerDTO cusAndAddr)
        {
            try
            {
                if (cusAndAddr == null)
                {
                    return BadRequest();
                }

                var checkDuplicate = await _customerRepository.FindCustomerByEmailAndPhone(cusAndAddr.CustomerEmail, cusAndAddr.CustomerPhone);

                if (checkDuplicate != null)
                {
                    return BadRequest("Email hoặc số điện thoại này đã được sử dụng");
                }

                Customer customer = new Customer();
                customer.FullName = cusAndAddr.CustomerName;
                customer.PhoneNumber = cusAndAddr.CustomerPhone;
                customer.DateOfBirth = cusAndAddr.DateOfBirth != null ? cusAndAddr.DateOfBirth : null;
                customer.Email = cusAndAddr.CustomerEmail;
                customer.PasswordHash = cusAndAddr.Password;

                var resultCustomer = await _customerRepository.CreateCustomer(customer);
                if (resultCustomer == null)
                {
                    return StatusCode(500, Constant.ErrorCode.DatabaseError);
                }

                Address address = new Address();
                address.CustomerId = resultCustomer.Id;
                address.FullName = cusAndAddr.CustomerAddressName;
                address.Phone = cusAndAddr.CustomerAddressPhone;
                address.City = cusAndAddr.CityId;
                address.District = cusAndAddr.DistrictId;
                address.Street = cusAndAddr.WardId;
                address.CityName = cusAndAddr.CityName;
                address.DistrictName = cusAndAddr.DistrictName;
                address.StreetName = cusAndAddr.WardName;
                address.OtherInfo = cusAndAddr.OtherAddressInfo;
                address.IsDefault = cusAndAddr.IsDefault;

                await _addressRepository.AddAddress(address);
                return Ok(resultCustomer);
            } catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpPut("update")]
        public async Task<ActionResult<Customer>> UpdateCustomer([FromBody] Customer customer)
        {
            try
            {
                if (customer == null) return BadRequest();

                Customer customerUpdate = new Customer 
                {
                    Id = customer.Id,
                    FullName = customer.FullName,
                    PhoneNumber = customer.PhoneNumber,
                    PasswordHash = customer.PasswordHash,
                    Email = customer.Email,
                    DateOfBirth = customer.DateOfBirth,
                    Status = customer.Status,
                    LastLogin = customer.LastLogin,
                    UpdateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UpdateAt = DateTime.Now,
                    Delete = customer.Delete,
                    DeleteAt = customer.Delete == true ? DateTime.Now : null
                };

                var resultUpdate = await _customerRepository.UpdateCustomer(customerUpdate);

                if (resultUpdate != null)
                {
                    return Ok(resultUpdate);
                } else
                {
                    return StatusCode(500);
                }
            } catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<Customer>> DeleteCustomer(int id)
        {
            try
            {
                var updateBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _customerRepository.DeleteCustomer(id, updateBy);
                if (result == null)
                {
                    return NotFound(Constant.ErrorCode.DataNotFound);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, Constant.ErrorCode.OtherError);
            }
        }
    }
}

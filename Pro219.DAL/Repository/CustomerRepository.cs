using Microsoft.EntityFrameworkCore;
using Pro219.DAL.Context;
using Pro219.DAL.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pro219.DAL.Repository
{
    public class CustomerRepository
    {
        ClothesDbContext _context;
        public CustomerRepository()
        {
            _context = new ClothesDbContext();
        }

        public async Task<Customer> GetByKeyAndPassword(string keyword, string hashPassword)
        {
            Customer avaiableUser = await _context.Customers.FirstOrDefaultAsync(x => (x.Email == keyword || x.PhoneNumber == keyword) && x.Delete != true);
            if (avaiableUser == null)
            {
                return null;
            }
            else
            {
                if (avaiableUser.PasswordHash.ToUpper() == hashPassword.ToUpper())
                {
                    return avaiableUser;
                }

                else
                {
                    return null;
                }
            }
        }

        public async Task<List<Customer>> GetAllCustomers()
        {
            return await _context.Customers.Where(x => x.Delete != true).ToListAsync();
        }
        public async Task<Customer> GetByIdCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null && customer.Delete == true)
                return null;
            return customer;
        }

        public async Task<Customer> GetByIdCustomerSendMail(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<Customer> AddCustomer(Customer customer)
        {
            try
            {
                var addedCustomer = _context.Customers.Add(customer).Entity;
                await _context.SaveChangesAsync();
                return addedCustomer;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Customer> CreateCustomer(Customer customer)
        {
            try
            {
                customer.CreateAt = DateTime.Now;
                customer.Status = 1;
                customer.Delete = false;
                var addedCustomer = _context.Customers.Add(customer).Entity;
                await _context.SaveChangesAsync();
                return addedCustomer;
            } catch (Exception)
            {
                return null;
            }
        }

        public async Task<Customer> UpdateCustomer(Customer customer)
        {
            try
            {
                var existingCustomer = await _context.Customers.FindAsync(customer.Id);

                if (existingCustomer == null || existingCustomer.Delete == true) return null;

                existingCustomer.FullName = customer.FullName;
                existingCustomer.DateOfBirth = customer.DateOfBirth;
                existingCustomer.PhoneNumber = customer.PhoneNumber;
                existingCustomer.Email = customer.Email;
                existingCustomer.PasswordHash = customer.PasswordHash;
                existingCustomer.Status = customer.Status;
                existingCustomer.LastLogin = customer.LastLogin;
                existingCustomer.UpdateAt = DateTime.Now;
                if (!string.IsNullOrEmpty(customer.UpdateBy))
                {
                    existingCustomer.UpdateBy = customer.UpdateBy;
                }

                var updatedCustomer = _context.Customers.Update(existingCustomer).Entity;
                await _context.SaveChangesAsync();
                return updatedCustomer;
            }
            catch
            {
                return null;
            }
        }
        public async Task<Customer> DeleteCustomer(int id, string updateBy = null)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);

                if (customer == null) return null;

                customer.Delete = true;
                customer.DeleteAt = DateTime.Now;
                customer.UpdateAt = DateTime.Now;
                if (!string.IsNullOrEmpty(updateBy))
                {
                    customer.UpdateBy = updateBy;
                }

                var updatedCustomer = _context.Customers.Update(customer).Entity;
                await _context.SaveChangesAsync();
                return updatedCustomer;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Customer> FindCustomerExistByKeyWord(string key)
        {
            try
            {
                var c = _context.Customers.FirstOrDefault(x => (x.Email == key || x.PhoneNumber == key) && x.Delete != true);

                if (c == null)
                    return null;
                return c;


            }
            catch
            {
                return null;
            }
        }

        public async Task<Customer> FindCustomerByEmailAndPhone(string email, string phoneNumber)
        {
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(x => (x.Email == email || x.PhoneNumber == phoneNumber) && x.Delete != true);
                return customer;
            }
            catch
            {
                return null;
            }
        }


    }
}

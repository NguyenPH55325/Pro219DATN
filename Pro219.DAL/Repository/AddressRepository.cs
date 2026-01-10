using Microsoft.EntityFrameworkCore;
using Pro219.DAL.Context;
using Pro219.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pro219.DAL.Repository
{
    public class AddressRepository
    {
        ClothesDbContext _context;

        public AddressRepository()
        {
            _context = new ClothesDbContext();
        }

        public async Task<Address> AddAddress(Address address)
        {
            try
            {
                if (address.IsDefault)
                {
                    var allAddressByCustomer = _context.Addresses.Where(a => a.CustomerId == address.CustomerId).ToList();

                    if (allAddressByCustomer != null && allAddressByCustomer.Any())
                    {
                        var hasAddressIsDefault = allAddressByCustomer.FirstOrDefault(a => a.IsDefault == true);

                        if (hasAddressIsDefault != null)
                        {
                            hasAddressIsDefault.IsDefault = false;

                            var obj = _context.Addresses.Update(hasAddressIsDefault).Entity;
                        }
                    }
                } else
                {
                    var allAddressByCustomer = _context.Addresses.Where(a => a.CustomerId == address.CustomerId).ToList();

                    if (allAddressByCustomer == null || (allAddressByCustomer != null && !allAddressByCustomer.Any())) 
                    { 
                        address.IsDefault = true;
                    }
                }

                var addedAddress = _context.Addresses.Add(address).Entity;
                await _context.SaveChangesAsync();
                return addedAddress;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Address> UpdateAddress(Address address)
        {
            try
            {
                var existingAddress = await _context.Addresses.FindAsync(address.Id);

                if (existingAddress == null || existingAddress.Delete == true) return null;

                existingAddress.CustomerId = address.CustomerId;
                existingAddress.FullName = address.FullName;
                existingAddress.Phone = address.Phone;
                existingAddress.Ward = address.Ward;
                existingAddress.Province = address.Province;
                existingAddress.District = address.District;
                existingAddress.DistrictName = address.DistrictName;
                existingAddress.ProvinceName = address.ProvinceName;
                existingAddress.WardName = address.WardName;
                existingAddress.OtherInfo = address.OtherInfo;
                existingAddress.IsDefault = address.IsDefault;
                existingAddress.Status = address.Status;
                existingAddress.UpdateAt = DateTime.Now;
                if (!string.IsNullOrEmpty(address.UpdateBy))
                {
                    existingAddress.UpdateBy = address.UpdateBy;
                }

                if (address.IsDefault)
                {
                    var allAddressByCustomer = _context.Addresses.Where(a => a.CustomerId == address.CustomerId).ToList();

                    if (allAddressByCustomer != null && allAddressByCustomer.Any())
                    {
                        var hasAddressIsDefault = allAddressByCustomer.FirstOrDefault(a => a.IsDefault == true);

                        if (hasAddressIsDefault != null)
                        {
                            hasAddressIsDefault.IsDefault = false;

                            var obj = _context.Addresses.Update(hasAddressIsDefault).Entity;
                        }
                    }
                }
                else
                {
                    var allAddressByCustomer = _context.Addresses.Where(a => a.CustomerId == address.CustomerId).ToList();

                    if (allAddressByCustomer == null || (allAddressByCustomer != null && !allAddressByCustomer.Any()))
                    {
                        existingAddress.IsDefault = true;
                    } else
                    {
                        var hasDefault = allAddressByCustomer.Any(x => x.IsDefault);

                        if(!hasDefault)
                        {
                            existingAddress.IsDefault = true;
                        }
                    }
                }


                var updatedAddress = _context.Addresses.Update(existingAddress).Entity;
                await _context.SaveChangesAsync();
                return updatedAddress;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Address>> GetByCustomerId(int customerId)
        {
            try
            {
                var addresses = await _context.Addresses
                    .Where(x => x.CustomerId == customerId && x.Delete != true)
                    .ToListAsync();
                return addresses;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Address> GetById(int id)
        {
            try
            {
                var address = await _context.Addresses.FindAsync(id);
                if (address != null && address.Delete == true)
                    return null;
                return address;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Address>> GetAllAddresses()
        {
            try
            {
                var addresses = await _context.Addresses
                    .Where(x => x.Delete != true)
                    .ToListAsync();
                return addresses;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Address> DeleteAddress(int id, string updateBy = null)
        {
            try
            {
                var address = await _context.Addresses.FindAsync(id);

                if (address == null) return null;

                if(address.IsDefault)
                {
                    var addresses = await _context.Addresses.Where(x => x.CustomerId == address.CustomerId).ToListAsync();

                    if (addresses != null && addresses.Any())
                    {
                        var addressNewDefault = addresses.OrderByDescending(x => x.CreateAt).FirstOrDefault();

                        if (addressNewDefault != null) {
                            addressNewDefault.IsDefault = true;
                            _context.Addresses.Update(address);
                        }
                    }
                }

                address.Delete = true;
                address.DeleteAt = DateTime.Now;
                address.UpdateAt = DateTime.Now;
                if (!string.IsNullOrEmpty(updateBy))
                {
                    address.UpdateBy = updateBy;
                }

                var updatedAddress = _context.Addresses.Update(address).Entity;
                await _context.SaveChangesAsync();
                return updatedAddress;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

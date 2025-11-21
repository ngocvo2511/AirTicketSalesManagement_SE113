using AirTicketSalesManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.Customer
{
    public interface ICustomerService
    {
        Task<List<Khachhang>> GetAllAsync();
        Task<Khachhang?> GetByIdAsync(int maKh);
        Task UpdateAsync(Khachhang customer);
    }
}

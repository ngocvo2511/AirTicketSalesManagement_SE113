using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.Customer
{
    [ExcludeFromCodeCoverage]
    public class CustomerService : ICustomerService
    {
        public async Task<List<Khachhang>> GetAllAsync()
        {
            await using var context = new AirTicketDbContext();
            return await context.Khachhangs.ToListAsync();
        }

        public async Task<Khachhang?> GetByIdAsync(int maKh)
        {
            await using var context = new AirTicketDbContext();
            return await context.Khachhangs.FindAsync(maKh);
        }

        public async Task UpdateAsync(Khachhang customer)
        {
            await using var context = new AirTicketDbContext();
            context.Khachhangs.Update(customer);
            await context.SaveChangesAsync();
        }
    }
}

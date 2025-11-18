using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.Services.EmailServices
{
    public interface IEmailTemplateService
    {
        string BuildBookingCash(string soHieuCB, DateTime departureTime, DateTime time, decimal price);
        string BuildBookingSuccess(string soHieuCB, DateTime departureTime, DateTime time, decimal price);
        string BuildBookingCancel(string soHieuCB, DateTime departureTime, DateTime time);
        string BuildForgotPasswordOtp(string otp);
        string BuildRegisterOtp(string otp);
    }
}

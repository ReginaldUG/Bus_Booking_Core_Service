using System.Net.Mail;
using BusBooking.Models.DTO.RequestDTOs;

using System.Net;
using System.Net.Mail;
using BusBooking.Core.Constants;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Queries.Interfaces;
using BusBooking.Models.DTO;
using BusBooking.Models.Entities;

namespace BusBookingAPI.Helpers;

public class EmailHelper
{
    private readonly GeneralHelpers _generalHelpers;
    private readonly AuthenticationHelper _authHelper;
    private readonly IQueryRepository<EmailVerify> _verifyQueryRepository;
    private readonly ICommandRepository<EmailVerify> _verifyCommandRepository;

    public EmailHelper(GeneralHelpers generalHelpers, AuthenticationHelper authHelper,
        IQueryRepository<EmailVerify> verifyQueryRepository, ICommandRepository<EmailVerify> verifyCommandRepository)
    {
        _generalHelpers = generalHelpers;
        _authHelper = authHelper;
        _verifyCommandRepository = verifyCommandRepository;
        _verifyQueryRepository = verifyQueryRepository;
    }

    public async Task<ApiResponse> SendOtp (SendOtpRequestDTO request)
    {
        try
        {
            //check if there is existing code for email
            var existing = await _verifyQueryRepository.FindByCriteriaAsync(nameof(EmailVerify.Email), request.EmailAddress);
            if(existing != null)
            {
                await _verifyCommandRepository.DeleteAsync(existing, nameof(EmailVerify.Email), request.EmailAddress);                
            }

            //Generate Code
            var generator = _generalHelpers.TokenGenerator();
            if (!generator.Status)
            {
                return ApiResponse.Failure("Error Generating code");
            }
            int code = generator.Data;

            //Store
            var codeHash = _authHelper.HashPassword(code.ToString());
            var codeStore = new EmailVerify
            {
                Email = request.EmailAddress,
                CodeHash = codeHash.Data,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            };
            await _verifyCommandRepository.AddAsync(codeStore);

            //SEND EMAIL
            MailMessage email = new MailMessage
            {
                From = new MailAddress(SMTP.mail, "BUS BOOKING SERVICE")
            };
            email.To.Add(request.EmailAddress);
            email.Subject = "Your Email Verification Code";
            email.Body = $"<h3>Hello {request.Name},</h3><p>Your verification code is: <b>{code}</b></p><p>This code expires in 5 minutes.</p>";
            email.IsBodyHtml = true;

            // Configure SMTP Client (Example using Gmail's server)
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(SMTP.mail, SMTP.password),
                EnableSsl = true
            };

            smtpClient.Send(email);
            Console.WriteLine("Verification email sent successfully.");

            return ApiResponse.Success("Verification email sent successfully");
        }
        catch (Exception e)
        {
            return ApiResponse.Failure(e.Message);
        }        
    }

    public async Task<ApiResponse> VerifyOtp (VerifyOtpRequestDTO request)
    {
        try
        {
            //check code and email match a db entry
            var entry = await _verifyQueryRepository.FindByCriteriaAsync(nameof(EmailVerify.Email), request.Email);
            if (entry == null)
                return ApiResponse.Failure("Invalid Verification Code");

            var codeCheck = _authHelper.VerifyPassword(request.Code.ToString(), entry.CodeHash);
            if (!codeCheck.Status)
                return ApiResponse.Failure("Email Verification Failed");
            if (entry.ExpiresAt < DateTime.UtcNow)
                return ApiResponse.Failure("Email token has expired");
            
            await _verifyCommandRepository.DeleteAsync(entry, nameof(EmailVerify.Email), request.Email);            

            //verify or fail
            return ApiResponse.Success("Email Verified");
        }
        catch (Exception e)
        {
            return ApiResponse.Failure(e.Message);
        }
        
    }
}
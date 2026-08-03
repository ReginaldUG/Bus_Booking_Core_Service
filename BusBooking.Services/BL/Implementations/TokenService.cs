using BusBooking.Core.Constants;
using BusBooking.Data.Commands.Interfaces;
using BusBooking.Data.Queries.Interfaces;
using BusBooking.Models.DTO;
using BusBooking.Models.DTO.RequestDTOs;
using BusBooking.Models.DTO.ResponseDTOs;
using BusBooking.Models.Entities;
using BusBooking.Services.BL.Interfaces;
using BusBooking.Services.Helpers;

namespace BusBooking.Services.BL.Implementations;

public class TokenService : ITokenService
{
    private readonly IQueryRepository<Token> _tokenQueryRepository;
    private readonly IQueryRepository<Customer> _customerQueryRepository;
    private readonly ICommandRepository<Token> _tokenCommandRepository;
    private readonly GeneralHelpers _generalHelpers;
    private readonly AuthenticationHelper _authHelper;

    public TokenService(IQueryRepository<Token> tokenQueryRepository, IQueryRepository<Customer> customerQueryRepository,
        ICommandRepository<Token> tokenCommandRepository, AuthenticationHelper authHelper,  GeneralHelpers generalHelpers)
    {
        _tokenQueryRepository = tokenQueryRepository;
        _tokenCommandRepository = tokenCommandRepository;
        _customerQueryRepository = customerQueryRepository;
        _generalHelpers = generalHelpers;
        _authHelper = authHelper;
    }

    //create token
    public async Task<ApiResponse<CreateAccessTokenResponseDTO>> CreateAssignAccessToken (int customerId)
    {
        try
        {
            //Introduce a random salt value that will be appended to the end of every issued token with a dot(.)
            //That will be stored in a seperate column as well, for verification we extract that salt value and 
            //check against db, to get first check, then ensure the token on the db matches the token string in the request

            //check that customer id exists
            var customerExists = await _customerQueryRepository.FindByIdAsync(customerId);
            if (customerExists == null)
                return ApiResponse<CreateAccessTokenResponseDTO>.Failure(ErrorMessages.INVALID_CREDENTIALS, StatusCodes.BadRequest);           
            
            using var transaction = _tokenCommandRepository.BeginTransaction();
            bool isCommitted = false;
            try
            {
                //Begin new token creation
                var response = _generalHelpers._rand_string();
                if (!response.Status)
                    return ApiResponse<CreateAccessTokenResponseDTO>.Failure("Error generating string",
                        StatusCodes.BadRequest);
                
                string rand = response.Data.Rand;
                string salt = response.Data.Salt;
                string randHash = _authHelper.HashPassword(rand).Data;

                string token = $"{rand}.{salt}";

                var newToken = new Token
                {
                    CustomerId = customerId,
                    TokenHash = randHash,
                    Salt = salt,
                    Revoked = false,
                    ExpiresAt = DateTime.UtcNow.AddDays(1)
                };
                var created = await _tokenCommandRepository.AddWithOpenDBTransaction(newToken, transaction);

                //Revoke All old tokens
                var searchParams = new Dictionary<string, object>
                {
                    { nameof(Token.CustomerId), customerId.ToString() },
                    { nameof(Token.Revoked), false }
                };
                var customerTokens = (await _tokenQueryRepository.FindByMultipleFieldsAsync(searchParams, null)).ToList();

                if(customerTokens.Count > 0)
                {
                    foreach (var t in customerTokens)
                    {
                        t.ExpiresAt = DateTime.UtcNow;
                        t.Revoked = true;
                        await _tokenCommandRepository.UpdateWithOpenDbTransactionAsync(t, transaction);
                    }
                }
                _tokenCommandRepository.CommitTransaction(transaction);
                isCommitted = true;

                return ApiResponse<CreateAccessTokenResponseDTO>.Success("Token Generated",
                    new CreateAccessTokenResponseDTO
                    {
                        Token = token
                    });
            }
            catch (Exception)
            {
                if(!isCommitted)
                    _tokenCommandRepository.RollbackTransaction(transaction);
                throw;
            }
        }
        catch (Exception e)
        {
            return ApiResponse<CreateAccessTokenResponseDTO>.Failure(e.Message, StatusCodes.ServerError);
        }
    }

    //verify token public
    public async Task<ApiResponse<VerifyAccessTokenResponseDTO>> VerifyToken(string token)
    {
        
        var verify = await VerifyAccessToken(token);
        
        if (!verify.Status)
            return ApiResponse<VerifyAccessTokenResponseDTO>.Failure(ErrorMessages.INVALID_TOKEN,
                StatusCodes.BadRequest);
        
        int customerId = verify.Data;
        return ApiResponse<VerifyAccessTokenResponseDTO>.Success("Token verified", new VerifyAccessTokenResponseDTO
        {
            CustomerId = customerId
        });
    }
    
    //verify token logic
    private async Task<ApiResponse<int>> VerifyAccessToken (string requestToken)
    {
        try
        {
            //split token in salt and main token
            string[] parts = requestToken.Split('.');
            if (parts.Length != 2)
            {
                return ApiResponse<int>.Failure(ErrorMessages.INVALID_TOKEN, StatusCodes.BadRequest);
            }

            string token = parts[0];
            string salt = parts[1];

            //verify salt exist with record
            var checkMatch = await _tokenQueryRepository.FindByCriteriaAsync(nameof(Token.Salt), salt);
            if (checkMatch == null)
                return ApiResponse<int>.Failure(ErrorMessages.INVALID_TOKEN, StatusCodes.BadRequest);

            //verify token matches salt entry
            var verify = _authHelper.VerifyPassword(token, checkMatch.TokenHash);
            if(!verify.Status)
                return ApiResponse<int>.Failure(ErrorMessages.INVALID_TOKEN, StatusCodes.BadRequest);
            
            //verify it satisfies other conditions
            bool failed = checkMatch.ExpiresAt < DateTime.UtcNow.AddMinutes(1) || checkMatch.Revoked;
            if (failed)
                return ApiResponse<int>.Failure(ErrorMessages.INVALID_TOKEN, StatusCodes.BadRequest);
            
            //check that customer account is valid
            var customerCheck = await _customerQueryRepository.FindByIdAsync(checkMatch.CustomerId);
            if (customerCheck == null)
                return ApiResponse<int>.Failure(ErrorMessages.INVALID_TOKEN, StatusCodes.BadRequest);
            if (customerCheck.Status != CustomerAccountStatus.Active)
                return ApiResponse<int>.Failure("Customer Account is not Active", StatusCodes.BadRequest);


            return ApiResponse<int>.Success("Token Verified", checkMatch.CustomerId);
        }
        catch (Exception e)
        {
            return ApiResponse<int>.Failure(e.Message, StatusCodes.ServerError);
        }
    }
}
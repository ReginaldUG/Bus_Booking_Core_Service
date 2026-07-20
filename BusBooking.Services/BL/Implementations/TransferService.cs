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

public class TransferService : ITransferService
{
    //mock replica of topping up customer wallet with money
    private readonly IQueryRepository<Customer> _customerQueryRepository;
    private readonly IQueryRepository<CustomerWallet> _walletQueryRepository;
    private readonly ICommandRepository<CustomerWallet> _walletCommandRepository;
    private readonly ICommandRepository<CustomerWalletTransactions> _txWalletCommandRepository;
    private readonly AuthenticationHelper _authHelper;
    private readonly ITokenService _tokenService;

    public TransferService (
        IQueryRepository<Customer> customerQueryRepository,  ITokenService tokenService,
        AuthenticationHelper authHelper,
        IQueryRepository<CustomerWallet> walletQueryRepository, 
        ICommandRepository<CustomerWallet> waleltCommandRepository, 
        ICommandRepository<CustomerWalletTransactions> txWalletCommandRepository)
    {
        _customerQueryRepository = customerQueryRepository;
        _walletQueryRepository = walletQueryRepository;
        _walletCommandRepository = waleltCommandRepository;
        _txWalletCommandRepository = txWalletCommandRepository;
        _authHelper = authHelper;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse> CustomerWalletTopUp(CustomerWalletTopUpRequestDTO request)
    {
        try
        {
            var requestDto = new VerifyAccessTokenRequestDTO { Token = request.Token };
            var verify = await _tokenService.VerifyAccessToken(requestDto);
            if (!verify.Status)
                return ApiResponse<EditCustomerDetailsResponseDTO>.Failure(ErrorMessages.INVALID_TOKEN,
                    StatusCodes.BadRequest);
            int customerId = verify.Data.CustomerId;

            //validate customer exist
            var customer = await _customerQueryRepository.FindByIdAsync(customerId);
            if (customer == null)
                return ApiResponse.Failure(ErrorMessages.INVALID_CREDENTIALS);
            
            //validate password entered is correct
            var validate = _authHelper.VerifyPassword(request.Password, customer.HashedPassword);
            if (!validate.Status)
                return ApiResponse.Failure(ErrorMessages.INVALID_CREDENTIALS);
            
            //validate the amount to be topped up
            if (request.Amount < 1000)
                return ApiResponse.Failure("Minimum amount to fund is NGN 1000");

            using var transaction = _walletCommandRepository.BeginTransaction();
            bool isCommitted = false;
            try
            {
                //update customer wallet
                var wallet = await _walletQueryRepository.FindByCriteriaAsync(nameof(CustomerWallet.CustomerId), customerId.ToString());
                if (wallet == null)
                    return ApiResponse.Failure("No wallet assigned to Customer");

                //update customer wallet tx
                var tx = new CustomerWalletTransactions
                {
                    CustomerWalletId = wallet.Id,
                    Type = TransactionType.Credit,
                    Amount = request.Amount,
                    Narration = "Wallet Funded"
                };

                wallet.Balance += request.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;

                await _walletCommandRepository.UpdateWithOpenDbTransactionAsync(wallet, transaction);                
                var txInsert = await _txWalletCommandRepository.AddWithOpenDBTransaction(tx, transaction);

                //save
                _walletCommandRepository.CommitTransaction(transaction);
                isCommitted = true;

                return ApiResponse.Success($"Wallet funded with NGN{request.Amount}");
            }
            catch (Exception)
            {
                if(!isCommitted)
                    _walletCommandRepository.RollbackTransaction(transaction);
                throw;
            }
        }
        catch (Exception e)
        {
            return ApiResponse.Failure(e.Message);
        }
    }

    public async Task<ApiResponse<CheckWalletBalanceResponseDTO>> CheckWalletBalance (CheckWalletBalanceRequestDTO request)
    {
        try
        {
            var requestDto = new VerifyAccessTokenRequestDTO { Token = request.Token };
            var verify = await _tokenService.VerifyAccessToken(requestDto);
            if (!verify.Status)
                return ApiResponse<CheckWalletBalanceResponseDTO>.Failure(ErrorMessages.INVALID_TOKEN,
                    StatusCodes.BadRequest);
            int customerId = verify.Data.CustomerId;

            //get customer data
            var customerWallet = await _walletQueryRepository.FindByCriteriaAsync(nameof(CustomerWallet.CustomerId), customerId.ToString());
            if (customerWallet == null)
                return ApiResponse<CheckWalletBalanceResponseDTO>.Failure("Invalid Customer Id",
                    StatusCodes.BadRequest);

            return ApiResponse<CheckWalletBalanceResponseDTO>.Success("Customer Wallet Balance retrieved",
                new CheckWalletBalanceResponseDTO { Balance = customerWallet.Balance });
        }
        catch (Exception e)
        {
            return ApiResponse<CheckWalletBalanceResponseDTO>.Failure(e.Message, StatusCodes.ServerError);
        }
    }

}
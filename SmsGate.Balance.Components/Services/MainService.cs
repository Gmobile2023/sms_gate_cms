using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsGate.Balance.Domain.Services;
using SmsGate.Balance.Models.Requests;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Web;
using SmsGate.Shared.Common;
using SmsGate.Shared.Contract.Grpc.Balance;

namespace SmsGate.Balance.Components.Services;

public class MainService : Service
{
    private readonly IBalanceService _balanceService;
    private readonly ILogger<MainService> _logger;

    public MainService(IBalanceService balanceService,
        ILogger<MainService> logger)
    {
        _balanceService = balanceService;
        _logger = logger;
    }

    public async Task<object> PostAsync(BalanceTransferRequest transferRequest)
    {
        try
        {
            _logger.LogInformation("Received TransferRequest: " + transferRequest.ToJson());
            var rs = await _balanceService.TransferAsync(transferRequest);
            _logger.LogInformation("TransferRequest return: " + rs.ToJson());
            return new ResponseMessageBase<BalanceResponse>()
            {
                ResponseStatus = rs.ResponseStatus,
                Results = rs.Results.ConvertTo<BalanceResponse>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"TransferRequest error: {ex}");
            return ResponseMessageBase<BalanceResponse>.Error();
        }
    }
    public async Task<object> PostAsync(CurrencyExchangeRequest exchangeRequest)
    {
        try
        {
            _logger.LogInformation("Received CurrencyExchangeRequest: " + exchangeRequest.ToJson());
            var rs = await _balanceService.CurrencyExchangeAsync(exchangeRequest);
            _logger.LogInformation("CurrencyExchangeRequest return: " + rs.ToJson());
            return new ResponseMessageBase<BalanceResponse>()
            {
                ResponseStatus = rs.ResponseStatus,
                Results = rs.Results.ConvertTo<BalanceResponse>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"TransferRequest error: {ex}");
            return ResponseMessageBase<BalanceResponse>.Error();
        }
    }

    public async Task<object> PostAsync(BalanceDepositRequest depositRequest)
    {
        try
        {
            _logger.LogInformation("Received DepositRequest: " + depositRequest.ToJson());
            var rs = await _balanceService.DepositAsync(depositRequest);
            _logger.LogInformation("DepositRequest return: " + rs.ToJson());
            return new ResponseMessageBase<BalanceResponse>()
            {
                ResponseStatus = rs.ResponseStatus,
                Results = rs.Results.ConvertTo<BalanceResponse>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"DepositRequest error: {ex}");
            return ResponseMessageBase<BalanceResponse>.Error();
        }
    }

    public async Task<object> PostAsync(CashOutRequest cashOutRequest)
    {
        try
        {
            _logger.LogInformation("Received CashOutRequest: " + cashOutRequest.ToJson());
            var rs = await _balanceService.CashOutAsync(cashOutRequest);
            _logger.LogInformation("CashOutRequest return: " + rs.ToJson());
            return new ResponseMessageBase<BalanceResponse>()
            {
                ResponseStatus = rs.ResponseStatus,
                Results = rs.Results.ConvertTo<BalanceResponse>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"CashOutRequest error: {ex}");
            return ResponseMessageBase<BalanceResponse>.Error();
        }
    }
    public async Task<ResponseMessageBase<BalanceResponse>> PostAsync(BalancePaymentRequest paymentRequest)
    {
        _logger.LogInformation("Received PaymentRequest: " + paymentRequest.ToJson());
        var rs = await _balanceService.PaymentAsync(paymentRequest);
        _logger.LogInformation("PaymentRequest return: " + rs.ToJson());

        return new ResponseMessageBase<BalanceResponse>()
        {
            ResponseStatus = rs.ResponseStatus,
            Results = rs.Results.ConvertTo<BalanceResponse>()
        };
    }

    //TODO: Xem lại hoàm này sau. Hiện chưa dùng đến và không nên dùng
    public async Task<object> PostAsync(BalanceRevertRequest revertRequest)
    {
        _logger.LogInformation("Received RevertRequest: " + revertRequest.ToJson());
        var rs = await _balanceService.RevertAsync(revertRequest);
        _logger.LogInformation("RevertRequest return: " + rs.ToJson());
        return new ResponseMessageBase<BalanceResponse>()
        {
            ResponseStatus = rs.ResponseStatus,
            Results = rs.Results.ConvertTo<BalanceResponse>()
        };
    }

    public async Task<ResponseMessageBase<BalanceResponse>> PostAsync(BalanceCancelPaymentRequest request)
    {
        _logger.LogInformation("BalanceCancelPaymentRequest: " + request.ToJson());
        var rs = await _balanceService.CancelPaymentAsync(request);
        _logger.LogInformation("BalanceCancelPaymentRequest: " + rs.ToJson());
        return new ResponseMessageBase<BalanceResponse>()
        {
            ResponseStatus = rs.ResponseStatus,
            Results = rs.Results.ConvertTo<BalanceResponse>()
        };
    }

    public async Task<object> PostAsync(MasterTopupRequest masterTopupRequest)
    {
        _logger.LogInformation("Received MasterTopupRequest: " + masterTopupRequest.ToJson());
        var rs = await _balanceService.MasterTopupAsync(masterTopupRequest);
        _logger.LogInformation("masterTopupRequest return: " + rs.ToJson());
        return rs;
    }
    public async Task<ResponseMessageBase<decimal>> GetAsync(AccountBalanceCheckRequest accountBalanceCheckRequest)
    {
        _logger.LogInformation("Received AccountBalanceCheckRequest: " + accountBalanceCheckRequest.ToJson());
        if (string.IsNullOrEmpty(accountBalanceCheckRequest.CurrencyCode))
            accountBalanceCheckRequest.CurrencyCode = CurrencyCode.VND.ToString("G");
        var rs = await _balanceService.AccountBalanceCheckAsync(accountBalanceCheckRequest);
        _logger.LogInformation("AccountBalanceCheckRequest return: " + rs.ToJson());
        return new ResponseMessageBase<decimal>
        {
            ResponseStatus = new ResStatus("00"),
            Results = rs
        };
    }
    public async Task<object> PostAsync(AdjustmentRequest request)
    {
        try
        {
            _logger.LogInformation("Received AdjustmentRequest: " + request.ToJson());
            var rs = await _balanceService.AdjustmentAsync(request);
            _logger.LogInformation("AdjustmentRequest return: " + rs.ToJson());
            return new ResponseMessageBase<BalanceResponse>()
            {
                ResponseStatus = rs.ResponseStatus,
                Results = rs.Results.ConvertTo<BalanceResponse>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"AdjustmentRequest error: {ex}");
            return ResponseMessageBase<BalanceResponse>.Error();
        }
    }
    
    public async Task<object> PostAsync(BlockBalanceRequest request)
    {
        _logger.LogInformation("Received BlockBalanceRequest: " + request.ToJson());
        var rs = await _balanceService.BlockBalanceAsync(request);
        _logger.LogInformation("BlockBalanceRequest return: " + rs.ToJson());
        return new ResponseMessageBase<BalanceResponse>()
        {
            ResponseStatus = rs.ResponseStatus,
            Results = rs.Results.ConvertTo<BalanceResponse>()
        };
    }

    public async Task<object> PostAsync(UnBlockBalanceRequest request)
    {
        _logger.LogInformation("Received UnBlockBalanceRequest: " + request.ToJson());
        var rs = await _balanceService.UnBlockBalanceAsync(request);
        _logger.LogInformation("UnBlockBalanceRequest return: " + rs.ToJson());
        return new ResponseMessageBase<BalanceResponse>()
        {
            ResponseStatus = rs.ResponseStatus,
            Results = rs.Results.ConvertTo<BalanceResponse>()
        };
    }
   
    public async Task<object> PostAsync(TransferSystemRequest transferRequest)
    {
        try
        {
            _logger.LogInformation("Received TransferSystemRequest: " + transferRequest.ToJson());
            var rs = await _balanceService.TransferSystemAsync(transferRequest);
            _logger.LogInformation("TransferSystemRequest return: " + rs.ToJson());
            return new ResponseMessageBase<BalanceResponse>()
            {
                ResponseStatus = rs.ResponseStatus,
                Results = rs.Results.ConvertTo<BalanceResponse>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"TransferSystemRequest error: {ex}");
            return ResponseMessageBase<BalanceResponse>.Error();
        }
    }

    public async Task PostAsync(TransferShardRequest request)
    {
        try
        {
            if (Enum.TryParse<CurrencyCode>(request.CurrencyCode, out var c))
                await _balanceService.TransferShardAccountToMain(request.AccountCode, c);
        }
        catch (Exception e)
        {
            _logger.LogError($"TransferShardRequest error: {e}");
        }
    }


    public async Task<object> GetAsync(BalanceHistoriesRequest request)
    {
        try
        {
            _logger.LogInformation("BalanceHistoriesRequest: " + request.ToJson());
            var rs = await _balanceService.GetSettlementListHistory(request);
            _logger.LogInformation($"BalanceHistoriesRequest return:  {rs.ResponseStatus}");
            return rs;
        }
        catch (Exception ex)
        {
            _logger.LogError($"BalanceHistoriesRequest error: {ex}");
            return ResponseMessageBase<string>.Error();
        }
    }
    public async Task<object> PostAsync(BalanceChargeFeeRequest request)
    {
        try
        {
            _logger.LogInformation("BalanceChargeFeeRequest: " + request.ToJson());
            var rs = await _balanceService.ChargeFeeAsync(request);
            _logger.LogInformation("BalanceChargeFeeRequest return: " + rs.ToJson());
            return new ResponseMessageBase<BalanceResponse>()
            {
                ResponseStatus = rs.ResponseStatus,
                Results = rs.Results.ConvertTo<BalanceResponse>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"BalanceChargeFeeRequest error: {ex}");
            return ResponseMessageBase<BalanceResponse>.Error();
        }
    }
    
    public override Task<object> OnExceptionAsync(object requestDto, Exception ex)
    {
        var error = DtoUtils.CreateErrorResponse(requestDto, ex);
        if (error is IHttpError httpError)
        {                
            var errorStatus = httpError.Response.GetResponseStatus();
            errorStatus.Meta = new Dictionary<string,string> {
                ["InnerType"] = ex.InnerException?.GetType().Name
            };
        }
        return Task.FromResult(error);
    }
}
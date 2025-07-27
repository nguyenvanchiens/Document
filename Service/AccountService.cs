using Document.ApiClient;
using Document.Share.Commons;
using Document.Share.Model;

namespace Document.Web.Service
{
    public class AccountService
    {
        private readonly AccountApiClient _accountApiClient;
        public AccountService(AccountApiClient accountApiClient)
        {
            _accountApiClient = accountApiClient;
        }

        public async Task<Result<LoginResponse>> LoginAsync(string userName, string password, bool rememberMe = false)
        {
            try
            {
                var rs = await _accountApiClient.LoginAsync(new LoginRequest() { UserName = userName, Password = password, RememerMe = rememberMe });
                return rs;
            }
            catch (Exception ex)
            {

                return Result<LoginResponse>.Failure(99, ex.Message);
            }
        }
    }
}

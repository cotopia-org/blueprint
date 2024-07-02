using blueprint.core;
using blueprint.core.CRUD;
using blueprint.modules.account.database;
using blueprint.modules.account.response;
using blueprint.modules.auth;
using blueprint.modules.drive.logic;
using blueprint.modules.drive.request;
using blueprint.modules.drive.response;
using blueprint.modules.auth.request;
using blueprint.modules.auth.response;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using srtool;
using blueprint.modules.database.logic;

namespace blueprint.modules.account
{
    public class AccountModule : Module<AccountModule>
    {
        public IMongoCollection<Account> accounts { get; private set; }
        public override async Task RunAsync()
        {
            await base.RunAsync();
            accounts = DatabaseModule.Instance.database.GetCollection<Account>("account");

        }
        public async Task<bool> IsExistEmail(string email)
        {
            return await accounts.AsQueryable().Where(i => i.email == email).AnyAsync();
        }

        public async Task Add(SignupRequest request)
        {
            var account = new Account();
            account._id = ObjectId.GenerateNewId();
            account.firstName = request.firstName;
            account.lastName = request.lastName;
            account.email = request.email;
            account.signupDateTime = DateTime.UtcNow;
            account.passwordMd5 = Utility.CalculateMD5Hash(request.password);
            await accounts.InsertOneAsync(account);
        }
        public async Task<PaginationResponse<AccountResponse>> List(Pagination pagination, string search = null, string fromAccountId = null)
        {
            var q1 = accounts.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                q1 = q1.Where(i => i.email.ToLower().Contains(search.ToLower()) || i.username.ToLower().Contains(search.ToLower()));

            var dbAccounts = await q1
             .OrderByDescending(i => i.signupDateTime)
             .Skip(pagination.Skip)
             .Take(pagination.Take).ToListAsync();

            var result = new PaginationResponse<AccountResponse>();
            result.total = 9999;
            result.page = pagination.Page;
            result.perPage = pagination.PerPage;
            result.items = await List(dbAccounts, fromAccountId);

            return result;
        }
        public async Task<List<AccountResponse>> List(List<string> ids, string fromAccountId = null)
        {
            if (ids == null)
                return new List<AccountResponse>();

            var _ids = ids.Select(i => i.ToObjectId()).Distinct().ToList();

            //var dbAccounts = await accounts.AsQueryable().Where(i => _ids.Contains(i._id)).ToListAsync();
            var dbAccounts = await accounts.Find_Cache("_id", ids.Select(i => i.ToObjectId()).ToList());
            return await List(dbAccounts, fromAccountId);
        }
        public async Task<List<AccountResponse>> List(List<Account> dbAccounts, string fromEntityObjectId = null)
        {
            var results = dbAccounts.Select(i => new
            {
                res = new AccountResponse()
                {
                    id = i._id.ToString(),
                    email = i.email,
                    firstName = i.firstName,
                    lastName = i.lastName,
                    signupDateTime = i.signupDateTime,
                },
                avatarId = i.avatar_fileId
            }).ToList();

            var medias = await DriveModule.Instance.List(results.Where(i => i.avatarId.HasValue).Select(i => i.avatarId.Value.ToString()).ToList());
            //Set avatars
            results.ForEach(acc => { acc.res.avatar = medias.FirstOrDefault(i => i.id == acc.avatarId?.ToString()); });
            return results.Select(i => i.res).ToList();
        }

        public async Task<AccountResponse> Get(string id, string fromAccountId = null)
        {
            var _id = id.ToObjectId();
            var result = await List(new List<string>() { _id.ToString() }, fromAccountId);
            return result.FirstOrDefault();
        }
        public async Task<bool> HasPermision(string accountId, string subrole)
        {
            var _accountId = accountId.ToObjectId();
            //var acc = await accounts.AsQueryable().Where(i => i._id == _accountId).Select(i => new Account() { roles = i.roles })
            //    .FirstOrDefaultAsync(new ExpertQuery() { cacheKey = _accountId });

            var acc = await accounts.Find_Cache("_id", _accountId);
            if (acc.roles == null)
                return false;
            else
                return acc.roles.Contains("super-root") || acc.roles.Contains(subrole);
        }
        public async Task CheckPermision(string accountId, string subrole)
        {
            var state = await HasPermision(accountId, subrole);

            if (!state)
            {
                var appException = new AppException(System.Net.HttpStatusCode.Forbidden);
                appException.AddHint("subrole", $"You have not {subrole} subrole permision.", new { subrole });
                throw appException;
            }
        }
        public async Task<AccountResponse> SetAvatar(string accountId, SetFileRequest request)
        {
            var _fileId = request.fileId.ToObjectId();
            var _accountId = accountId.ToObjectId();

            await accounts
                .UpdateOneAsync(Builders<Account>.Filter.Eq(i => i._id, _accountId), Builders<Account>.Update.Set(i => i.avatar_fileId, _fileId));
            //MongoCacheExtensions.Remove(_accountId);
            accounts.CacheFind_remove("_id", accountId);

            return await Get(accountId);
        }
        //public async Task<ProfileRespone> UpdateProfile(string accountId, ChangeProfileRequest request)
        //{
        //    var _accountId = accountId.ToObjectId();
        //    var currentAccount = await DBManager.Instance.accounts.AsQueryable().Where(i => i._id == _accountId).FirstOrDefaultAsync();
        //    if (currentAccount.email != request.email.ToLower())
        //    {
        //        if (await Instance.IsExistEmail(request.email.ToLower()))
        //            throw new ArgumentException("This email address already exist!");
        //    }
        //    if (currentAccount.username != request.username.ToLower())
        //    {
        //        if (await Instance.IsExistUsername(request.username.ToLower()))
        //            throw new ArgumentException("This username already exist!");
        //    }
        //    await DBManager.Instance.accounts.UpdateOneAsync(
        //    Builders<Account>.Filter.Eq(i => i._id, _accountId),
        //     Builders<Account>.Update
        //     .Set(i => i.email, request.email)
        //     .Set(i => i.username, request.username)
        //     .Set(i => i.fullname, request.fullname)
        //     .Set(i => i.avatarMediaId, request.avatarMediaId.ToObjectId())
        //     .Set(i => i.isVerifyEmail, true)
        //     .Set(i => i.jobs, request.jobs)
        //     .Set(i => i.bio, request.bio)
        //     .Set(i => i.location, request.location)
        //     );
        //    return await GetProfile(accountId);
        //}
        public async Task<ResetPasswordResponse> ResetPassword(string accountId, ResetPasswordRequest request)
        {
            var _accountId = accountId.ToObjectId();

            await accounts.UpdateOneAsync(
            Builders<Account>.Filter.Eq(i => i._id, _accountId),

             Builders<Account>.Update
             .Set(i => i.passwordMd5, Utility.CalculateMD5Hash(request.newPassowrd)));
            ResetPasswordResponse resetPasswordResponse = new ResetPasswordResponse();
            resetPasswordResponse.signouts = await AuthModule.Instance.Signout(accountId);
            //MongoCacheExtensions.Remove(_accountId);
            accounts.CacheFind_remove("_id", accountId);
            return resetPasswordResponse;
        }
        public async Task<ChangePasswordResponse> ChangePassword(string accountId, ChangePassowrdRequest request)
        {
            var _accountId = accountId.ToObjectId();

            if (request.newPassowrd == request.currentPassword)
            {
                var appE = new AppException(System.Net.HttpStatusCode.Forbidden);
                appE.AddHint("password", "The password is the same as the previous password.");
                throw appE;
            }

            await accounts.UpdateOneAsync(
            Builders<Account>.Filter.Eq(i => i._id, _accountId),

             Builders<Account>.Update
             .Set(i => i.passwordMd5, Utility.CalculateMD5Hash(request.newPassowrd)));
            var result = new ChangePasswordResponse();
            result.signouts = await AuthModule.Instance.Signout(accountId);
            //MongoCacheExtensions.Remove(_accountId);
            accounts.CacheFind_remove("_id", accountId);

            return result;
        }

        public async Task DeleteAccount(string accountId, DeleteAccountRequest request)
        {
            accounts.CacheFind_remove("_id", accountId);
        }

    }
}
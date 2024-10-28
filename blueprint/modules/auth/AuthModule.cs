using blueprint.modules.auth.request;
using blueprint.modules.auth.response;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Security.Claims;
using System.Text;
using api_server.modules.auth.database;
using srtool;
using blueprint.srtool;
using blueprint.core;
using blueprint.modules.account.database;
using blueprint.modules.account;
using blueprint.modules.config;
using blueprint.modules.database.logic;

namespace blueprint.modules.auth
{
    public class AuthModule : Module<AuthModule>
    {
        public JWTHandler JWTHandler { get; private set; }

        public IMongoCollection<SigninSession> signinSession { get; private set; }

        public override async Task RunAsync()
        {
            JWTHandler = new JWTHandler(ConfigModule.GetString("jwt.secret"));
            await base.RunAsync();
            signinSession = DatabaseModule.Instance.database.GetCollection<SigninSession>("signinSession");
        }


        //Register
        public async Task<SigninResponse> Signup(SignupRequest request)
        {
            if (await AccountModule.Instance.IsExistEmail(request.email.ToLower()))
            {
                AppException appException = new AppException(System.Net.HttpStatusCode.Conflict);
                appException.AddHint("email", $"{request.email.ToLower()} already exist!", new { request.email });
                throw appException;
            }
            await AccountModule.Instance.Add(request);
            return await Signin(new SigninRequest() { email = request.email, password = request.password, sessionName = "default-session" }, TimeSpan.FromHours(24));
        }
        public async Task<SignoutResponse> Signout(SignoutRequest request)
        {
            if (ObjectId.TryParse(request.sessionId, out var _id))
            {
                //var session = await signinSession.AsQueryable().Where(i => i._id == _id).FirstOrDefaultAsync(new ExpertQuery() { cacheKey = _id });
                var session = await signinSession.Find_Cache("_id", _id);

                await signinSession.DeleteOneAsync(Builders<SigninSession>.Filter.Eq(i => i._id, _id));
                signinSession.CacheFind_remove("_id", _id);
                signinSession.CacheFind_remove("refreshToken", _id);

                if (session != null)
                    return new SignoutResponse() { sessionName = session.sessionName };
                else
                    return null;
            }
            else
            {
                return null;
            }
        }
        public async Task<List<SignoutResponse>> Signout(string accountId, string sessionName = null)
        {
            var result = new List<SignoutResponse>();

            if (ObjectId.TryParse(accountId, out var _id))
            {
                List<SigninSession> selectItems = null;
                if (sessionName != null)
                {
                    selectItems = await signinSession.AsQueryable().Where(i => i.account_id == _id).ToListAsync();
                }
                else
                {
                    selectItems = await signinSession.AsQueryable().Where(i => i.account_id == _id && i.sessionName == sessionName).ToListAsync();
                }


                await signinSession.DeleteManyAsync(Builders<SigninSession>.Filter.Eq(i => i.account_id, _id));
                selectItems.ForEach(i =>
                {
                    // MongoCacheExtensions.Remove(i._id);
                    // MongoCacheExtensions.Remove("refreshToken_" + i.refreshToken);
                    signinSession.CacheFind_remove("_id", i._id);
                    signinSession.CacheFind_remove("refreshToken", i.refreshToken);
                }
                );

                result = selectItems.Select(i => new SignoutResponse() { sessionName = i.sessionName }).ToList();
            }
            return result;
        }
        public async Task<SigninResponse> Signin(SigninRequest request, TimeSpan expireDuration)
        {
            string sessionName = request.sessionName;

            var result = new SigninResponse();

            Account foundAccount = null;

            string md5Password = Utility.CalculateMD5Hash(request.password);

            if (Utility.IsValidEmail(request.email))
            {
                foundAccount =
                    await AccountModule.Instance.accounts.AsQueryable()
                    .Where(i =>
                    i.email == request.email.ToLower() &&
                    i.passwordMd5 == md5Password).FirstOrDefaultAsync();
            }

            if (foundAccount == null)
            {
                var appE = new AppException(System.Net.HttpStatusCode.Forbidden);
                appE.AddHint("incorrect", "Username or password is not correct!");
                throw appE;
            }

            string refreshToken = Utility.CalculateMD5Hash("A" + Guid.NewGuid().ToString()) + Utility.CalculateMD5Hash("B" + Guid.NewGuid().ToString());


            //////////// Upsert session in database
            ObjectId sessionId = ObjectId.GenerateNewId();
            var filter = Builders<SigninSession>.Filter;
            await signinSession.UpdateOneAsync(
                filter.Eq(i => i.account_id, foundAccount._id) & filter.Eq(i => i.sessionName, sessionName),

                 Builders<SigninSession>.Update
                 .Set(i => i.refreshToken, refreshToken)
                 .Set(i => i.sessionName, sessionName)
                 .Set(i => i.account_id, foundAccount._id)
                 .Set(i => i.loginDateTime, DateTime.UtcNow)
                 ,
                new UpdateOptions() { IsUpsert = true }
                 );

            var sessionData = await signinSession.AsQueryable().Where(i => i.account_id == foundAccount._id && i.sessionName == sessionName).FirstOrDefaultAsync();
            if (sessionData != null)
            {
                // MongoCacheExtensions.Remove(sessionData._id);
                signinSession.CacheFind_remove("_id", sessionData._id);
            }
            /////////////////////////////
            ///
            result.refreshToken = refreshToken;
            result.accessToken = CreateAccessTokenGenerator(foundAccount._id.ToString(), sessionData._id.ToString(), expireDuration);
            return result;
        }

        public async Task<AccessTokenResponse> GenerateAccessToken(string refreshToken, TimeSpan timeSpan)
        {
            var result = new AccessTokenResponse();

            var session = await signinSession.Find_Cache("refreshToken", refreshToken);

            if (session == null)
                return null;

            result.sessionName = session.sessionName;
            result.accessToken = CreateAccessTokenGenerator(session.account_id.ToString(), session._id.ToString(), timeSpan);
            return result;
        }

        private string CreateAccessTokenGenerator(string accountId, string sessionId, TimeSpan timeSpan)
        {
            return $"Bearer {JWTHandler.GenerateToken(accountId, sessionId, DateTime.UtcNow.Add(timeSpan))}";
        }
        private string CreateAccessTokenGenerator(string accountId, string sessionId)
        {
            return CreateAccessTokenGenerator(accountId, sessionId, TimeSpan.FromHours(24));
        }


        public async Task<SessionResponse> GetSession(string id)
        {
            var _id = ObjectId.Parse(id);

            var session = await signinSession.Find_Cache("_id", _id);

            if (session == null)
                return null;

            return new SessionResponse() { id = session._id.ToString(), refreshToken = session.refreshToken, sessionName = session.sessionName };
        }


    }
}
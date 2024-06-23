using blueprint.modules.auth;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace blueprint.core
{
    public static class BlueprintUtil
    {
        public static string GetBearerToken(this HttpContext httpContext)
        {
            if (httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                // Get the bearer token from the request headers
                string token = httpContext.Request.Headers["Authorization"];

                // Extract the token value
                if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
                {
                    string bearerToken = token.Substring("Bearer ".Length);
                    // Use the bearer token as needed
                    // ...
                    return bearerToken;
                }
                return null;
            }
            else
            {
                return null;
            }
        }
        public static async Task<string> GetAccountId(this ControllerBase controller)
        {
            string bearerToken = controller.HttpContext.GetBearerToken();
            if (bearerToken == null)
            {
                var appE = new AppException(System.Net.HttpStatusCode.Unauthorized);
                throw appE;
            }

            string sessionId = AuthModule.Instance.JWTHandler.GetClaim(bearerToken, "session_id");
            ObjectId _sessionId = ObjectId.Parse(sessionId);

            //var session = await AuthLogic.Instance.signinSession.AsQueryable()
            //    .Where(i => i._id == _sessionId).FirstOrDefaultAsync(new ExpertQuery() { cacheKey = _sessionId });
            var session = await AuthModule.Instance.signinSession.Find_Cahce("_id", _sessionId);

            if (session == null)
            {
                var appE = new AppException(System.Net.HttpStatusCode.Unauthorized);
                throw appE;
            }

            return session.account_id.ToString();
        }
        public static string GetLoginSessionId(this ControllerBase controller)
        {
            return AuthModule.Instance.JWTHandler.GetClaim(controller.HttpContext.GetBearerToken(), "session_id");
        }

    }
}

using blueprint.core;
using blueprint.modules.account;
using blueprint.modules.auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using srtool;

namespace blueprint.core
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public sealed class AuthRequire : Attribute, IAsyncActionFilter
    {
        private string subrule = null;
        public AuthRequire(string subrule) { this.subrule = subrule; }
        public AuthRequire()
        {
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            var myToken = context.HttpContext.GetBearerToken();
            if (myToken == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!AuthModule.Instance.JWTHandler.ValidateCurrentToken(myToken))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            var session = await AuthModule.Instance.GetSession(AuthModule.Instance.JWTHandler.GetClaim(myToken, "session_id"));

            if (session == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (subrule != null)
            {
                var sDb = await AuthModule.Instance.signinSession.AsQueryable().Where(i => i._id == session.id.ToObjectId()).FirstOrDefaultAsync();
                await AccountModule.Instance.CheckPermission(sDb.account_id.ToString(), subrule);
            }
            var result = await next();

        }
    }
}

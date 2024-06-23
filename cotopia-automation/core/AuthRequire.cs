using blueprint.core;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace blueprint.core
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public sealed class AuthRequire : Attribute, IAsyncActionFilter
    {
        string subrole = null;
        public AuthRequire(string subrole) { this.subrole = subrole; }
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

            //if (!AuthLogic.Instance.JWTHandler.ValidateCurrentToken(myToken))
            //{
            //    context.Result = new UnauthorizedResult();
            //    return;
            //}
            //var session = await AuthLogic.Instance.GetSession(AuthLogic.Instance.JWTHandler.GetClaim(myToken, "session_id"));

            //if (session == null)
            //{
            //    context.Result = new UnauthorizedResult();
            //    return;
            //}

            //if (subrole != null)
            //{
            //    var sDb = await AuthLogic.Instance.signinSession.AsQueryable().Where(i => i._id == session.id.ToObjectId())
            //        .FirstOrDefaultAsync(new ExpertQuery() { cacheKey = session.id });
            //    await AccountLogic.Instance.CheckPermision(sDb.account_id.ToString(), subrole);
            //}
            var result = await next();

        }
    }
}

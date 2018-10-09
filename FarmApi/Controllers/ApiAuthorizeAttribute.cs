using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System.Web.Http.Controllers;
using FarmApi.Model;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Text;
using System.Web;

namespace FarmApi.Controllers
{
    /// <summary>
    /// 权限拦截器 解析token
    /// </summary>
    public class ApiAuthorizeAttribute : AuthorizeAttribute
    {

        /// <summary>
        ///  权限验证 解析token 是否授权
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var authHeader = from t in actionContext.Request.Headers where t.Key == "auth" select t.Value.FirstOrDefault();
            if (authHeader != null)
            {
                string token = authHeader.FirstOrDefault();
                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        string sectet = System.Configuration.ConfigurationManager.AppSettings["TokenKey"];
                        //secret需要加密
                        IJsonSerializer serializer = new JsonNetSerializer();
                        IDateTimeProvider provider = new UtcDateTimeProvider();
                        IJwtValidator validator = new JwtValidator(serializer, provider);
                        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                        IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);

                        var json = decoder.DecodeToObject<AuthInfo>(token, sectet, verify: true);
                        if (json != null)
                        {
                            actionContext.RequestContext.RouteData.Values.Add("auth", json);
                            return true;
                        }
                        //自定义错误信息 返回错误json
                        HandleUnauthorizedRequest(actionContext);
                        return false;

                    }
                    catch (Exception ex)
                    {
                        HandleUnauthorizedRequest(actionContext);
                        return false;
                    }
                }
            }
            HandleUnauthorizedRequest(actionContext);
            return false;
        }

        //另一种重写验证权限的方法 重点是重写未授权返回的错误信息
        //public override void OnAuthorization(HttpActionContext actionContext)
        //{

        //    var content = actionContext.Request.Properties["MS_HttpContext"] as HttpContextBase;
        //    var authHeader = content.Request.Headers["auth"];

        //    //var authHeader = from t in actionContext.Request.Headers where t.Key == "auth" select t.Value.FirstOrDefault();
        //    //authHeader头不为空 判断token是否有效
        //    if (authHeader != null)
        //    {
        //        string token = authHeader;
        //        if (ValidateAuth(token))
        //        {
        //            actionContext.RequestContext.RouteData.Values.Add("auth", token);
        //            base.OnAuthorization(actionContext);
        //        }
        //        else
        //        {
        //            HandleUnauthorizedRequest(actionContext);
        //        }
        //    }
        //    //else
        //    //{
        //    //    HandleUnauthorizedRequest(actionContext);
        //    //}



        //}

        //public bool ValidateAuth(string token)
        //{
        //    string sectet = System.Configuration.ConfigurationManager.AppSettings["TokenKey"];
        //    //secret需要加密
        //    IJsonSerializer serializer = new JsonNetSerializer();
        //    IDateTimeProvider provider = new UtcDateTimeProvider();
        //    IJwtValidator validator = new JwtValidator(serializer, provider);
        //    IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
        //    IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);

        //    var json = decoder.DecodeToObject<AuthInfo>(token, sectet, verify: true);

        //    if (json != null)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //}

        /// <summary>
        /// 未授权返回错误信息 
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(HttpActionContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
            var response = filterContext.Response = filterContext.Response ?? new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.Forbidden;
            AuthResponse AuthRes = new AuthResponse();
            AuthRes.Code = "401";
            AuthRes.Message = "用户未授权，请重新登录";
            AuthRes.NowTime = DateTime.Now;
            AuthRes.data = "";
            AuthRes.IsSuccess = false;
            //解决序列化json时间带T
            IsoDateTimeConverter timejson = new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss"
            };
            string json = JsonConvert.SerializeObject(AuthRes, timejson);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

    }
}

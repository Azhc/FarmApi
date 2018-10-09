using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using System.Text;
using System;
using System.Net.Http;
using Newtonsoft.Json;
using FarmDomain.Entities;
using FarmDomain.Service;
using FarmApi.Model;
//生成token所需库
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
//序列化 反序列化所需库
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace FarmApi.Controllers

{

    public class LoginController : ApiController

    {
        GetSmsReturnService gsSer = new GetSmsReturnService();
        LoginLogService logSer = new LoginLogService();
        UserInfoService userSer = new UserInfoService();
        /// <summary>
        /// 返回用户信息所需类
        /// </summary>
        public class retUserAuth
        {
            public int IsUser { get; set; }
            public string userAuth { get; set; }
        }

        [HttpPost]
        /// <summary>
        /// 根据手机号获取验证码
        /// </summary>
        public IHttpActionResult GetSms([FromBody]GetSmsReturn ret)
        {
            if (!string.IsNullOrEmpty(ret.getSmsTel))
            {
                GetSmsReturn retu = new GetSmsReturn();
                //生成随机数
                Random rad = new Random();
                int ranValue = rad.Next(100000, 999999);
                //访问url
                string url = "https://api.miaodiyun.com/20150822/industrySMS/sendSMS";
                //调用接口开发者id
                string accountSid = ConfigurationManager.ConnectionStrings["SmsAccountSid"].ToString();
                string SmsToken = ConfigurationManager.ConnectionStrings["SmsToken"].ToString();
                //短信内容
                string content = "您的验证码为" + ranValue + "，如非本人操作，请忽略此短信。【中新一路农场】";
                //时间戳
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                //生成md5
                string md5str = accountSid + SmsToken + timestamp;
                string sig = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(md5str, "MD5").ToLower();
                string postdata = "&accountSid=" + accountSid + "&smsContent=" + content + "&to=" + ret.getSmsTel + "&timestamp=" + timestamp + "&sig=" + sig + "&respDataType=JSON";
                //获取到接口返回的json数据
                string getJson = BaseController.HttpUploadData(url, postdata);
                //反序列化json实例化为对象
                retu = JsonConvert.DeserializeObject<GetSmsReturn>(getJson);
                retu.getSmsTel = ret.getSmsTel;
                retu.getSmsDate = DateTime.Now;
                retu.VerCode = ranValue.ToString();
                //添加调用接口信息到数据库
                gsSer.Add(retu);

                //调用成功返回码为00000 判断是否调用成功根据手机号和短信ID生成token 
                if (retu.respCode == "00000")
                {
                    AuthInfo info = new AuthInfo();
                    info.UserTel = retu.getSmsTel;
                    info.VerSmsID = retu.smsId;
                    info.ValidTime = DateTime.Now;
                    //生成token
                    retu.auth = GetToken(info);
                }
                AuthResponse retResponse = new AuthResponse();
                retResponse.Code = "200";
                retResponse.Message = "调用接口成功";
                retResponse.NowTime = DateTime.Now;
                retResponse.IsSuccess = true;
                //返回数据信息不返回发送的验证码
                retu.VerCode = " ";
                retResponse.data = JsonConvert.SerializeObject(retu);
                //返回json数据
                return Ok(retResponse);
            }
            else
            {
                AuthResponse retResponse = new AuthResponse();
                retResponse.Code = "400";
                retResponse.Message = "参数错误";
                retResponse.NowTime = DateTime.Now;
                retResponse.data = "";
                return Ok(retResponse);
            }
        }

        /// <summary>
        /// 登录验证手机号和验证码 实现登录
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        [ApiAuthorize]
        [HttpPost]
        public IHttpActionResult Login([FromBody] LoginLog getmodel)
        {
            /**所需参数  2018-09-25
             *  LoginTel：登录手机号
             *   SmsID：短信ID 
             *   VerCode：短信验证码
             *   头部所需auth：token
             * */
            LoginLog logModel = new LoginLog();
            GetSmsReturn retmodel = new GetSmsReturn();
            //手机号或者验证码或短信ID为空 返回400错误 缺少参数
            if (string.IsNullOrEmpty(getmodel.LoginTel) || string.IsNullOrEmpty(getmodel.SmsID))
            {
                AuthResponse retResponse = new AuthResponse();
                retResponse.Code = "400";
                retResponse.Message = "参数错误";
                retResponse.NowTime = DateTime.Now;
                retResponse.data = "";
                retResponse.IsSuccess = false;
                return Ok(retResponse);
            }

            //获取解析过的token值 包含手机号和短信ID
            AuthInfo info = RequestContext.RouteData.Values["auth"] as AuthInfo;
            //根据smsid和手机号 查询验证码 
            retmodel = gsSer.Where(a => a.smsId == getmodel.SmsID && a.getSmsTel == getmodel.LoginTel).FirstOrDefault();
            //根据传过来的手机号和验证码判断是否匹配并进行登录
            if (info.UserTel == getmodel.LoginTel && retmodel.VerCode == getmodel.VerCode)
            {
                logModel.LoginDate = DateTime.Now;
                logModel.VerCode = getmodel.VerCode;
                logModel.ErrorCode = "0000";
                logModel.ErrorDec = "登录成功";
                logModel.LoginRemark = "登录成功";
                logModel.LoginTel = getmodel.LoginTel;
                logModel.SmsID = getmodel.SmsID;
                logSer.Add(logModel);
                //返回接口json
                AuthResponse retResponse = new AuthResponse();
                retResponse.Code = "200";
                retResponse.Message = "调用接口成功";
                retResponse.NowTime = DateTime.Now;
                retResponse.IsSuccess = true;
                retResponse.data = JsonConvert.SerializeObject(logModel);
                return Ok(retResponse);
            }
            else
            {
                logModel.LoginDate = DateTime.Now;
                logModel.VerCode = getmodel.VerCode;
                logModel.ErrorCode = "0001";
                logModel.ErrorDec = "登录失败";
                logModel.LoginRemark = "验证码和手机号不匹配";
                logModel.LoginTel = getmodel.LoginTel;
                logModel.SmsID = getmodel.SmsID;
                logSer.Add(logModel);
                //统一格式 返回接口json
                AuthResponse retResponse = new AuthResponse();
                retResponse.Code = "200";
                retResponse.Message = "调用接口成功";
                retResponse.NowTime = DateTime.Now;
                retResponse.IsSuccess = true;
                retResponse.data = JsonConvert.SerializeObject(logModel);
                return Ok(retResponse);
            }
        }


        /// <summary>
        /// 根据登录手机号查询是否已经注册过
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [ApiAuthorize]
        [HttpPost]
        public HttpResponseMessage IsUser(dynamic obj)
        {   /**
             * 根据auth获取手机号信息 对数据库进行查询 
             * 返回1表示手机号已经被注册过
             * 返回0表示手机号第一次注册 需要设置用户名 才可进行后续操作
             */
            AuthInfo info = RequestContext.RouteData.Values["auth"] as AuthInfo;
            HttpResponseMessage result = new HttpResponseMessage();


            if (!string.IsNullOrEmpty(info.UserTel))
            {
                UserInfo userModel = userSer.Where(a => a.UserTel == info.UserTel).FirstOrDefault();
                if (userModel != null)
                {
                    retUserAuth ret = new retUserAuth();
                    ret.IsUser = 1;
                    ret.userAuth = getUserAuth(info.UserTel);
                    result.Content = new StringContent(JsonConvert.SerializeObject(ret), Encoding.GetEncoding("UTF-8"), "application/json");
                    return result;
                    //return JsonConvert.SerializeObject(ret);
                }
                else
                {
                    retUserAuth ret = new retUserAuth();
                    ret.IsUser = 0;
                    ret.userAuth = string.Empty;
                    result.Content = new StringContent(JsonConvert.SerializeObject(ret), Encoding.GetEncoding("UTF-8"), "application/json");
                    return result;
                }
            }
            else
            {
                retUserAuth ret = new retUserAuth();
                ret.IsUser = 0;
                ret.userAuth = string.Empty;
                result.Content = new StringContent(JsonConvert.SerializeObject(ret), Encoding.GetEncoding("UTF-8"), "application/json");
                return result;
                //HttpResponseMessage result=new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(ret), Encoding.GetEncoding("UTF-8"), "application/json") };
                //return result；
            }
            }

        /// <summary>
        /// 添加用户信息
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthorize]
        public HttpResponseMessage AddUser([FromBody] JObject jdata)
        {
            /**
             添加用户信息 
             所需参数：头部使用auth验证 
                              用户自行设置的用户名称
              根据auth头解密出信息 获取手机号，后台自动生成用户唯一ID 
              UserID生成 使用随机数进行生成 生成前判断数据库中是否存在该ID
              返回1为添加成功 0为添加失败 -1为用户已存在
             */
            //获取解析过的token值 包含手机号和短信ID
            AuthInfo info = RequestContext.RouteData.Values["auth"] as AuthInfo;
            HttpResponseMessage result = new HttpResponseMessage();
            dynamic json = jdata;
            retUserAuth ret = new retUserAuth();
            //判断用户是否已经存在 
            UserInfo userSelect = userSer.Where(a => a.UserTel == info.UserTel).FirstOrDefault();
            if (userSelect != null)
            {
                ret.IsUser = -1;
                ret.userAuth = string.Empty;
                result.Content = new StringContent(JsonConvert.SerializeObject(ret), Encoding.GetEncoding("UTF-8"), "application/json");
                return result;
            }
            UserInfo userModel = new UserInfo();
            userModel.UserTel = info.UserTel;
            userModel.RegDate = DateTime.Now;
            userModel.UserName = json.UserName;
            userModel.UserID = GetRandomID().ToString();
            try
            {
                userSer.Add(userModel);
                ret.IsUser = 1;
                ret.userAuth = getUserAuth(userModel.UserTel);
                result.Content = new StringContent(JsonConvert.SerializeObject(ret), Encoding.GetEncoding("UTF-8"), "application/json");
                return result;
            }
            catch
            { 
                ret.IsUser = 0;
                ret.userAuth = string.Empty;
                result.Content = new StringContent(JsonConvert.SerializeObject(ret), Encoding.GetEncoding("UTF-8"), "application/json");
                return result;
            }

        }

        /// <summary>
        /// 获取随机用户ID 保证唯一性
        /// </summary>
        /// <returns></returns>
        public int GetRandomID()
        {
            int rdValue;
            Random rd = new Random();
            rdValue = rd.Next(1, 99999);
            UserInfo userSelect = userSer.Where(a => a.UserID == rdValue.ToString()).FirstOrDefault();
            if (userSelect == null)
            {
                return rdValue;
            }
            else
            {
                GetRandomID();
            }
            return rdValue;
        }

        /// <summary>
        /// 根据用户信息生成Usertoken
        /// </summary>
        /// <returns></returns>
        public string getUserAuth(string userTel)
        {
            UserInfo userModel = userSer.Where(a => a.UserTel == userTel).FirstOrDefault();
            if (userModel != null)
            {
                AuthInfo info = new AuthInfo();
                info.UserTel = userTel;
                info.userID = userModel.UserID;
                info.ValidTime = DateTime.Now;
                //token生成时间 由datetime转换成unix时间戳
                info.iat = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
                //token过期时间
                info.exp = (DateTime.Now.AddMinutes(30).ToUniversalTime().Ticks - 621355968000000000) / 10000000;
                return GetToken(info);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static string GetToken(AuthInfo info)
        {
            //设置加密token的key 后续放到web.config 中调用
            string secret = System.Configuration.ConfigurationManager.AppSettings["TokenKey"];
            //采用HS256加密算法
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();//将指定对象序列化
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();//对url重新编码 避免生成特殊字符 通过url传参失败
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            //生成token
            var token = encoder.Encode(info, secret);
            return token;
        }





    }
}
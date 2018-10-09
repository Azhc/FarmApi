using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
//生成token所需库
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
//序列化 反序列化所需库
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Web.Security;
using System.Text;
using Newtonsoft.Json;
using FarmApi.Redis;
using FarmDomain.Service;
using FarmDomain.Entities;
using FarmApi.Model;
using System.Configuration;

namespace FarmApi.Controllers
{
    public class WxController : ApiController
    {
        UserInfoService userSer = new UserInfoService();
        string appid = ConfigurationManager.ConnectionStrings["appid"].ToString();
        string appsecret = ConfigurationManager.ConnectionStrings["appsecret"].ToString();

        /// <summary>
        /// 验证消息是否来自微信服务器 调用接口必须
        /// </summary>
        /// <param name="signature">微信加密签名，signature结合了开发者填写的token参数和请求中的timestamp参数、nonce参数。</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="echostr">随机字符串</param>
        /// <returns></returns>
        ///  * 将token、timestamp、nonce三个参数进行字典序排序
        /// * 将三个参数字符串拼接成一个字符串进行sha1加密
        /// * 开发者获得加密后的字符串可与signature对比，标识该请求来源于微信。

        [HttpGet]
        public HttpResponseMessage checkSignature(string signature, string timestamp, string nonce, string echostr)
        {
            //return echostr;
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            string token = "thisistoken";
            //将变量存入数组
            string[] arrtmp = { token, timestamp, nonce };
            Array.Sort(arrtmp);//字典排序
            string tmpstr = string.Join("", arrtmp);
            tmpstr = FormsAuthentication.HashPasswordForStoringInConfigFile(tmpstr, "SHA1");
            if (tmpstr.ToLower() == signature)
            {
                responseMessage = new HttpResponseMessage { Content = new StringContent(echostr, Encoding.GetEncoding("UTF-8"), "text/plain") };
                return responseMessage;
            }
            else
            {
                responseMessage = new HttpResponseMessage { Content = new StringContent("信息错误", Encoding.GetEncoding("UTF-8"), "text/plain") };
                return responseMessage;
            }
        }


        /// <summary>
        /// 根据微信code获取微信token以及UID 后续获取用户名称等
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage getWxToken(string code,string tel)
        {
            /**
             调用接口前提用户已经使用手机号注册 数据存在数据库中
             此接口只需获取用户token时调用一次 
             用户token分两种 
             access_token 有效期较短 只有两个小时 失效与否可以使用authToken() 判断 
             refresh_token 有效期为30天 失效后调用此方法 重新获取用户token
             */
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            string url = "https://api.weixin.qq.com/sns/oauth2/access_token";
            string data = "?appid=" + appid + "&secret=" + appsecret + "&code=" + code + "&grant_type=authorization_code";
            string getJson = BaseController.HttpGet(url + data);
            JObject jo = (JObject)JsonConvert.DeserializeObject(getJson);
            if (jo.Property("errcode") == null&&jo.Property("access_token")!=null)//判断返回信息是否为错误信息  
            {
                //获取成功后存储用户唯一ID到用户信息表中
                UserInfo userModel = userSer.Where(a => a.UserTel == tel).FirstOrDefault();
                userModel.WxOpenid = jo["openid"].ToString();
                userSer.Update(userModel);
                //以用户openid或者用户手机号为键名 存储用户token信息 以及用户其他信息
                WxUserInfo wxUser = new WxUserInfo();
                wxUser= JsonConvert.DeserializeObject<WxUserInfo>(getJson);
                wxUser.UserTel = tel;
                wxUser.getdate = DateTime.Now;
                //将用户信息序列化为json存储到redis中
                var db = RedisManager.Instance.GetDatabase();
                db.StringSet(userModel.WxOpenid, JsonConvert.SerializeObject(wxUser));
                //前台返回信息 只返回用户openid存储到本地
                responseMessage = new HttpResponseMessage { Content = new StringContent(getJson, Encoding.GetEncoding("UTF-8"), "text/plain") };
                return responseMessage;
            }
            else {
                //直接返回错误信息
                responseMessage = new HttpResponseMessage { Content = new StringContent(getJson, Encoding.GetEncoding("UTF-8"), "text/plain") };
                return responseMessage;
            }



            if (!string.IsNullOrEmpty(getJson))
            {
                responseMessage = new HttpResponseMessage { Content = new StringContent(getJson, Encoding.GetEncoding("UTF-8"), "text/plain") };
                return responseMessage;
            }
            else
            {
                responseMessage = new HttpResponseMessage { Content = new StringContent("接口返回信息错误", Encoding.GetEncoding("UTF-8"), "text/plain") };
                return responseMessage;
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage getWxInfo(string access_token, string openid, string refresh_token)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            //判断token是否可用 可用进行后续操作 
            if (authToken(access_token, openid))
            {
                responseMessage = new HttpResponseMessage { Content = new StringContent(getUserinfo(access_token, openid), Encoding.GetEncoding("UTF-8"), "text/plain") };
                return responseMessage;
            }
            //重新获取最新token并返回最新token
            //使用redis存储最新token
            else
            {
                responseMessage = new HttpResponseMessage { Content = new StringContent(getUserinfo(getNewToken(refresh_token), openid), Encoding.GetEncoding("UTF-8"), "text/plain") };
                return responseMessage;
            }
        }



        /// <summary>
        /// 刷新获取最新access_token
        /// </summary>
        /// <param name="retoken">refresh_token 用来刷新获取access_token</param>
        /// <returns></returns>
        public string getNewToken(string retoken)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            string url = "https://api.weixin.qq.com/sns/oauth2/refresh_token?appid=" + appid + "&grant_type=refresh_token&refresh_token=" + retoken;
            string getJson = BaseController.HttpGet(url);
            if (!string.IsNullOrEmpty(getJson))
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(getJson);
                //json中不包含access_token键 说明调用失败 包含错误代码
                if (jo.Property("access_token") != null)
                {
                    return jo["access_token"].ToString();
                }
                else
                {
                    return "调用错误";
                }
            }
            else
            {
                return "调用错误";
            }
        }

        /// <summary>
        /// 调用接口判断accesstoken是否可用
        /// </summary>
        /// <param name="ACCESS_TOKEN"></param>
        /// <param name="OPENID"></param>
        /// <returns></returns>
        public bool authToken(string ACCESS_TOKEN, string OPENID)
        {
            //将可用的token存储到redis中
            var db = RedisManager.Instance.GetDatabase();
            db.StringSet("access_token", ACCESS_TOKEN); //插入成功，返回true
            string url = "https://api.weixin.qq.com/sns/auth?access_token=" + ACCESS_TOKEN + "&openid=" + OPENID;
            string getJson = BaseController.HttpGet(url);
            JObject jo = (JObject)JsonConvert.DeserializeObject(getJson);
            if (jo["errcode"].ToString() == "0")
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        /// <summary>
        /// 获取微信信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="openid"></param>
        /// <returns></returns>
        public string getUserinfo(string access_token, string openid)
        {
            string url = "https://api.weixin.qq.com/sns/userinfo?access_token=" + access_token + "&openid=" + openid + "&lang=zh_CN";
            string getJson = BaseController.HttpGet(url);
            if (!string.IsNullOrEmpty(getJson))
            {
                return getJson;
            }
            else
            {
                return "接口返回信息错误";
            }
        }

    }
}

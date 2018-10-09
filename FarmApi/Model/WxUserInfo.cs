using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmApi.Model
{
    public class WxUserInfo
    {
        /// <summary>
        ///  用户与公众号中唯一的UID
        /// </summary>
        /// <returns></returns>
        public string openid { get; set; }

        /// <summary>
        /// 用户token 获取用户信息等
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// 用户refresh_token 用来刷新获取最新token 有效期30天
        /// </summary>
        public string refresh_token { get; set; }

        /// <summary>
        /// 用户注册手机号
        /// </summary>
        public string UserTel { get; set; }

        /// <summary>
        /// 获取token时间
        /// </summary>
        public DateTime getdate { get; set; }

    }
}
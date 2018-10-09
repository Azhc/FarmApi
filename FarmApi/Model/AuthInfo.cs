using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmApi.Model
{
    /// <summary>
    /// jwt所需数据字段 生成token
    /// </summary>
    public class AuthInfo
    {
        /// <summary>
        /// 加密所需_用户手机号
        /// </summary>
        public string UserTel { get; set; }

        /// <summary>
        /// 加密所需_发送短信ID
        /// </summary>
        public string VerSmsID { get; set; }

        /// <summary>
        /// 有效时间
        /// </summary>
        public DateTime ValidTime { get; set; }
        /// <summary>
        /// 短信验证码
        /// </summary>
        public string VerCode { get; set; }


        /// <summary>
        /// 用户唯一ID
        /// </summary>
        public string userID { get; set; }

        /// <summary>
        /// token生成时间
        /// </summary>
        public double iat{get;set;}


        /// <summary>
        /// token过期时间
        /// </summary>
        public double exp { get; set; }





    }
}
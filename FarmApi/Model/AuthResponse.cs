using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FarmApi.Model
{
    /// <summary>
    /// 使用token返回json字段
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// 返回代码
        /// </summary>
        public string Code
        {
            get; set;
        }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string Message
        {
            get; set;
        }

        /// <summary>
        /// 调用接口获取数据
        /// </summary>
        public object data
        {
            get; set;
        }

        /// <summary>
        /// 调用接口是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 返回当前时间
        /// </summary>
        public DateTime NowTime
        {
            get; set;
        }


    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace FarmApi.Controllers
{
    /// <summary>
    /// 基类方法
    /// </summary>
    public class BaseController : ApiController
    {
        /// <summary>
        /// post发送数据调用接口
        /// </summary>
        ///  <param name="url">接收地址</param>
        /// <param name="postString">拼接Post字符串"arg1=a&arg2=b"</param>
        /// <returns></returns>
        public static string HttpUploadData(string url, string postString)
        {
            byte[] postData = Encoding.UTF8.GetBytes(postString);//编码，尤其是汉字，事先要看下抓取网页的编码方式  
            WebClient webClient = new WebClient();
            byte[] responseData;
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可  
            responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
            return Encoding.UTF8.GetString(responseData);//解码 
        }

        /// <summary>
        /// get方式调用接口方法
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string HttpGet(string url)
        {
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch {
                return "访问超时";
            }

        }


    }
}

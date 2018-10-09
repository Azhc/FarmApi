using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;

namespace FarmDomain.Repositories
{
	/// <summary>
	/// SessionHelper(NHibernate)
	/// </summary>
	public static class SessionHelper
	{

		/// <summary>
		/// 静态构造方法
		/// </summary>
		static SessionHelper()
		{  
			// 1.配置方式
			factory = new Configuration().Configure().BuildSessionFactory();

			// 2.字符串方式
			// 使用这种方式，可以让实施人员少修改一个链接字符串
			//string con = System.Configuration.ConfigurationManager.ConnectionStrings["sql"].ConnectionString;
			//Configuration cfg = new Configuration().AddAssembly("Domain");//.AddAssembly("ContractBusiness");
			//cfg.SessionFactory().Integrate.Using<MsSql2005Dialect>()
			//    .Connected.Using(con);
			//factory = cfg.BuildSessionFactory();
		}

		/// <summary>
		/// ISessionFactory
		/// </summary>
		public static ISessionFactory Factory
		{
			get { return factory; }
		}

		/// <summary>
		/// 获取ISession
		/// </summary>
		/// <returns>ISession</returns>
		public static ISession GetSession()
		{
			return factory.OpenSession();
		}

		/// <summary>
		/// ISessionFactory
		/// </summary>
		private static ISessionFactory factory;
	}
}

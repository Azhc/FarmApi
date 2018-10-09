using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FarmDomain.Repositories
{
	/// <summary>
	/// 仓储模式接口
	/// </summary>
	/// <typeparam name="T">要存储的实体类</typeparam>
	public interface IRepository<T> : IQueryable<T>
	{
		/// <summary>
		/// 添加
		/// </summary>
		/// <param name="item">T类型的实例</param>
		void Add(T item);

		/// <summary>
		/// 删除
		/// </summary>
		/// <param name="item">T类型的实例</param>
		void Delete(T item);

		/// <summary>
		/// 更改
		/// </summary>
		/// <param name="item">T类型的实例</param>
		void Update(T item);


    }
}

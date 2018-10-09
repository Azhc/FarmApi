using System;

namespace FarmDomain.Entities
{
	/// <summary>
	/// UserInfo
	/// </summary>
	public class UserInfo
	{
		/// <summary>
		/// ID
		/// </summary>
		public virtual int ID { get; set; }

		/// <summary>
		/// UserTel
		/// </summary>
		public virtual string UserTel { get; set; }

		/// <summary>
		/// UserID
		/// </summary>
		public virtual string UserID { get; set; }

		/// <summary>
		/// UserName
		/// </summary>
		public virtual string UserName { get; set; }

		/// <summary>
		/// WxOpenid
		/// </summary>
		public virtual string WxOpenid { get; set; }

		
		/// <summary>
		/// RegDate
		/// </summary>
		public virtual DateTime? RegDate { get; set; }

		#region Override Object Method
		public override string ToString()
		{
			return this.ID.ToString();
		}
		public override int GetHashCode()
		{
			return this.ID.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			if (object.ReferenceEquals(this, obj)) return true;
			if (this.GetType() != obj.GetType()) return false;
			return this.ID == ((UserInfo)obj).ID;
		}
		#endregion
	}
}

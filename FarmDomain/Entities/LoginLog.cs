using System;

namespace FarmDomain.Entities
{
	/// <summary>
	/// LoginLog
	/// </summary>
	public class LoginLog
	{
		/// <summary>
		/// LoginID
		/// </summary>
		public virtual int LoginID { get; set; }

		/// <summary>
		/// LoginDate
		/// </summary>
		public virtual DateTime? LoginDate { get; set; }

		/// <summary>
		/// LoginTel
		/// </summary>
		public virtual string LoginTel { get; set; }

		/// <summary>
		/// LoginRemark
		/// </summary>
		public virtual string LoginRemark { get; set; }

		/// <summary>
		/// VerCode
		/// </summary>
		public virtual string VerCode { get; set; }

		/// <summary>
		/// ErrorCode
		/// </summary>
		public virtual string ErrorCode { get; set; }

		/// <summary>
		/// ErrorDec
		/// </summary>
		public virtual string ErrorDec { get; set; }

		/// <summary>
		/// SmsID
		/// </summary>
		public virtual string SmsID { get; set; }

		#region Override Object Method
		public override string ToString()
		{
			return this.LoginID.ToString();
		}
		public override int GetHashCode()
		{
			return this.LoginID.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			if (object.ReferenceEquals(this, obj)) return true;
			if (this.GetType() != obj.GetType()) return false;
			return this.LoginID == ((LoginLog)obj).LoginID;
		}
		#endregion
	}
}

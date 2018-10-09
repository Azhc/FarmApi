using System;

namespace FarmDomain.Entities
{
	/// <summary>
	/// GetSmsReturn
	/// </summary>
	public class GetSmsReturn
	{
		/// <summary>
		/// LogID
		/// </summary>
		public virtual int LogID { get; set; }

		/// <summary>
		/// getSmsTel
		/// </summary>
		public virtual string getSmsTel { get; set; }

		/// <summary>
		/// getSmsDate
		/// </summary>
		public virtual DateTime? getSmsDate { get; set; }

		/// <summary>
		/// respCode
		/// </summary>
		public virtual string respCode { get; set; }

		/// <summary>
		/// respDesc
		/// </summary>
		public virtual string respDesc { get; set; }

		/// <summary>
		/// failCount
		/// </summary>
		public virtual string failCount { get; set; }

		/// <summary>
		/// smsId
		/// </summary>
		public virtual string smsId { get; set; }

		/// <summary>
		/// VerCode
		/// </summary>
		public virtual string VerCode { get; set; }

        /// <summary>
        /// token
        /// </summary>
        public virtual string auth { get; set; }

		#region Override Object Method
		public override string ToString()
		{
			return this.LogID.ToString();
		}
		public override int GetHashCode()
		{
			return this.LogID.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			if (object.ReferenceEquals(this, obj)) return true;
			if (this.GetType() != obj.GetType()) return false;
			return this.LogID == ((GetSmsReturn)obj).LogID;
		}
		#endregion
	}
}

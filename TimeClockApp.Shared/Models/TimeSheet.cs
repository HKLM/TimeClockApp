using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeClockApp.Shared.Helpers;
#nullable enable

namespace TimeClockApp.Shared.Models
{
    public enum SheetStatus
    {
        Current = 0,
        Unpaid = 1,
        Paid = 2,
        Deleted = 3
    }

    [NotMapped]
    public class TimeSheet
    {
        public TimeSheet() { }

        public TimeSheet(int EmployeeId, DateOnly PayPeriodBegin, DateOnly PayPeriodEnd, string TimeCard_EmployeeName)
        {
            this.EmployeeId = EmployeeId;
            this.TimeCard_EmployeeName = TimeCard_EmployeeName;
            PayPeriodWeekNum = TimeHelper.GetWeekNumber(PayPeriodBegin);
            this.PayPeriodBegin = PayPeriodBegin;
            this.PayPeriodEnd = PayPeriodEnd;
        }

        [NotMapped]
        [Key]
        public int TimeSheetId { get; set; }
        public int EmployeeId { get; set; }
        public SheetStatus Status { get; set; } = SheetStatus.Current;
        /// <summary>
        /// Week number for this pay period
        /// </summary>
        [Required]
        public int PayPeriodWeekNum { get; set; }

        [Column(TypeName = "date")]
        public DateOnly PayPeriodBegin { get; set; }
        [Column(TypeName = "date")]
        public DateOnly PayPeriodEnd { get; set; }

        [Column(TypeName = "double")]
        public double TotalWorkHours { get; set; }
        [Column(TypeName = "double")]
        public double RegTotalHours { get; set; }
        [Column(TypeName = "double")]
        public double TotalOTHours { get; set; }
        [Column(TypeName = "double")]
        public double TotalOT2Hours { get; set; }

 #region UnPaid Hours
        [Column(TypeName = "double")]
        public double UnPaidTotalWorkHours { get; set; }
        [Column(TypeName = "double")]
        public double UnPaidRegTotalHours { get; set; }
        [Column(TypeName = "double")]
        public double UnPaidTotalOTHours { get; set; }
        [Column(TypeName = "double")]
        public double UnPaidTotalOT2Hours { get; set; }

#endregion
        [Column(TypeName = "double")]
        public double RegTotalPay { get; set; }
        [Column(TypeName = "double")]
        public double TotalOTPay { get; set; }
        [Column(TypeName = "double")]
        public double TotalOT2Pay { get; set; }
        [Column(TypeName = "double")]
        public double TotalGrossPay { get; set; }

#region UnPaid Wages
        [Column(TypeName = "double")]
        public double UnPaidRegTotalPay { get; set; }
        [Column(TypeName = "double")]
        public double UnPaidTotalOTPay { get; set; }
        [Column(TypeName = "double")]
        public double UnPaidTotalOT2Pay { get; set; }
        [Column(TypeName = "double")]
        public double TotalOwedGrossPay { get; set; }
        #endregion

        //[NotMapped]
        public string TimeCard_EmployeeName { get; set; } = string.Empty;

        public virtual IList<TimeCard> TimeCards { get; set; } = [];

        [NotMapped]
        public virtual IList<TimeCard?> UnpaidTimeCards { get; set; } = [];
        [NotMapped]
        public virtual IList<TimeCard?> PaidTimeCards { get; set; } = [];

        /// <summary>
        /// Determines if this TimeSheet can be edited. Paid sheets can not be altered.
        /// </summary>
        /// <returns>False if status is Paid or Deleted, otherwise true.</returns>
        public bool AllowEdit() => this.Status < SheetStatus.Paid;

        public void Reset()
        {
            // Do not allow changing a paid TimeSheet
            if (Status == SheetStatus.Paid)
                return;

            TotalWorkHours = 0;
            RegTotalHours = 0;
            TotalOTHours = 0;
            TotalOT2Hours = 0;

            UnPaidTotalWorkHours = 0;
            UnPaidRegTotalHours = 0;
            UnPaidTotalOTHours = 0;
            UnPaidTotalOT2Hours = 0;

            RegTotalPay = 0;
            TotalOTPay = 0;
            TotalOT2Pay = 0;
            TotalGrossPay = 0;

            UnPaidRegTotalPay = 0;
            UnPaidTotalOTPay = 0;
            UnPaidTotalOT2Pay = 0;
            TotalOwedGrossPay = 0;

            if (TimeCards != null)
                TimeCards?.Clear();

            if (UnpaidTimeCards != null)
                UnpaidTimeCards?.Clear();

            if (PaidTimeCards != null)
                PaidTimeCards?.Clear();
        }
    }
}

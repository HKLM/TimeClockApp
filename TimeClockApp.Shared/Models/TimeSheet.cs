using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeClockApp.Shared.Helpers;

namespace TimeClockApp.Shared.Models
{
    public enum SheetStatus
    {
        Current = 0,
        Unpaid = 1,
        Paid = 2,
        Deleted = 3
    }

    public class TimeSheet
    {
        public TimeSheet() { }

        public TimeSheet(int employeeId, DateOnly payPeriodStart, DateOnly payPeriodEnd, string employeeName)
        {
            EmployeeId = employeeId;
            TimeCard_EmployeeName = employeeName;
            PayPeriodWeekNum = TimeHelper.GetWeekNumber(payPeriodStart);
            PayPeriodBegin = payPeriodStart;
            PayPeriodEnd = payPeriodEnd;
        }

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
        public string TimeCard_EmployeeName { get; set; }

#nullable enable
        public virtual IList<TimeCard> TimeCards { get; set; } = new List<TimeCard>();

        [NotMapped]
        public virtual IList<TimeCard?> UnpaidTimeCards { get; set; } = new List<TimeCard?>();
        [NotMapped]
        public virtual IList<TimeCard?> PaidTimeCards { get; set; } = new List<TimeCard?>();

        /// <summary>
        /// Determines if this TimeSheet can be edited. Paid sheets can not be altered.
        /// </summary>
        /// <returns>False if status is Paid or Deleted, otherwise true.</returns>
        public bool AllowEdit() => this.Status < SheetStatus.Paid;

        public void Reset()
        {
            // Do not allow changing a paid TimeSheet
            if (this.Status == SheetStatus.Paid)
                return;

            this.TotalWorkHours = 0;
            this.RegTotalHours = 0;
            this.TotalOTHours = 0;
            this.TotalOT2Hours = 0;

            this.UnPaidTotalWorkHours = 0;
            this.UnPaidRegTotalHours = 0;
            this.UnPaidTotalOTHours = 0;
            this.UnPaidTotalOT2Hours = 0;

            this.RegTotalPay = 0;
            this.TotalOTPay = 0;
            this.TotalOT2Pay = 0;
            this.TotalGrossPay = 0;

            this.UnPaidRegTotalPay = 0;
            this.UnPaidTotalOTPay = 0;
            this.UnPaidTotalOT2Pay = 0;
            this.TotalOwedGrossPay = 0;

            this.TimeCards?.Clear();
            this.UnpaidTimeCards?.Clear();
            this.PaidTimeCards?.Clear();
        }

        public virtual Employee Employee { get; private set; }
    }
}

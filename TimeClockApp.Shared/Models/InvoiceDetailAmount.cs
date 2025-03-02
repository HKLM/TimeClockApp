namespace TimeClockApp.Shared.Models
{
    public class InvoiceDetailAmount
    {
        public double TotalInvoice;
        public double LaborBurden;
        public double Expenses;
        public double Overhead;
        public double OtherFee;
        //if 0 do not apply
        public double MarkupRate = 0.0;
    }
}

namespace TimeClockApp.Shared.Models;

public class TimeCardDayTotal
{
    public double RegTotalHours { get; set; }
    public double TotalOTHours { get; set; }
    public double TotalOT2Hours { get; set; }
    private double _totalWorkHours;
    public double TotalWorkHours
    {
        get => _totalWorkHours;
        set => CalculateWorkingHours(value);
    }
    public TimeCardDayTotal() { }

    public TimeCardDayTotal(double totalWorkHours) => TotalWorkHours = totalWorkHours;

    private void CalculateWorkingHours(double totalWorkHours)
    {
        _totalWorkHours = totalWorkHours;
        double i = Math.Max(0.0, totalWorkHours - 8);
        TotalOTHours = Math.Clamp(i, 0.0, 4.0);
        TotalOT2Hours = Math.Max(0.0, i - 4);
        RegTotalHours = Math.Clamp(totalWorkHours, 0.0, 8.0);
    }

    public void Clear()
    {
        TotalWorkHours = 0.0;
    }
}
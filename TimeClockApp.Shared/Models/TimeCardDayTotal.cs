namespace TimeClockApp.Shared.Models;

public class TimeCardDayTotal
{
    public double RegTotalHours { get; set; } = 0;
    public double TotalOTHours { get; set; } = 0;
    public double TotalOT2Hours { get; set; } = 0;
    private double _totalWorkHours = 0;
    public double TotalWorkHours
    {
        get => _totalWorkHours;
        set => CalculateWorkingHours(value);
    }

    public TimeCardDayTotal(double totalWorkHours) => TotalWorkHours = totalWorkHours;

    private void CalculateWorkingHours(double totalWorkHours)
    {
        _totalWorkHours = totalWorkHours;
        RegTotalHours = Math.Clamp(totalWorkHours, 0.0, 8.0);
        if (totalWorkHours > 8)
        {
            double z = totalWorkHours - 8;
            if (z > 4)
            {
                TotalOTHours = 4;
                TotalOT2Hours = Math.Max(0.0, z - 4);
            }
            else
            {
                TotalOTHours = Math.Clamp(z, 0.0, 4.0);
            }
        }
    }
}
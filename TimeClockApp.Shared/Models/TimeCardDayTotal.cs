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

        if (totalWorkHours <= 8)
            return;

        double overtime = totalWorkHours - 8;
        TotalOTHours = Math.Min(overtime, 4.0);
        TotalOT2Hours = Math.Max(0.0, overtime - 4.0);
    }
}
﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CsvHelper.Configuration;

namespace TimeClockApp.Shared.Models
{
    public enum ProjectStatus
    {
        [Description("Ready")]
        Ready = 0,
        [Description("Active")]
        Active = 1,
        [Description("Completed")]
        Completed = 2,
        [Description("Archived")]
        Archived = 3,
        [Description("Deleted")]
        Deleted = 4
    }

    /// <summary>
    /// Projects are a way to group together TimeCards, Wages and Expenses. From this Project totals can be calculated
    /// TotalWages, TotalExpenses, TotalHours, etc. After a Project has been completed, it can be set tto [Archived].
    /// This will remove that Project from the active ProjectLists, yet keep that Project saved. Another Project of the
    /// same name can be created. Just be sure to have only 1 of them be set to [Active] at one time. If 2+ of same name
    /// are [Active], it would be difficult for the user to determine which Project they actually want.
    /// </summary>
    public class Project
    {
        public Project()
        {
            ProjectDate = DateOnly.FromDateTime(DateTime.Now);
            Status = ProjectStatus.Active;
            TimeCards = new HashSet<TimeCard>();
            Expenses = new HashSet<Expense>();
        }

        public Project(string Name)
        {
            this.Name = Name;
            ProjectDate = DateOnly.FromDateTime(DateTime.Now);
            Status = ProjectStatus.Active;
        }

        [Key]
        public int ProjectId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Used to filter out older (archived, completed, deleted, etc) projects
        /// </summary>
        [Required]
        public ProjectStatus Status { get; set; }

        /// <summary>
        /// Date of project creation.
        /// </summary>
        /// <remarks>Used mainly for filtering out items that come after this projects date</remarks>
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "DateOnly")]
        public DateOnly ProjectDate { get; set; }

        /*
        /// <summary>
        /// Date of project completion.
        /// </summary>
        //[System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "DateOnly")]
        //public DateOnly? ProjectDateEnd { get; set; } = null;
        */

        public virtual ICollection<TimeCard> TimeCards { get; set; }
        public virtual ICollection<Expense> Expenses { get; set; }
    }

    public sealed class ProjectMap : ClassMap<Project>
    {
        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        public ProjectMap()
        {
            //AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.ProjectId);
            Map(m => m.Name);
            Map(m => m.Status);
            Map(m => m.ProjectDate);
            //Map(m => m.ProjectDateEnd).Optional();
        }
    }
}

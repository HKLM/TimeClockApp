//#define TIMESTAMP
namespace TimeClockApp.Shared.Models
{
    /// <summary>
    /// Adds timestamps for when the row has changed and was created.
    /// </summary>
    public partial class BaseEntity
    {
#if TIMESTAMP
        /// <summary>
        /// Auto-generated timestamp the row was first created.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "datetime")]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Auto-updated timestamp each time this row has any value changed.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "datetime")]
        public DateTime DateModified { get; set; }
#endif
    }
}

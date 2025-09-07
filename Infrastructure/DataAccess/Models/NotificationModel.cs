using LinqToDB.Mapping;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ToDoListConsoleBot.Core.DataAccess;

namespace ToDoListConsoleBot.Infrastructure.DataAccess.Models
{
    [Table("Notification")]
    public class NotificationModel
    {
        [PrimaryKey]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("UserId"), NotNull]
        public int UserId { get; set; }

        [Column("Type"), NotNull]
        public string Type { get; set; } = string.Empty;

        [Column("Text"), NotNull]
        public string Text { get; set; } = string.Empty;

        [Column("ScheduledAt"), NotNull]
        public DateTime ScheduledAt { get; set; }

        [Column("IsNotified"), NotNull]
        public bool IsNotified { get; set; }

        [Column("NotifiedAt"), Nullable]
        public DateTime? NotifiedAt { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.Id))]
        public ToDoUserModel User { get; set; } = null!;
    }
}

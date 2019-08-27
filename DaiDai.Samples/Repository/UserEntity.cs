using System;
using DaiDai.Attributes;

namespace DaiDai.Repository
{
    [Table("Users")]
    public class UserEntity : EntityBase
    {
        public string Username { get; set; }
        [Column("CreateTime", Convert = "TODATETIMEOFFSET({0}, DATEPART(tz, SYSDATETIMEOFFSET()))")]
        public DateTimeOffset CreateTime { get; set; }
    }
}
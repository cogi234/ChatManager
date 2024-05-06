using System;

namespace ChatManager.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int From { get; set; }
        public int To { get; set; }

        public string Content { get; set; }

        public DateTime SendTime { get; set; } = DateTime.Now;
        public DateTime ReadTime { get; set; } = DateTime.MinValue;
    }
}
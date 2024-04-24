using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ChatManager.Models
{
    public enum RelationshipStatus { None, Request, Denied, Friend }
    public class Relationship
    {
        public (int from, int to) Id { get; set; }

        public RelationshipStatus Status { get; set; }
    }
}
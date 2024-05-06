namespace ChatManager.Models
{
    public enum RelationshipStatus { None, Request, Denied, Friend }
    public class Relationship
    {
        public (int from, int to) Id { get; set; }

        public RelationshipStatus Status { get; set; }
    }
}
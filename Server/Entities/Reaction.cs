using System;

namespace Entities;

public class Reaction
{
    public int ReactionId {get; set;}
    public ReactionType ReactionType {get; set;}
    public int UserId { get; set; }
    public int ContentId { get; set; }
}

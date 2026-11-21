namespace Entities;

using System;
using ApiContracts;

public class Reaction
{
    public int ReactionId { get; set; }
    public ReactionType ReactionType { get; set; }
    public int UserId { get; set; }
    public int ContentId { get; set; }
    public required ContentType ContentType { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;

    /*
    Det content, som reaktionen tilhører, identificeres ud fra `ContentId` og `ContentType`.
    Derfor kan relationen ikke modelleres som en almindelig foreign key-relation i EF Core.
    Der er ingen direkte navigation property til `Content`, og EF Core skal ikke konfigurere 
    en foreign key for denne relation.
    */
}

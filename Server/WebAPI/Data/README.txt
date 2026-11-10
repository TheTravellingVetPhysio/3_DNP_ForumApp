ForumApp seed data

Replace the corresponding JSON repository files with these files.

Test accounts:
- Admin: username `admin`, password `Admin123!`
- Regular user: username `julie`, password `Julie123!`
- Regular user: username `alex`, password `Alex123!`

Roles:
- Role 0 = User
- Role 1 = Admin

Assumed enum values (based on the existing sample data and typical enum ordering):
- ReactionType 0 = Like, 1 = Dislike
- ContentType 0 = Post, 1 = Comment

The PasswordHash values use the ASP.NET Core Identity PasswordHasher V3 format
(default HMAC-SHA512, 100,000 iterations). They are independently salted hashes,
not plaintext passwords.

Important:
- Back up your current JSON files before replacing them.
- The seed data assumes the entity/repository property names shown in your examples.
- The `ContentId` values are independent sequences for posts and comments.

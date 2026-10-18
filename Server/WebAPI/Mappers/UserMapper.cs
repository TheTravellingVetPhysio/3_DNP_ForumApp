
namespace WebAPI.Mappers;

using ApiContracts;
using Entities;

public static class UserMapper
{
    public static UserDTO ToDTO(User user)
    {
        return new UserDTO
        {
            UserId = user.UserId,
            Username = user.Username
        };
    }
}

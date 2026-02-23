using ConfigurationManager.Db.Models;

namespace ConfigurationManager.Core.Models.Dto.Mappings;

public static class UserMappings
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }

    public static IEnumerable<UserDto> ToDto(this IEnumerable<User> users)
    {
        return users.Select(u => u.ToDto());
    }
}
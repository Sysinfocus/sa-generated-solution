namespace Demo.API.Mappings;

public static class UserMappings
{
    public static UserDto ToDTO(this User model)
    {
        return new() {
            Fullname = model.Fullname            
        };
    }

    public static User ToModel(this UserDto dto, Guid? id = null)
    {
        return new(
            id ?? Guid.NewGuid(),
            dto.Fullname
        );
    }
}
namespace WebAPI.Mappers;

using ApiContracts;
using Entities;

public static class SubforumMapper
{
    public static SubforumDTO ToDTO(Subforum subforum)
    {
        return new SubforumDTO
        {
            SubforumId = subforum.SubforumId,
            Name = subforum.Name
        };
    }

    public static List<SubforumDTO> ToDTOList(
        IEnumerable<Subforum> subforums)
    {
        return subforums.Select(ToDTO).ToList();
    }
}
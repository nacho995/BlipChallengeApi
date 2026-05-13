namespace BlipChallengeApi.Models;

public class GithubRepository
{
    public string? Full_Name { get; set; }

    public string? Description { get; set; }

    public string? Language { get; set; }

    public DateTime Created_At { get; set; }

    public Owner? Owner { get; set; }
}

public class Owner
{
    public string? Avatar_Url { get; set; }
}
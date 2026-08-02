using NodaTime;
using StarterKit.Api.Data;

namespace StarterKit.Api.Models;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public string UserId { get; set; }
    public Instant Expires { get; set; }
    public ApplicationUser ApplicationUser { get; set; }
}

using PadelBooking.Domain.Common;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Domain.Entities;

/// <summary>
/// An admin/staff login account. Passwords are never stored in plain text —
/// only a salted hash (see Infrastructure/Identity in Phase 2).
/// </summary>
public class AdminUser : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public AdminRole Role { get; set; } = AdminRole.Admin;
    public bool IsActive { get; set; } = true;
}

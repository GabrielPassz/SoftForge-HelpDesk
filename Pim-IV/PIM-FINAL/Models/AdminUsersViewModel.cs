using System.Collections.Generic;

namespace PIM_FINAL.Models
{
 public class AdminUsersViewModel
 {
 public List<Usuario> ActiveUsers { get; set; } = new List<Usuario>();
 public List<Usuario> PendingUsers { get; set; } = new List<Usuario>();
 public int TotalActiveCount => ActiveUsers?.Count ??0;
 public int PendingCount => PendingUsers?.Count ??0;
 // additional metrics
 public int TotalUsers => (ActiveUsers?.Count ??0) + (PendingUsers?.Count ??0);
 }
}

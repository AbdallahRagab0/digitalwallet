using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PaymentsApplication.Models;

[Table("users")]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    [StringLength(100)]
    public string UserName { get; set; } = null!;

    [StringLength(100)]
    public string PhoneNumber { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Balance { get; set; }

    [StringLength(20)]
    public string Password { get; set; } = null!;

    [StringLength(80)]
    public string? UserEmail { get; set; }
}

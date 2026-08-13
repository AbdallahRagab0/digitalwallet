using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PaymentsApplication.Models;

[Table("transactions")]
public partial class Transaction
{
    [Key]
    [Column("transactionID")]
    public int TransactionId { get; set; }

    [Column("senderID")]
    public int SenderId { get; set; }

    [Column("receiverID")]
    public int ReceiverId { get; set; }

    [Column("senderPhoneNumber")]
    [StringLength(11)]
    public string SenderPhoneNumber { get; set; } = null!;

    [Column("receiverPhoneNumber")]
    [StringLength(11)]
    public string ReceiverPhoneNumber { get; set; } = null!;

    [Column("amount", TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    [Column("transactionType")]
    [StringLength(50)]
    public string TransactionType { get; set; } = null!;

    [Column("transactionDate", TypeName = "datetime")]
    public DateTime TransactionDate { get; set; }
}

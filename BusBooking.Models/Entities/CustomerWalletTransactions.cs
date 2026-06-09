using System.ComponentModel.DataAnnotations.Schema;
using BusBooking.Core.Attributes;

namespace BusBooking.Models.Entities
{
    [Table("CustomerWalletTransactions")]
    [ReadTableName("CustomerWalletTransactions")]
    [WriteTableName("CustomerWalletTransactions")]
    public class CustomerWalletTransactions
    {
        public int Id { get; set; }
        public int CustomerWalletId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }
        
        public DateTime CreatedAt { get; set; }

        public virtual CustomerWallet CustomerWallet { get; set; }
    }
}
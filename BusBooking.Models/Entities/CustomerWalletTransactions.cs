namespace BusBooking.Models.Entities
{
    public class CustomerWalletTransactions
    {
        public int Id { get; set; }
        public int CustomerWalletId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }
        
        public DateTime CreatedAt { get; set; }

        public virtual CustomerWallet CustomerWallet { get; set; }
    }
}
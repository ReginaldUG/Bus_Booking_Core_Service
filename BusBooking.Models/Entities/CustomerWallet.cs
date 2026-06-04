namespace BusBooking.Models.Entities
{
    public class CustomerWallet
    {
        public int Id { get; set; }
        public int CustomerId {get; set; }        
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual ICollection<CustomerWalletTransactions> Transactions { get; set; }= new List<CustomerWalletTransactions>();
    }
}
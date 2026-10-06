namespace DAL.Entities
{
    public enum UserRole { Staff, Admin}
    public enum RoomType { Single, Double, Suite }
    public enum RoomStatus { Active, Maintenance, Inactive }
    public enum PaymentStatus { Paid, PartiallyPaid, Unpaid }
    public enum PaymentMethod { Cash, Card, BankTransfer }
    public enum ChargeType {  PerNight, Flat}
}
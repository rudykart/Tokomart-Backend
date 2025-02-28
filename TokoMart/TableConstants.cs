namespace TokoMart
{
    public static class TableConstants
    {
        public static class Products
        {
            public const string TableName = "products";
            public const string Id = "id";
            public const string Name = "name";
            public const string Category = "category";
            public const string Price = "price";
            public const string Stock = "stock";
            public const string CreatedAt = "created_at";
            public const string UpdatedAt = "updated_at";
        }

        public static class Discounts
        {
            public const string TableName = "discounts";
            public const string Id = "id";
            public const string Value = "value";
            public const string StartAt = "start_at";
            public const string ExpiredAt = "expired_at";
            public const string ProductId = "product_id";
            public const string CreatedAt = "created_at";
            public const string UpdatedAt = "updated_at";
        }

        public static class Customers
        {
            public const string TableName = "customers";
            public const string Id = "id";
            public const string Name = "name";
            public const string PhoneNumber = "phone_number";
            public const string Email = "email";
            public const string Member = "member";
            public const string CreatedAt = "created_at";
            public const string UpdatedAt = "updated_at";
        }

        public static class Users
        {
            public const string TableName = "users";
            public const string Id = "id";
            public const string Name = "name";
            public const string Username = "username";
            public const string Password = "password";
            public const string Role = "role";
            public const string CreatedAt = "created_at";
            public const string UpdatedAt = "updated_at";
        }

        public static class Transactions
        {
            public const string TableName = "transactions";
            public const string Id = "id";
            public const string TotalAmount = "total_amount";
            public const string CustomerId = "customer_id";
            public const string UserId = "user_id";
            public const string CreatedAt = "created_at";
            public const string UpdatedAt = "updated_at";
        }

        public static class ProductTransactions
        {
            public const string TableName = "product_transactions";
            public const string ProductId = "product_id";
            public const string TransactionId = "transaction_id";
            public const string Quantity = "quantity";
            public const string Price = "price";
            public const string Discount = "discount";
        }

        public static class Classifications
        {
            public const string TableName = "classifications";
            public const string Id = "id";
            public const string Name = "name";
            public const string Description = "description";
            public const string TableNameField = "table_name"; // Menghindari bentrok dengan nama kelas
            public const string FieldName = "field_name";
            public const string CreatedAt = "created_at";
            public const string UpdatedAt = "updated_at";
        }

        public static class Attachments
        {
            public const string TableName = "attachments";
            public const string Id = "id";
            public const string TableNameField = "table_name";
            public const string FileId = "file_id";
            public const string FileType = "file_type";
            public const string FilePath = "file_path";
            public const string CreatedAt = "created_at";
            public const string UpdatedAt = "updated_at";
        }

        public static class Notifications
        {
            public const string TableName = "notifications";
            public const string Id = "id";
            public const string Title = "title";
            public const string Description = "description";
            public const string TableNameField = "table_name";
            public const string PathId = "path_id";
            public const string UserId = "user_id";
            public const string CreatedAt = "created_at";
            public const string UpdatedAt = "updated_at";
        }
    }
}
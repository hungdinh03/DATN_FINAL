namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateProduct : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Product", "Price", c => c.Int(nullable: false));
            AlterColumn("dbo.Product", "PriceSale", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Product", "PriceSale", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.Product", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}

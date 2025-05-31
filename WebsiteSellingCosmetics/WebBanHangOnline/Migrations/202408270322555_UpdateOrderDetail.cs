namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateOrderDetail : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OrderDetail", "Price", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.OrderDetail", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}

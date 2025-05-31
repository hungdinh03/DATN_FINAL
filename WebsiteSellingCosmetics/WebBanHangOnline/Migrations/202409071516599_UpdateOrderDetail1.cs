namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateOrderDetail1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderDetail", "ProductName", c => c.String());
            AddColumn("dbo.OrderDetail", "Alias", c => c.String());
            AddColumn("dbo.OrderDetail", "CategoryName", c => c.String());
            AddColumn("dbo.OrderDetail", "ProductImg", c => c.String());
            AddColumn("dbo.OrderDetail", "TotalPrice", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderDetail", "TotalPrice");
            DropColumn("dbo.OrderDetail", "ProductImg");
            DropColumn("dbo.OrderDetail", "CategoryName");
            DropColumn("dbo.OrderDetail", "Alias");
            DropColumn("dbo.OrderDetail", "ProductName");
        }
    }
}

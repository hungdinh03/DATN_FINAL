namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateCartItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CartItem", "ProductName", c => c.String());
            AddColumn("dbo.CartItem", "Alias", c => c.String());
            AddColumn("dbo.CartItem", "CategoryName", c => c.String());
            AddColumn("dbo.CartItem", "ProductImg", c => c.String());
            AddColumn("dbo.CartItem", "Price", c => c.Int(nullable: false));
            AddColumn("dbo.CartItem", "TotalPrice", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CartItem", "TotalPrice");
            DropColumn("dbo.CartItem", "Price");
            DropColumn("dbo.CartItem", "ProductImg");
            DropColumn("dbo.CartItem", "CategoryName");
            DropColumn("dbo.CartItem", "Alias");
            DropColumn("dbo.CartItem", "ProductName");
        }
    }
}

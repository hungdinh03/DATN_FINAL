namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateProductImage2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductImage", "IsHover", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProductImage", "IsFeature", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductImage", "IsFeature");
            DropColumn("dbo.ProductImage", "IsHover");
        }
    }
}

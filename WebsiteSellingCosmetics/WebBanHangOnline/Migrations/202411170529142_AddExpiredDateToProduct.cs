namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExpiredDateToProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "ExpiredDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "ExpiredDate");
        }
    }
}

namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateOrder1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Order", "Email", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Order", "Email");
        }
    }
}

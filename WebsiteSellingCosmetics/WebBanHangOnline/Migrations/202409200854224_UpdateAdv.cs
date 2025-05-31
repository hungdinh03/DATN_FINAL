namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateAdv : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Adv", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Adv", "IsActive");
        }
    }
}

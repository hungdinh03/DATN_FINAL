namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateAdv2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Adv", "SubTitle");
            DropColumn("dbo.Adv", "Description");
            DropColumn("dbo.Adv", "Type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Adv", "Type", c => c.Int(nullable: false));
            AddColumn("dbo.Adv", "Description", c => c.String(nullable: false, maxLength: 500));
            AddColumn("dbo.Adv", "SubTitle", c => c.String(nullable: false, maxLength: 150));
        }
    }
}

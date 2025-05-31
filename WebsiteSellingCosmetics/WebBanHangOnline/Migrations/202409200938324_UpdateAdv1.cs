namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateAdv1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Adv", "SubTitle", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.Adv", "Description", c => c.String(nullable: false, maxLength: 500));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Adv", "Description", c => c.String(maxLength: 500));
            DropColumn("dbo.Adv", "SubTitle");
        }
    }
}

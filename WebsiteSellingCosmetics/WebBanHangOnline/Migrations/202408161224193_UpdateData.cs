namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateData : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Category", "Alias", c => c.String());
            AddColumn("dbo.News", "Alias", c => c.String());
            AddColumn("dbo.Post", "Alias", c => c.String());
            AddColumn("dbo.Product", "Alias", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "Alias");
            DropColumn("dbo.Post", "Alias");
            DropColumn("dbo.News", "Alias");
            DropColumn("dbo.Category", "Alias");
        }
    }
}

namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateOrder3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Order", "Status", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Order", "Status", c => c.String());
        }
    }
}

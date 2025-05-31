namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateSystemConfig : DbMigration
    {
        public override void Up()
        {
            CreateTable(
            "dbo.SystemConfig",
            c => new
            {
                Id = c.Int(nullable: false, identity: true),
                Title = c.String(nullable: false, maxLength: 150),
                Position = c.Int(nullable: false),
                Alias = c.String(nullable: true, maxLength: 200),
            })
            .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
            DropTable("dbo.SystemConfig");
        }
    }
}

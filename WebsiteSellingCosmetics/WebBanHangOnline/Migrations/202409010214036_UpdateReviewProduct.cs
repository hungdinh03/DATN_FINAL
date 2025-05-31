namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateReviewProduct : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ReviewProducts", "ProductId");
            AddForeignKey("dbo.ReviewProducts", "ProductId", "dbo.Product", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReviewProducts", "ProductId", "dbo.Product");
            DropIndex("dbo.ReviewProducts", new[] { "ProductId" });
        }
    }
}

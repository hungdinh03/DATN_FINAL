﻿namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateOrder2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Order", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Order", "Status");
        }
    }
}

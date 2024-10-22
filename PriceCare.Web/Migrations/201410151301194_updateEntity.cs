namespace PriceCare.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateEntity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "LastConnectionDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "LastConnectionDate");
        }
    }
}

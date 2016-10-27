namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BashPost",
                c => new
                    {
                        BashPostID = c.Int(nullable: false, identity: true),
                        title = c.String(),
                        text = c.String(),
                        rating = c.Int(nullable: false),
                        dt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.BashPostID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.BashPost");
        }
    }
}

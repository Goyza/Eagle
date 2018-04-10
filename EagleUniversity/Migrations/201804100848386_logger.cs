namespace EagleUniversity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class logger : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExceptionLoggers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExceptionMessage = c.String(),
                        ControllerName = c.String(),
                        ExceptionStackTrace = c.String(),
                        LogTime = c.DateTime(nullable: false),
                        userId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ExceptionLoggers");
        }
    }
}

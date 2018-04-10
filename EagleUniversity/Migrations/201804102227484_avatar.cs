namespace EagleUniversity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class avatar : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApplicationUserDocuments",
                c => new
                    {
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                        Document_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApplicationUser_Id, t.Document_Id })
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.Documents", t => t.Document_Id, cascadeDelete: true)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.Document_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApplicationUserDocuments", "Document_Id", "dbo.Documents");
            DropForeignKey("dbo.ApplicationUserDocuments", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.ApplicationUserDocuments", new[] { "Document_Id" });
            DropIndex("dbo.ApplicationUserDocuments", new[] { "ApplicationUser_Id" });
            DropTable("dbo.ApplicationUserDocuments");
        }
    }
}

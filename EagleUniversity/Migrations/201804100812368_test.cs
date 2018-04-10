namespace EagleUniversity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Activities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ActivityName = c.String(nullable: false, maxLength: 30),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        ModuleId = c.Int(nullable: false),
                        ActivityTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActivityTypes", t => t.ActivityTypeId, cascadeDelete: true)
                .ForeignKey("dbo.Modules", t => t.ModuleId, cascadeDelete: true)
                .Index(t => t.ModuleId)
                .Index(t => t.ActivityTypeId);
            
            CreateTable(
                "dbo.ActivityTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ActivityTypeName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ActivityDocuments",
                c => new
                    {
                        DocumentId = c.Int(nullable: false),
                        ActivityId = c.Int(nullable: false),
                        AssignDate = c.DateTime(nullable: false),
                        OwnerId = c.String(),
                    })
                .PrimaryKey(t => new { t.DocumentId, t.ActivityId })
                .ForeignKey("dbo.Activities", t => t.ActivityId, cascadeDelete: true)
                .ForeignKey("dbo.Documents", t => t.DocumentId, cascadeDelete: true)
                .Index(t => t.DocumentId)
                .Index(t => t.ActivityId);
            
            CreateTable(
                "dbo.Documents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentName = c.String(),
                        DocumentContent = c.String(),
                        Content = c.Binary(),
                        FileType = c.String(),
                        UploadDate = c.DateTime(nullable: false),
                        DueDate = c.DateTime(nullable: false),
                        DocumentTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DocumentTypes", t => t.DocumentTypeId, cascadeDelete: true)
                .Index(t => t.DocumentTypeId);
            
            CreateTable(
                "dbo.CourseDocuments",
                c => new
                    {
                        DocumentId = c.Int(nullable: false),
                        CourseId = c.Int(nullable: false),
                        AssignDate = c.DateTime(nullable: false),
                        OwnerId = c.String(),
                    })
                .PrimaryKey(t => new { t.DocumentId, t.CourseId })
                .ForeignKey("dbo.Courses", t => t.CourseId, cascadeDelete: true)
                .ForeignKey("dbo.Documents", t => t.DocumentId, cascadeDelete: true)
                .Index(t => t.DocumentId)
                .Index(t => t.CourseId);
            
            CreateTable(
                "dbo.Courses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseName = c.String(nullable: false, maxLength: 30),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        OwnerId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Modules",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ModuleName = c.String(nullable: false, maxLength: 30),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        CourseId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Courses", t => t.CourseId, cascadeDelete: true)
                .Index(t => t.CourseId);
            
            CreateTable(
                "dbo.ModuleDocuments",
                c => new
                    {
                        DocumentId = c.Int(nullable: false),
                        ModuleId = c.Int(nullable: false),
                        AssignDate = c.DateTime(nullable: false),
                        OwnerId = c.String(),
                    })
                .PrimaryKey(t => new { t.DocumentId, t.ModuleId })
                .ForeignKey("dbo.Documents", t => t.DocumentId, cascadeDelete: true)
                .ForeignKey("dbo.Modules", t => t.ModuleId, cascadeDelete: true)
                .Index(t => t.DocumentId)
                .Index(t => t.ModuleId);
            
            CreateTable(
                "dbo.Assignments",
                c => new
                    {
                        ApplicationUserId = c.String(nullable: false, maxLength: 128),
                        CourseId = c.Int(nullable: false),
                        AssignDate = c.DateTime(nullable: false),
                        OwnerId = c.String(),
                    })
                .PrimaryKey(t => new { t.ApplicationUserId, t.CourseId })
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId, cascadeDelete: true)
                .ForeignKey("dbo.Courses", t => t.CourseId, cascadeDelete: true)
                .Index(t => t.ApplicationUserId)
                .Index(t => t.CourseId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(nullable: false, maxLength: 50),
                        RegistrationTime = c.DateTime(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.DocumentTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentTypeName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Documents", "DocumentTypeId", "dbo.DocumentTypes");
            DropForeignKey("dbo.CourseDocuments", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.Assignments", "CourseId", "dbo.Courses");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Assignments", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ModuleDocuments", "ModuleId", "dbo.Modules");
            DropForeignKey("dbo.ModuleDocuments", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.Modules", "CourseId", "dbo.Courses");
            DropForeignKey("dbo.Activities", "ModuleId", "dbo.Modules");
            DropForeignKey("dbo.CourseDocuments", "CourseId", "dbo.Courses");
            DropForeignKey("dbo.ActivityDocuments", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.ActivityDocuments", "ActivityId", "dbo.Activities");
            DropForeignKey("dbo.Activities", "ActivityTypeId", "dbo.ActivityTypes");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Assignments", new[] { "CourseId" });
            DropIndex("dbo.Assignments", new[] { "ApplicationUserId" });
            DropIndex("dbo.ModuleDocuments", new[] { "ModuleId" });
            DropIndex("dbo.ModuleDocuments", new[] { "DocumentId" });
            DropIndex("dbo.Modules", new[] { "CourseId" });
            DropIndex("dbo.CourseDocuments", new[] { "CourseId" });
            DropIndex("dbo.CourseDocuments", new[] { "DocumentId" });
            DropIndex("dbo.Documents", new[] { "DocumentTypeId" });
            DropIndex("dbo.ActivityDocuments", new[] { "ActivityId" });
            DropIndex("dbo.ActivityDocuments", new[] { "DocumentId" });
            DropIndex("dbo.Activities", new[] { "ActivityTypeId" });
            DropIndex("dbo.Activities", new[] { "ModuleId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.DocumentTypes");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Assignments");
            DropTable("dbo.ModuleDocuments");
            DropTable("dbo.Modules");
            DropTable("dbo.Courses");
            DropTable("dbo.CourseDocuments");
            DropTable("dbo.Documents");
            DropTable("dbo.ActivityDocuments");
            DropTable("dbo.ActivityTypes");
            DropTable("dbo.Activities");
        }
    }
}

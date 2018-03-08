namespace GH.Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGalleryModels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GalleryFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 255),
                        Description = c.String(maxLength: 255),
                        Path = c.String(nullable: false),
                        CompressedPath = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        Creator_Id = c.String(maxLength: 128),
                        Folder_Id = c.Int(),
                        Modifier_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Creator_Id)
                .ForeignKey("dbo.GalleryFolders", t => t.Folder_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Modifier_Id)
                .Index(t => t.Creator_Id)
                .Index(t => t.Folder_Id)
                .Index(t => t.Modifier_Id);
            
            CreateTable(
                "dbo.GalleryFolders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GalleryType = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 255),
                        Description = c.String(maxLength: 255),
                        PreviewImage = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        Creator_Id = c.String(maxLength: 128),
                        Modifier_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Creator_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Modifier_Id)
                .Index(t => t.Creator_Id)
                .Index(t => t.Modifier_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GalleryFiles", "Modifier_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.GalleryFolders", "Modifier_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.GalleryFiles", "Folder_Id", "dbo.GalleryFolders");
            DropForeignKey("dbo.GalleryFolders", "Creator_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.GalleryFiles", "Creator_Id", "dbo.AspNetUsers");
            DropIndex("dbo.GalleryFolders", new[] { "Modifier_Id" });
            DropIndex("dbo.GalleryFolders", new[] { "Creator_Id" });
            DropIndex("dbo.GalleryFiles", new[] { "Modifier_Id" });
            DropIndex("dbo.GalleryFiles", new[] { "Folder_Id" });
            DropIndex("dbo.GalleryFiles", new[] { "Creator_Id" });
            DropTable("dbo.GalleryFolders");
            DropTable("dbo.GalleryFiles");
        }
    }
}

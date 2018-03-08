namespace GH.Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLayoutZone : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CmsLayouts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 255),
                        Name = c.String(nullable: false, maxLength: 255),
                        Description = c.String(maxLength: 255),
                        HtmlPreview = c.String(),
                        Path = c.String(nullable: false, maxLength: 255),
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
            
            CreateTable(
                "dbo.CmsZones",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        Description = c.String(maxLength: 255),
                        Layout_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CmsLayouts", t => t.Layout_Id)
                .Index(t => t.Layout_Id);
            
            CreateTable(
                "dbo.CmsZoneContents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Content = c.String(),
                        Zone_Id = c.Int(),
                        CmsPage_Id = c.Int(),
                        CmsPage_Id1 = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CmsZones", t => t.Zone_Id)
                .ForeignKey("dbo.CmsPages", t => t.CmsPage_Id)
                .ForeignKey("dbo.CmsPages", t => t.CmsPage_Id1)
                .Index(t => t.Zone_Id)
                .Index(t => t.CmsPage_Id)
                .Index(t => t.CmsPage_Id1);
            
            AddColumn("dbo.CmsPages", "DraftLayout_Id", c => c.Int());
            AddColumn("dbo.CmsPages", "Layout_Id", c => c.Int());
            AlterColumn("dbo.CmsImages", "Path", c => c.String(nullable: false, maxLength: 255));
            CreateIndex("dbo.CmsPages", "DraftLayout_Id");
            CreateIndex("dbo.CmsPages", "Layout_Id");
            AddForeignKey("dbo.CmsPages", "DraftLayout_Id", "dbo.CmsLayouts", "Id");
            AddForeignKey("dbo.CmsPages", "Layout_Id", "dbo.CmsLayouts", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CmsZoneContents", "CmsPage_Id1", "dbo.CmsPages");
            DropForeignKey("dbo.CmsPages", "Layout_Id", "dbo.CmsLayouts");
            DropForeignKey("dbo.CmsZoneContents", "CmsPage_Id", "dbo.CmsPages");
            DropForeignKey("dbo.CmsPages", "DraftLayout_Id", "dbo.CmsLayouts");
            DropForeignKey("dbo.CmsZones", "Layout_Id", "dbo.CmsLayouts");
            DropForeignKey("dbo.CmsZoneContents", "Zone_Id", "dbo.CmsZones");
            DropForeignKey("dbo.CmsLayouts", "Modifier_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.CmsLayouts", "Creator_Id", "dbo.AspNetUsers");
            DropIndex("dbo.CmsPages", new[] { "Layout_Id" });
            DropIndex("dbo.CmsPages", new[] { "DraftLayout_Id" });
            DropIndex("dbo.CmsZoneContents", new[] { "CmsPage_Id1" });
            DropIndex("dbo.CmsZoneContents", new[] { "CmsPage_Id" });
            DropIndex("dbo.CmsZoneContents", new[] { "Zone_Id" });
            DropIndex("dbo.CmsZones", new[] { "Layout_Id" });
            DropIndex("dbo.CmsLayouts", new[] { "Modifier_Id" });
            DropIndex("dbo.CmsLayouts", new[] { "Creator_Id" });
            AlterColumn("dbo.CmsImages", "Path", c => c.String());
            DropColumn("dbo.CmsPages", "Layout_Id");
            DropColumn("dbo.CmsPages", "DraftLayout_Id");
            DropTable("dbo.CmsZoneContents");
            DropTable("dbo.CmsZones");
            DropTable("dbo.CmsLayouts");
        }
    }
}

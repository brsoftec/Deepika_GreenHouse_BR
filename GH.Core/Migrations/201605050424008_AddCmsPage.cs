namespace GH.Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCmsPage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CmsPages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        Content = c.String(),
                        DraftContent = c.String(),
                        MetaTitle = c.String(maxLength: 255),
                        DraftMetaTitle = c.String(maxLength: 255),
                        MetaContent = c.String(maxLength: 255),
                        DraftMetaContent = c.String(maxLength: 255),
                        MetaDescription = c.String(maxLength: 255),
                        DraftMetaDescription = c.String(maxLength: 255),
                        PublishedDate = c.DateTime(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        PermanentLink = c.String(nullable: false, maxLength: 255),
                        HasDraft = c.Boolean(nullable: false),
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
            DropForeignKey("dbo.CmsPages", "Modifier_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.CmsPages", "Creator_Id", "dbo.AspNetUsers");
            DropIndex("dbo.CmsPages", new[] { "Modifier_Id" });
            DropIndex("dbo.CmsPages", new[] { "Creator_Id" });
            DropTable("dbo.CmsPages");
        }
    }
}

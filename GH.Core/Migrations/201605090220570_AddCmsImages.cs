namespace GH.Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCmsImages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CmsImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Path = c.String(),
                        Uploader_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Uploader_Id)
                .Index(t => t.Uploader_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CmsImages", "Uploader_Id", "dbo.AspNetUsers");
            DropIndex("dbo.CmsImages", new[] { "Uploader_Id" });
            DropTable("dbo.CmsImages");
        }
    }
}

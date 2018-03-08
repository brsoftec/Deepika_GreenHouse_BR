namespace GH.Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateCmsPage : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.CmsPages", "MetaContent");
            DropColumn("dbo.CmsPages", "DraftMetaContent");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CmsPages", "DraftMetaContent", c => c.String(maxLength: 255));
            AddColumn("dbo.CmsPages", "MetaContent", c => c.String(maxLength: 255));
        }
    }
}

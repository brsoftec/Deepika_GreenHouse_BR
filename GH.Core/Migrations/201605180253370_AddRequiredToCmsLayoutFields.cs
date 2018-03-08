namespace GH.Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRequiredToCmsLayoutFields : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CmsLayouts", "HtmlPreview", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CmsLayouts", "HtmlPreview", c => c.String());
        }
    }
}

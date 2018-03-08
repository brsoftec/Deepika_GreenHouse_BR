namespace GH.Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveCodeFieldFromLayout : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CmsLayouts", "Default", c => c.Boolean(nullable: false));
            DropColumn("dbo.CmsLayouts", "Code");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CmsLayouts", "Code", c => c.String(nullable: false, maxLength: 255));
            DropColumn("dbo.CmsLayouts", "Default");
        }
    }
}

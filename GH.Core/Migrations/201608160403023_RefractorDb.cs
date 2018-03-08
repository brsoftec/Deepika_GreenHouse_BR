namespace GH.Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefractorDb : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.FacebookLongLivedAccessTokens", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.FacebookLongLivedAccessTokens", new[] { "UserId" });
            DropColumn("dbo.AspNetUsers", "FacebookLongLivedAccessToken");
            DropColumn("dbo.AspNetUsers", "FacebookLongLivedAccessTokenCreatedAt");
            DropTable("dbo.FacebookLongLivedAccessTokens");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.FacebookLongLivedAccessTokens",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        Token = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UserCode = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
            AddColumn("dbo.AspNetUsers", "FacebookLongLivedAccessTokenCreatedAt", c => c.DateTime());
            AddColumn("dbo.AspNetUsers", "FacebookLongLivedAccessToken", c => c.String());
            CreateIndex("dbo.FacebookLongLivedAccessTokens", "UserId");
            AddForeignKey("dbo.FacebookLongLivedAccessTokens", "UserId", "dbo.AspNetUsers", "Id");
        }
    }
}

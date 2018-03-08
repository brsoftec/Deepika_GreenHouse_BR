namespace GH.Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFacebookToken : DbMigration
    {
        public override void Up()
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
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.AspNetUsers", "FacebookLongLivedAccessToken", c => c.String());
            AddColumn("dbo.AspNetUsers", "FacebookLongLivedAccessTokenCreatedAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FacebookLongLivedAccessTokens", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.FacebookLongLivedAccessTokens", new[] { "UserId" });
            DropColumn("dbo.AspNetUsers", "FacebookLongLivedAccessTokenCreatedAt");
            DropColumn("dbo.AspNetUsers", "FacebookLongLivedAccessToken");
            DropTable("dbo.FacebookLongLivedAccessTokens");
        }
    }
}

namespace GH.Core.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<GH.Core.Models.GreenHouseDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(GH.Core.Models.GreenHouseDbContext context)
        {
            //config default cms layout
            var exist = context.CmsLayouts.FirstOrDefault(c => c.Default);
            var now = DateTime.Now;

            if (exist == null)
            {
                exist = new Models.CmsLayout();

                context.CmsLayouts.Add(exist);
            }


            exist.CreatedDate = now;
            exist.Default = true;
            exist.Description = "Default layout of Green House";
            exist.HtmlPreview = "<div>Content Here</div>";
            exist.ModifiedDate = now;
            exist.Name = "Default Layout";
            exist.Path = "~/Areas/Cms/Views/Shared/_Layout.cshtml";

            context.SaveChanges();
        }
    }
}

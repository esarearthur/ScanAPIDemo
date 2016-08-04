namespace API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FingerPrintDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FP_ID = c.Int(nullable: false),
                        FP_NAME = c.String(nullable: false),
                        FP_BLOB01 = c.Binary(),
                        FP_BLOB02 = c.Binary(),
                        FP_BLOB03 = c.Binary(),
                        FP_BLOB04 = c.Binary(),
                        FP_BLOB05 = c.Binary(),
                        FP_BLOB06 = c.Binary(),
                        FP_BLOB07 = c.Binary(),
                        FP_BLOB08 = c.Binary(),
                        FP_BLOB09 = c.Binary(),
                        FP_BLOB10 = c.Binary(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FingerPrintDetails");
        }
    }
}

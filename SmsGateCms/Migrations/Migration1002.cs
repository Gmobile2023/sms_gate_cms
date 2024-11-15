using ServiceStack.OrmLite;

namespace SmsGateCms.Migrations;

public class Migration1002 : MigrationBase
{
    public override void Up()
    {
        // Thêm cột mới vào bảng Message
        if (!Db.ColumnExists<Message>(x => x.RequestId))
        {
            Db.AddColumn<Message>(x => x.RequestId);
          
        }
        Db.ExecuteSql("CREATE UNIQUE INDEX ix_request_partner ON Message(_id, partner_id)");
    }

    public override void Down()
    {
        Db.ExecuteSql("DROP INDEX IF EXISTS ix_request_partner");
        if (Db.ColumnExists<Message>(x => x.RequestId))
        {
            Db.DropColumn<Message>(x => x.RequestId);
            
        }
    }
}
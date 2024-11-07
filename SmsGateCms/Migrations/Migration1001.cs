using ServiceStack;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace SmsGateCms.Migrations;

public class Migration1001 : MigrationBase
{
    public class Message : AuditBase
    {
        [AutoIncrement] public long Id { get; set; }
        [StringLength(500)] public string Sms { get; set; }
        public MessageStatus Status { get; set; } = 0;
        [StringLength(15)] public string Receiver { get; set; }
        [Reference] public MessageTemplate MessageTemplate { get; set; }

        [Ref(Model = nameof(MessageTemplate), RefId = nameof(MessageTemplate.Id),
            RefLabel = nameof(MessageTemplate.Content))]
        [References(typeof(MessageTemplate))]
        public int MessageTemplateId { get; set; }

        [Ref(Model = nameof(Partner), RefId = nameof(Partner.Id), RefLabel = nameof(Partner.PartnerCode))]
        [References(typeof(Partner))]
        public int PartnerId { get; set; }
        [Reference] public Partner Partner { get; set; }

        [Ref(Model = nameof(Provider), RefId = nameof(Provider.Id), RefLabel = nameof(Provider.ProviderCode))]
        [References(typeof(ServiceModel.Provider))]
        public int ProviderId { get; set; }
        [Reference] public ServiceModel.Provider Provider { get; set; }

        public DateTime RequestDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? ResponseDate { get; set; }
        [StringLength(30)] public string Telco { get; set; }
        [StringLength(255)] public string ResponseMassage { get; set; }
        [StringLength(250)] [Index] public string MessageId { get; set; }
    }

    public class MessageTemplate : AuditBase
    {
        [AutoIncrement]
        public int Id { get; set; }
        [StringLength(500)]
        public string Content { get; set; }
        public MessageTemplateStatus Status { get; set; } = MessageTemplateStatus.Initial;
        [Ref(Model = nameof(Partner), RefId = nameof(Partner.Id), RefLabel = nameof(Partner.PartnerCode))]
        [References(typeof(Partner))]
        public int PartnerId { get; set; }
    }

    public class Partner : AuditBase
    {
        [AutoIncrement] public int Id { get; set; }
        [StringLength(50)] public string PartnerCode { get; set; }
        [StringLength(150)] public string PartnerName { get; set; }
        [StringLength(150)] public string EmailAddress { get; set; }
        [StringLength(15)] public string PhoneNumber { get; set; }
        [StringLength(50)] public string ApiKey { get; set; }
        [StringLength(50)] public string UserName { get; set; }
        [StringLength(50)] public string Password { get; set; }
        [StringLength(200)] public string Ips { get; set; }
        public PartnerStatus Status { get; set; } = 0;
    }

    public class Provider : AuditBase
    {
        [AutoIncrement] public int Id { get; set; }
        [StringLength(50)] public string ProviderCode { get; set; }
        [StringLength(150)] public string ProviderName { get; set; }
        [StringLength(150)] public string EmailAddress { get; set; }
        [StringLength(15)] public string PhoneNumber { get; set; }
        [StringLength(50)] public string ApiKey { get; set; }
        [StringLength(50)] public string UserName { get; set; }
        [StringLength(50)] public string Password { get; set; }
        [StringLength(250)] public string ApiUrl { get; set; }
        public ProviderStatus Status { get; set; } = 0;
    }


    public override void Up()
    {
        Db.CreateTable<Partner>();
        Db.CreateTable<Provider>();
        Db.CreateTable<MessageTemplate>();
        Db.CreateTable<Message>();
    }


    public override void Down()
    {
        Db.CreateTable<Partner>();
        Db.CreateTable<Provider>();
        Db.CreateTable<MessageTemplate>();
        Db.CreateTable<Message>();
    }
}
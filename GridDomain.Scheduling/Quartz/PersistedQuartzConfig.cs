using System.Collections.Specialized;

namespace GridDomain.Scheduling.Quartz
{
    public class PersistedQuartzConfig : IQuartzConfig
    {
        public string ConnectionString => @"Data Source=(localdb);Initial Catalog=Quartz;Integrated Security = true";
        public string StorageType => "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";

        public NameValueCollection Settings => new NameValueCollection
        {
            ["quartz.jobStore.type"] = StorageType,
            ["quartz.jobStore.clustered"] = "true",
            ["quartz.jobStore.dataSource"] = "default",
            ["quartz.jobStore.tablePrefix"] = "QRTZ_",
            ["quartz.jobStore.lockHandler.type"] = "Quartz.Impl.AdoJobStore.UpdateLockRowSemaphore, Quartz",
            ["quartz.dataSource.default.connectionString"] = ConnectionString,
            ["quartz.dataSource.default.provider"] = "SqlServer-20",

            ["quartz.scheduler.instanceId"] = "AUTO"
        };
    }
}
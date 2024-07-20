using srtool;
using blueprint.core;
using blueprint.modules.account;
using blueprint.modules.auth;
using blueprint.modules.blueprint;
using blueprint.modules.blueprint.core;
using blueprint.modules.config;
using blueprint.modules.database.logic;
using blueprint.modules.drive.logic;
using blueprint.modules.node.logic;
using blueprint.modules.scheduler.logic;

namespace blueprint.modules._global
{
    public class SystemModule : Module<SystemModule>
    {
        public override async Task RunAsync()
        {
            Debug.InitConsoleSetup();
            Script.Init();
            await base.RunAsync();
            await ConfigModule.Instance.RunAsync();
            await DatabaseModule.Instance.RunAsync();
            await SchedulerModule.Instance.RunAsync();
            await AuthModule.Instance.RunAsync();
            await DriveModule.Instance.RunAsync();
            await NodeModule.Instance.RunAsync();
            await AccountModule.Instance.RunAsync();
            await BlueprintModule.Instance.RunAsync();
            await BlueprintProcessModule.Instance.RunAsync();
        }
    }
}

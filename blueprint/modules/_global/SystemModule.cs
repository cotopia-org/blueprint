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
using Newtonsoft.Json.Linq;
using blueprint.modules.blueprint.core.fields;
namespace blueprint.modules._global
{
    public class SystemModule : Module<SystemModule>
    {
        public override async Task RunAsync()
        {

            //var field = new Field();
            //field.SetValue("a", new Expression("{{2}}"));
            //field.SetValue("b", 22);
            //field.SetValue("c", 22);
            //field.SetValue("c", 44);
            //field.SetValue("u.b2", "saeed");
            //field.SetValue("c.102.a.2", "1");

            //Console.WriteLine(field.GetValue("c.102.a.2"));
            //Console.WriteLine(field.GetArraySize("c"));

            //return;

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

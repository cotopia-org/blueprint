namespace blueprint.modules.blueprint.core.test
{
    public class test
    {
        public void Run()
        {

            var blueprint = new Blueprint();

            var startNode = builtin_nodes._start_node();
            blueprint.AddBlock(startNode);
            var logNode = builtin_nodes._log_node();
            blueprint.AddBlock(logNode);

            var delayNode = builtin_nodes._delay_node();
            blueprint.AddBlock(delayNode);

            var log2Node = builtin_nodes._log_node();
            blueprint.AddBlock(log2Node);



            startNode.BindNode(logNode);
            logNode.SetField("text", "salam");
            logNode.BindNode(delayNode);

            //delayNode.SetField("delay", 4);
            delayNode.SetField("delay", new Expression("{{2+2}}"));
            delayNode.BindNode(logNode);

            var snapshot_data = blueprint.Snapshot();

            var blueprint2 = BlueprintSnapshot.LoadBlueprint(snapshot_data);

            blueprint2.FindNodeWithName("start-node").CallStart();

            Console.WriteLine(blueprint.Snapshot());
        }


    } 
}

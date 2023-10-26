using Dapr.Workflow;

namespace BasicWorkflowSamples
{
    public class ChainingWorkflow : Workflow<string, string>
    {
        public override async Task<string> RunAsync(WorkflowContext context, string input)
        {

            // if (context.IsReplaying)
            // {
            //     Console.WriteLine("Replay!");
            // } else {
            //     Console.WriteLine("First execution!");
            // }
            
            // Console.WriteLine("Activity 1");
            var message1 =  await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                input);
            
            // Console.WriteLine("Activity 2");
            var message2 = await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                message1);
            
            // Console.WriteLine("Activity 3");
            var message3 = await context.CallActivityAsync<string>(
                nameof(CreateGreetingActivity),
                message2);

            // Console.WriteLine("Finally done!");
            return message3;
        }
    }
}
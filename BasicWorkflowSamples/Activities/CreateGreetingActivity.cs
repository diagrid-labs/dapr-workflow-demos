using Dapr.Workflow;

namespace BasicWorkflowSamples
{
    public class CreateGreetingActivity : WorkflowActivity<string, string>
    {
        public override Task<string> RunAsync(WorkflowActivityContext context, string name)
        {
            Console.WriteLine($"{nameof(CreateGreetingActivity)} with input: {name}");

            var greetings = new []{"Hello", "Hi", "Hey", "Hola", "Bonjour", "Ciao", "Guten Tag", "Konnichiwa"};
            var selectedGreeting = greetings[new Random().Next(0, greetings.Length)];
            var message = $"{selectedGreeting} {name}";

            return Task.FromResult(message);
        }
    }
}
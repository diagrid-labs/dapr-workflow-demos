# Dapr workflow demos

Demos applications that use the Dapr Workflow building block.

## Prerequisites

1. [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
2. [Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/)
3. A REST client, such as [cURL](https://curl.se/), or the VSCode [REST client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client).

    > The VSCode REST client is configured as a recommended extension when opening this repo in VSCode.

## Hello world workflow sample

The Hello World Workflow sample is a very basic workflow with just one activity that returns a random greeting. The workflow takes a name as input and returns a greeting with the name as output.

```mermaid
graph TD
    A[Start]
    B[CreateGreetingActivity]
    C[End]
    A -->|"input: {name}"| B -->|"output: {greeting} {name}"| C
```

### Run the HelloWorldWorkflowSample app

1. Change to the BasicWorkflowSamples directory and build the ASP.NET app:

    ```bash
    cd BasicWorkflowSamples
    dotnet build
    ```

2. Run the app using the Dapr CLI:

    ```bash
    dapr run --app-id basic-workflows --app-port 5065 --dapr-http-port 3500 dotnet run
    ```

    > Ensure the --app-port is the same as the port specified in the launchSettings.json file.

3. Start the `HelloWorldWorkflow` via the Workflow HTTP API using cURL, or use the [basicworkflows.http](BasicWorkflowSamples/basicworkflows.http) file if you're using VSCode with the REST client:

   ```bash
   curl -i -X POST http://localhost:3500/v1.0-alpha1/workflows/dapr/HelloWorldWorkflow/1234a/start \
     -H "Content-Type: application/json" \
     -d '{ "input" : "Marc"}'
    ```

    > Note that `1234a` in the URL is the workflow instance ID. This can be any string you want.

    Expected result:

    ```json
    {
        "instance_id": "<WORKFLOW_ID>"
    }
    ```

4. Check the workflow status via Workflow HTTP API:

    ```bash
    curl -i -X GET http://localhost:3500/v1.0-alpha1/workflows/dapr/HelloWorldWorkflow/1234a/status
    ```

    Expected result:

    ```json
    {
        "WFInfo": {
            "instance_id": "<WORKFLOW_ID>"
        },
        "start_time": "2023-05-01T12:15:45Z",
        "metadata": {
            "dapr.workflow.custom_status": "",
            "dapr.workflow.input": "\"Marc"\",
            "dapr.workflow.last_updated": "2023-05-01T12:15:45Z",
            "dapr.workflow.name": "HelloWorldWorkflow",
            "dapr.workflow.output": "\"Hi Marc"\",
            "dapr.workflow.runtime_status": "COMPLETED"
        }
    }
    ```

## Chaining workflow sample

The Chaining Workflow sample is a workflow that chains three activities of the same type `CreateGreetingActivity`, each prepending a random greeting to the input. The output of the activity is used as an input for the next activity in the chain.

```mermaid
graph TD
    A[Start]
    B1[CreateGreetingActivity]
    B2[CreateGreetingActivity]
    B3[CreateGreetingActivity]
    C[End]
    A -->|"input: {name}"| B1 -->|"output/input: {greeting1} {name}"| B2
    B2 -->|"output/input: {greeting2} {greeting1} {name}"| B3
    B3 -->|"output: {greeting3} {greeting2} {greeting1} {name}"| C
```

1. Ensure that the BasicWorkflowSamples app is still running, if not change to the BasicWorkflowSamples directory, build the app, and run the app using the Dapr CLI:

    ```bash
    cd BasicWorkflowSamples
    dotnet build
    dapr run --app-id basic-workflows --app-port 5065 --dapr-http-port 3500 dotnet run
    ```

2. Start the `ChainingWorkflow` via the Workflow HTTP API using cURL, or use the [basicworkflows.http](BasicWorkflowSamples/basicworkflows.http) file if you're using VSCode with the REST client:

   ```bash
   curl -i -X POST http://localhost:3500/v1.0-alpha1/workflows/dapr/ChainingWorkflow/1234b/start \
     -H "Content-Type: application/json" \
     -d '{ "input" : "Marc"}'
    ```

    > Note that `1234b` in the URL is the workflow instance ID. This can be any string you want.

    Expected result:

    ```json
    {
        "instance_id": "<WORKFLOW_ID>"
    }
    ```

3. Check the workflow status via Workflow HTTP API:

    ```bash
    curl -i -X GET http://localhost:3500/v1.0-alpha1/workflows/dapr/ChainingWorkflow/1234b/status
    ```

    Expected result:

    ```json
    {
        "WFInfo": {
            "instance_id": "<WORKFLOW_ID>"
        },
        "start_time": "2023-05-10T12:07:38Z",
        "metadata": {
            "dapr.workflow.custom_status": "",
            "dapr.workflow.input": "\"Marc\"",
            "dapr.workflow.last_updated": "2023-05-10T12:07:38Z",
            "dapr.workflow.name": "ChainingWorkflow",
            "dapr.workflow.output": "\"Ciao Hi Bonjour Marc\"",
            "dapr.workflow.runtime_status": "COMPLETED"
        }
    }
    ```

## Fan-out / Fan-in workflow sample

The Fan-out / Fan-in Workflow sample is a workflow that fans out and schedules an activity (`CreateGreetingActivity`) for each element in the input array and waits until all of the activities are completed. The output of the workflow is an array of greetings.

```mermaid
graph TD
    A[Start]
    A1([for each name in names])
    B1[CreateGreetingActivity]
    B2[CreateGreetingActivity]
    B3[CreateGreetingActivity]
    C([Task.WhenAll])
    D[End]
    A --> |"input: [{name1}, {name2}, {name3}]"| A1
    A1 -->|"input: {name1}"| B1 -->|"output: {greeting1} {name1}"| C
    A1 -->|"input: {name2}"| B2 -->|"output: {greeting2} {name2}"| C
    A1 -->|"input: {name3}"| B3 -->|"output: {greeting3} {name3}"| C
    C -->|"output: [{greeting1} {name1}, {greeting2} {name2}, {greeting3} {name3}]"| D
```

1. Ensure that the BasicWorkflowSamples app is still running, if not change to the BasicWorkflowSamples directory, build the app, and run the app using the Dapr CLI:

    ```bash
    cd BasicWorkflowSamples
    dotnet build
    dapr run --app-id basic-workflows --app-port 5065 --dapr-http-port 3500 dotnet run
    ```

2. Start the `FanOutFanInWorkflow` via the Workflow HTTP API using cURL, or use the [basicworkflows.http](BasicWorkflowSamples/basicworkflows.http) file if you're using VSCode with the REST client:

   ```bash
   curl -i -X POST http://localhost:3500/v1.0-alpha1/workflows/dapr/FanOutFanInWorkflow/1234c/start \
     -H "Content-Type: application/json" \
     -d '{ "input" : ["Alice", "Kendall", "Sam"]"}'
    ```

    > Note that `1234c` in the URL is the workflow instance ID. This can be any string you want.

    Expected result:

    ```json
    {
        "instance_id": "<WORKFLOW_ID>"
    }
    ```

3. Check the workflow status via Workflow HTTP API:

    ```bash
    curl -i -X GET http://localhost:3500/v1.0-alpha1/workflows/dapr/FanOutFanInWorkflow/1234c/status
    ```

    Expected result:

    ```json
    {
        "WFInfo": {
            "instance_id": "<WORKFLOW_ID>"
        },
        "start_time": "2023-05-14T16:29:53Z",
        "metadata": {
            "dapr.workflow.custom_status": "",
            "dapr.workflow.input": "[\"Alice\",\"Kendall\",\"Sam\"]",
            "dapr.workflow.last_updated": "2023-05-14T16:29:54Z",
            "dapr.workflow.name": "FanOutFanInWorkflow",
            "dapr.workflow.output": "[\"Guten Tag Alice\",\"Hi Kendall\",\"Hello Sam\"]",
            "dapr.workflow.runtime_status": "COMPLETED"
        }
    ```

## ContinueAsNew sample

The ContinueAsNew sample is a workflow with a numeric input (counter) and restarts the workflow (with an updated counter) until the counter reaches 10. The workflow diagram is as follows:

```mermaid
graph TD
    A[Start]
    B[CreateGreetingActivity]
    C{counter < 10}
    D[End]
    A --> |"input: {counter}"| B
    B -->|"output: Hi {counter}"| C
    C --> |"[Yes] {counter+=1}"| A
    C -->|"[No] output: Konnichiwa 10"| D

```

1. Ensure that the BasicWorkflowSamples app is still running, if not change to the BasicWorkflowSamples directory, build the app, and run the app using the Dapr CLI:

    ```bash
    cd BasicWorkflowSamples
    dotnet build
    dapr run --app-id basic-workflows --app-port 5065 --dapr-http-port 3500 dotnet run
    ```

2. Start the `ContinueAsNewWorkflow` via the Workflow HTTP API using cURL, or use the [basicworkflows.http](BasicWorkflowSamples/basicworkflows.http) file if you're using VSCode with the REST client:

   ```bash
   curl -i -X POST http://localhost:3500/v1.0-alpha1/workflows/dapr/ContinueAsNewWorkflow/1234d/start \
     -H "Content-Type: application/json" \
     -d '{ "input" : 0}'
    ```

    > Note that `1234d` in the URL is the workflow instance ID. This can be any string you want.

    Expected result:

    ```json
    {
        "instance_id": "<WORKFLOW_ID>"
    }
    ```

3. Check the workflow status via Workflow HTTP API:

    ```bash
    curl -i -X GET http://localhost:3500/v1.0-alpha1/workflows/dapr/ContinueAsNewWorkflow/1234d/status
    ```

    Expected result:

    ```json
    {
        "WFInfo": {
            "instance_id": "<WORKFLOW_ID>"
        },
        "start_time": "2023-05-14T16:29:53Z",
        "metadata": {
            "dapr.workflow.custom_status": "",
            "dapr.workflow.input": "0",
            "dapr.workflow.last_updated": "2023-05-14T16:29:54Z",
            "dapr.workflow.name": "ContinueAsNewWorkflow",
            "dapr.workflow.output": "\"Konnichiwa 10\"",
            "dapr.workflow.runtime_status": "COMPLETED"
        }
    ```

## Timer sample

The Timer sample is a workflow with a TimerWorkflowInput object that contains a name and a date. If the date from the input is larger that the current date, a timer is started and the workflow will only continue once the timer has completed.:

```mermaid
graph TD
    A[Start]
    B{input date > current date}
    C([CreateTimer])
    D[CreateGreetingActivity]
    E[End]
    A --> |"input: {name}"| B
    B -->|"[Yes]"| C
    C -->|"Wait"| A
    B -->|"[No] input: {name}"| D
    D -->|"output: {greeting} {name}"| E

```

1. Ensure that the BasicWorkflowSamples app is still running, if not change to the BasicWorkflowSamples directory, build the app, and run the app using the Dapr CLI:

    ```bash
    cd BasicWorkflowSamples
    dotnet build
    dapr run --app-id basic-workflows --app-port 5065 --dapr-http-port 3500 dotnet run
    ```

2. Start the `TimerWorkflow` via the Workflow HTTP API using cURL, or use the [basicworkflows.http](BasicWorkflowSamples/basicworkflows.http) file if you're using VSCode with the REST client:

   ```bash
   curl -i -X POST http://localhost:3500/v1.0-alpha1/workflows/dapr/TimerWorkflow/1234e/start \
     -H "Content-Type: application/json" \
     -d '{ "input" : "Marc"}'
    ```

    > Note that `1234e` in the URL is the workflow instance ID. This can be any string you want.

    Expected result:

    ```json
    {
        "instance_id": "<WORKFLOW_ID>"
    }
    ```

3. Check the workflow status via Workflow HTTP API:

    ```bash
    curl -i -X GET http://localhost:3500/v1.0-alpha1/workflows/dapr/TimerWorkflow/1234e/status
    ```

    Expected result:

    ```json
    {
        "WFInfo": {
            "instance_id": "<WORKFLOW_ID>"
        },
        "start_time": "2023-05-14T16:29:53Z",
        "metadata": {
            "dapr.workflow.custom_status": "",
            "dapr.workflow.input": "Marc",
            "dapr.workflow.last_updated": "2023-05-14T16:29:54Z",
            "dapr.workflow.name": "TimerWorkflow",
            "dapr.workflow.output": "\"Konnichiwa Marc\"",
            "dapr.workflow.runtime_status": "COMPLETED"
        }
    ```

## CheckoutWorkflow sample

```mermaid
flowchart LR
A[CheckoutService]
B[PaymentService]
C[(Inventory DB)]
A --> B
A --> C
```

The Checkout Workflow sample is a workflow that processes an order. The workflow takes an `OrderItem` as input and returns a `CheckoutResult` as output. The workflow uses these activities:

- `NotifyActivity`: Notifies the customer of the progress of the order.
- `CheckInventoryActivity`: Checks if the inventory is sufficient.
- `ProcessPaymentActivity`: Processes the payment, by calling another Dapr service (PaymentService).
- `UpdateInventoryActivity`: Updates the inventory after checking it again.
- `RefundPaymentActivity`: Refunds the payment if the `UpdateInventoryActivity` throws an exception.

```mermaid
graph TD
    A[Start]
    B[NotifyActivity]
    C[CheckInventoryActivity]
    X{Sufficient Inventory?}
    D[ProcessPaymentActivity]
    E[UpdateInventoryActivity]
    F[RefundPaymentActivity]
    BB[NotifyActivity]
    BBB[NotifyActivity]
    XX{Sufficient Inventory?}
    Z[End]
    A --> |OrderItem| B --> C
    C --> X
    X -->|Yes| D
    X -->|"No - CheckoutResult(Processed:false)"| Z
    D --> E --> XX
    XX -->|Yes| BB --> |"CheckoutResult(Processed:true)"| Z
    XX -->|No| BBB --> F --> |"CheckoutResult(Processed:false)"| Z
```

The `CheckInventoryActivity` and `UpdateInventoryActivity` classes use [Dapr's state management building block](https://docs.dapr.io/developing-applications/building-blocks/state-management/) to manage the inventory in a Redis state store.

The `ProcessPaymentActivity` and `RefundPaymentActivity` call out to the PaymentService.

Next to the workflow, this application has an `InventoryController` with the following endpoints:

- `GET http://localhost:5064/inventory`: retrieves the inventory
- `DELETE http://localhost:5064/inventory`: clears the inventory
- `POST http://localhost:5064/inventory/restock`: restocks the inventory

The `InventoryController` also uses Dapr's state management building block.

### Run the PaymentService app

The CheckoutWorkflowSample app relies on the PaymentService app to process the payment. The PaymentService app is a small ASP.NET app that exposes two endpoints:

- `/pay`: processes the payment
- `/refund`: refunds the payment

This service will be started first before the CheckoutWorkflowSample app is started.

1. Change to the PaymentService directory and build the ASP.NET app:

    ```bash
    cd PaymentService
    dotnet build
    ```

2. Run the app using the Dapr CLI:

    ```bash
    dapr run --app-id payment --app-port 5063 --dapr-http-port 3501 dotnet run
    ```

### Run the CheckoutWorkflowSample app

1. Change to the CheckoutService directory and build the ASP.NET app:

    ```bash
    cd CheckoutService
    dotnet build
    ```

2. Run the app using the Dapr CLI:

    ```bash
    dapr run --app-id checkout --app-port 5064 --dapr-http-port 3500 --resources-path ./Resources dotnet run
    ```

    > Ensure the --app-port is the same as the port specified in the launchSettings.json file.

3. Check the inventory using cURL, or use the [checkout.http](CheckoutService/checkout.http) file if you're using VSCode with the REST client:

    ```bash
    curl -X POST http://localhost:5064/inventory/restock
    ```

    If the quantity of the items is not 0, clear the inventory by running:

    ```bash
    curl -X POST http://localhost:5064/inventory/clear
    ```

4. Try ordering 100 paperclips while the inventory is not sufficient. Start the `CheckoutWorkflow` via the Workflow HTTP API:

   ```bash
   curl -i -X POST http://localhost:3500/v1.0-alpha1/workflows/dapr/CheckoutWorkflow/1234e/start \
     -H "Content-Type: application/json" \
     -d '{ "input" : {"Name": "Paperclips", "Quantity": 100}}'
    ```

    > Note that `1234e` in the URL is the workflow instance ID. This can be any string you want.

    Expected result:

    ```json
    {
        "instance_id": "<WORKFLOW_ID>"
    }
    ```

    > Pay attention to the console output. A message will appear that indicates the inventory is insufficient.

5. Check the workflow status via Workflow HTTP API:

    ```bash
    curl -i -X GET http://localhost:3500/v1.0-alpha1/workflows/dapr/CheckoutWorkflow/1234e/status
    ```

    Expected result:

    ```json
    {
        "WFInfo": {
            "instance_id": "<WORKFLOW_ID>"
        },
        "start_time": "2023-05-01T12:15:45Z",
        "metadata": {
            "dapr.workflow.custom_status": "\"Stopped order process due to insufficient inventory.\"",
            "dapr.workflow.input": "{\"Name\":\"Paperclips\",\"Quantity\":100}",
            "dapr.workflow.last_updated": "2023-05-01T12:15:45Z",
            "dapr.workflow.name": "CheckoutWorkflow",
            "dapr.workflow.output": "{\"Processed\":false}",
            "dapr.workflow.runtime_status": "COMPLETED"
        }
    }
    ```

    > Depending on how quick the status is retrieved after starting the workflow, the `dapr.workflow.runtime_status` could still be `"RUNNING"`. Repeat the GET status request until the status is `"COMPLETED"`.

6. Restock the inventory:

    ```bash
    curl -X POST http://localhost:5064/inventory/restock
    ```

    Expected result: `HTTP 200 OK`

7. Try ordering paperclips again, now within the limits of the inventory. Start the `CheckoutWorkflow` via the Workflow HTTP API:

    ```bash
    curl -i -X POST http://localhost:3500/v1.0-alpha1/workflows/dapr/CheckoutWorkflow/1234d/start \
     -H "Content-Type: application/json" \
     -d '{ "input" : {"Name": "Paperclips", "Quantity": 100}}'
    ```

    > Note that `1234d` in the URL is the workflow instance ID. This can be any string you want.

    Expected result:

    ```json
    {
        "instance_id": "<WORKFLOW_ID>"
    }
    ```

    > Pay attention to the console output. Messages will appear that indicate the inventory is sufficient and payment has been processed successfully.

8. Check the workflow status via Workflow HTTP API:

    ```bash
    curl -i -X GET http://localhost:3500/v1.0-alpha1/workflows/dapr/CheckoutWorkflow/1234d/status
    ```

    Expected result:

    ```json
    {
        "WFInfo": {
            "instance_id": "<WORKFLOW_ID>"
        },
        "start_time": "2023-05-01T12:22:25Z",
        "metadata": {
            "dapr.workflow.custom_status": "",
            "dapr.workflow.input": "{\"Name\":\"Paperclips\",\"Quantity\":100}",
            "dapr.workflow.last_updated": "2023-05-01T12:22:37Z",
            "dapr.workflow.name": "CheckoutWorkflow",
            "dapr.workflow.output": "{\"Processed\":true}",
            "dapr.workflow.runtime_status": "COMPLETED"
        }
    }
    ```

9. Inspect the logs in ZipKin: [`localhost:9411/zipkin`](http://localhost:9411/zipkin). Find the entry marked `checkout:create_orchestration||checkoutworkflow` and show the details. You'll now see a timeline of the workflow at the top, and the activities underneath.

    ![Checkout workflow in Zipkin](images/checkout_zipkin.png)

## Resources

1. [Dapr Workflow overview](https://docs.dapr.io/developing-applications/building-blocks/workflow/workflow-overview/).
2. [How to: Author and manage Dapr Workflow in the .NET SDK](https://docs.dapr.io/developing-applications/sdks/dotnet/dotnet-workflow/dotnet-workflow-howto/)

## More information

Any questions or comments about this sample? Join the [Dapr discord](https://bit.ly/dapr-discord) and post a message the `#workflow` channel.
Have you made something with Dapr? Post a message in the `#show-and-tell` channel, we love to see your creations!

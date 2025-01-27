# WebSharper Payment Request API Binding

This repository provides an F# [WebSharper](https://websharper.com/) binding for the [Payment Request API](https://developer.mozilla.org/en-US/docs/Web/API/Payment_Request_API). By leveraging this API, developers can simplify and standardize payment flows in web applications, ensuring seamless integration with WebSharper projects.

## Repository Structure

The repository consists of two main projects:

1. **Binding Project**:
   - Contains the F# WebSharper binding for the Payment Request API.

2. **Sample Project**:
   - Demonstrates how to use the Payment Request API with WebSharper syntax.
   - Includes a GitHub Pages demo: [View Demo](https://dotnet-websharper.github.io/PaymentRequest/).

## Features

- WebSharper bindings for the Payment Request API.
- Simplified integration of payment workflows.
- Example usage through the Sample project.
- Hosted demo to explore API functionality.

## Installation and Building

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.
- Node.js and npm (for building web assets).
- WebSharper tools.

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/dotnet-websharper/PaymentRequest.git
   cd PaymentRequest
   ```

2. Build the Binding Project:

   ```bash
   dotnet build WebSharper.PaymentRequest/WebSharper.PaymentRequest.fsproj
   ```

3. Build and Run the Sample Project:

   ```bash
   cd WebSharper.PaymentRequest.Sample
   dotnet build
   dotnet run
   ```

4. Open the hosted demo to see the Sample project in action:
   [https://dotnet-websharper.github.io/PaymentRequest/](https://dotnet-websharper.github.io/PaymentRequest/)

## Why Use the Payment Request API

The Payment Request API streamlines the checkout process in web applications by providing a consistent user interface for payment methods. Key benefits include:

1. **Simplified Payment Flow**: Reduces the number of steps required to complete a payment.
2. **Improved User Experience**: Offers a browser-native, standardized interface for various payment methods.
3. **Enhanced Security**: Supports secure payment methods like tokenized credit cards and digital wallets.
4. **Cross-Browser Compatibility**: Provides a consistent experience across supported browsers.

Integrating the Payment Request API with WebSharper allows developers to build secure, user-friendly payment flows in F# web applications.

## How to Use the Payment Request API

### Example Usage

The Payment Request API allows developers to create and handle payment requests. Below is an example of how to use it with WebSharper:

```fsharp
namespace WebSharper.PaymentRequest.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.PaymentRequest

// Define the connection to the HTML template
// `IndexTemplate` binds to `wwwroot/index.html` and enables dynamic updates to the UI
type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

[<JavaScript>]
module Client =    
    let handlePaymentRequest() = promise {        

        // Define the payment method (e.g., Google Pay)
        let paymentMethodData = PaymentMethodData.create()

        // Define the payment details (e.g., total cost)
        let paymentDetails = PaymentDetails.create()

        try
            // Create a PaymentRequest instance
            let paymentRequest = new PaymentRequest([| paymentMethodData |], paymentDetails)

            Console.Log("PaymentRequest JSON:", paymentRequest)

            // Show the payment UI
            let! paymentResponse = paymentRequest.Show()

            Console.Log("Payment successful!", paymentResponse)

            // Complete the payment process
            do! paymentResponse.Complete(PaymentCompletionResult.Success)
            JS.Alert("Payment completed successfully!")
        with ex ->
            Console.Error("Payment failed:", ex.Message)
            JS.Alert("Payment failed or was cancelled.")
    }

    [<SPAEntryPoint>]
    let Main () =

        IndexTemplate.Main()
            // Binds the payment logic to the Pay button in the template, triggering the handlePaymentRequest function when clicked
            .Pay(fun _ -> 
                async {
                    do! handlePaymentRequest() |> Promise.AsAsync
                }
                |> Async.StartImmediate
            )
            .Doc()
        |> Doc.RunById "main"
```

This example demonstrates how to define payment methods and details, create a payment request, and handle the response.

For a complete implementation, refer to the [Sample Project](https://dotnet-websharper.github.io/PaymentRequest/).

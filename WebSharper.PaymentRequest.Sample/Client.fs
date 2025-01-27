namespace WebSharper.PaymentRequest.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.PaymentRequest

type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

type AllowedPaymentMethodsParameter = {
    allowedAuthMethods: string array
    allowedCardNetworks: string array
}

type TokenizationSpecificationParameter = {
    gateway: string
    gatewayMerchantId: string
}

type TokenizationSpecification= {
    ``type``: string
    parameters: TokenizationSpecificationParameter
}

type AllowedPaymentMethods = {
    ``type``: string
    parameters: AllowedPaymentMethodsParameter
    tokenizationSpecification: TokenizationSpecification
}

type TransactionInfo = {
    totalPriceStatus: string
    totalPrice: string
    currencyCode: string
}

type PaymentMethodsData = {
    environment: string
    apiVersion: int
    apiVersionMinor: int
    allowedPaymentMethods: AllowedPaymentMethods array
    transactionInfo: TransactionInfo
}

[<JavaScript>]
module PaymentMethodData = 
    let authMethods = [| "PAN_ONLY"; "CRYPTOGRAM_3DS" |]
    let cardNetworks = [| "VISA"; "MASTERCARD" |]

    let allowedPaymentMethodsParameter = {
        allowedAuthMethods = authMethods
        allowedCardNetworks = cardNetworks
    }

    let tokenizationSpecificationParameter = {
        gateway = "example"
        gatewayMerchantId = "exampleMerchantId"
    }

    let tokenizationSpecification = {
        ``type`` = "PAYMENT_GATEWAY"
        parameters = tokenizationSpecificationParameter
    }

    let allowedPaymentMethods = {
        ``type`` = "CARD"
        parameters = allowedPaymentMethodsParameter
        tokenizationSpecification = tokenizationSpecification
    }

    let transactionInfo = {
        totalPriceStatus = "FINAL"
        totalPrice = "1.00"
        currencyCode = "USD"
    }

    let create () =
        let paymentMethodData = new PaymentMethodData()
        paymentMethodData.SupportedMethods <- "https://google.com/pay"
        paymentMethodData.Data <- {
            environment = "TEST"
            apiVersion = 2
            apiVersionMinor = 0
            allowedPaymentMethods = [| allowedPaymentMethods |]
            transactionInfo = transactionInfo
        }
        Console.Log("Payment Method Data:", paymentMethodData)
        paymentMethodData

[<JavaScript>]
module PaymentDetails = 
    let create () = 
        let paymentDetails = new PaymentDetails()

        paymentDetails.Total <- new PaymentItem(
            label = "Total",
            amount = new Amount(
                currency = "USD",
                value = "1.00"
            )
        )
        paymentDetails

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
            .Pay(fun _ -> 
                async {
                    do! handlePaymentRequest() |> Promise.AsAsync
                }
                |> Async.StartImmediate
            )
            .Doc()
        |> Doc.RunById "main"

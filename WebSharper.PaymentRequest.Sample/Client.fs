namespace WebSharper.PaymentRequest.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.PaymentRequest

// The Google Pay payment method payload. It becomes the `data` member of the
// `PaymentMethodData` dictionary, so the field names stay lowercase to match
// the Google Pay JSON schema.

type AllowedPaymentMethodsParameter = {
    allowedAuthMethods: string array
    allowedCardNetworks: string array
}

type TokenizationSpecificationParameter = {
    gateway: string
    gatewayMerchantId: string
}

type TokenizationSpecification = {
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
module Client =
    // The template is loaded from the DOM, so editing index.html and refreshing
    // the browser is enough; a rebuild is only needed when holes change.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let status = Var.Create "Ready. Start a payment to open the browser's payment sheet."
    let preview = Var.Create "No payment request has been created yet."

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

    let methodData : PaymentMethodsData = {
        environment = "TEST"
        apiVersion = 2
        apiVersionMinor = 0
        allowedPaymentMethods = [| allowedPaymentMethods |]
        transactionInfo = transactionInfo
    }

    /// Build a `PaymentMethodData` dictionary for Google Pay.
    let createPaymentMethod () =
        let method = new PaymentMethodData()
        method.SupportedMethods <- "https://google.com/pay"
        method.Data <- methodData
        method

    /// Build a `PaymentDetails` dictionary with the amount to charge.
    let createPaymentDetails () =
        let details = new PaymentDetails()
        details.Total <- new PaymentItem(
            label = "Total",
            amount = new Amount(
                currency = "USD",
                value = "1.00"
            )
        )
        details

    /// Build a `PaymentRequest` for Google Pay with a 1.00 USD total.
    let createPaymentRequest () =
        new PaymentRequest([| createPaymentMethod () |], createPaymentDetails ())

    let checkSupport () =
        promise {
            try
                status := "Checking whether the browser supports this payment method..."
                let request = createPaymentRequest ()
                let! supported = request.CanMakePayment()
                if supported then
                    status := "Google Pay is available in this browser."
                    preview := "Payment request created and the browser has a matching payment method."
                else
                    status := "Google Pay is not available in this browser."
                    preview := "Payment request created, but no matching payment method is configured."
            with ex ->
                status := $"Error: {ex.Message}"
                preview := "Could not create a payment request."
        }

    let startPayment () =
        promise {
            try
                status := "Opening the payment sheet..."
                preview := "Payment request created; waiting for the user to confirm."
                let request = createPaymentRequest ()

                let! response = request.Show()
                status := $"Payment completed ({response.MethodName})."
                preview := "Payment response received; the browser handled the payment details."

                do! response.Complete(PaymentCompletionResult.Success)
            with ex ->
                status := $"Payment failed or was cancelled: {ex.Message}"
                preview := "No payment was completed."
        }

    [<SPAEntryPoint>]
    let Main () =
        IndexTemplate()
            .startPayment(fun _ -> startPayment () |> ignore)
            .checkSupport(fun _ -> checkSupport () |> ignore)
            .status(status.V)
            .preview(preview.V)
            .Bind()

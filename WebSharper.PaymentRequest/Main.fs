namespace WebSharper.PaymentRequest

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =

    let BinaryData = T<ArrayBuffer> + T<DataView>

    module Enum = 
        let PaymentShippingType = 
            Pattern.EnumStrings "PaymentShippingType" [
                "shipping"
                "delivery"
                "pickup"
            ]

        let PaymentCompletionResult = 
            Pattern.EnumStrings "PaymentCompletionResult" [
                "success"
                "fail"
                "unknown"
            ]

    let Amount = 
        Pattern.Config "Amount" {
            Required = [
                "currency", T<string>
                "value", T<string>
            ]
            Optional = []
        }

    let PaymentItem =
        Pattern.Config "PaymentItem" {
            Required = [
                "amount", Amount.Type
                "label", T<string>
            ]
            Optional = [
                "pending", T<bool>
            ]
        }

    let PaymentDetailsModifier = 
        Pattern.Config "PaymentDetailsModifier" {
            Required = []
            Optional = [
                "total", PaymentItem.Type
                "additionalDisplayItems", !| PaymentItem
                "data", T<obj>
                "supportedMethods", !| T<string>
            ]
        }

    let PaymentUpdateDetails = 
        Pattern.Config "PaymentUpdateDetails" {
            Required = []
            Optional = [
                "displayItems", !| PaymentItem
                "modifiers", !| PaymentDetailsModifier
                "total", PaymentItem.Type 
            ]
        }

    let PaymentRequestUpdateEvent =
        Class "PaymentRequestUpdateEvent"
        |=> Inherits T<Dom.Event>
        |+> Static [
            Constructor T<unit>
        ]
        |+> Instance [
            "updateWith" => PaymentUpdateDetails?details ^-> T<unit>
        ]

    let PaymentMethodChangeOptions = 
        Pattern.Config "PaymentMethodChangeOptions" {
            Required = []
            Optional = [
                "methodName", T<string>
                "methodDetails", T<obj>
            ]
        }

    let PaymentMethodChangeEvent = 
        Class "PaymentMethodChangeEvent"
        |=> Inherits PaymentRequestUpdateEvent
        |+> Static [
            Constructor (T<string>?``type`` * !?PaymentMethodChangeOptions?options)
        ]
        |+> Instance [
            "methodDetails" =? T<obj>
            "methodName" =? T<string>
        ]

    let PaymentMethodData = 
        Pattern.Config "PaymentMethodData" {
            Required = []
            Optional = [
                "supportedMethods", T<string>
                "data", T<obj>
            ]
        }

    let PaymentDetails =
        Pattern.Config "PaymentDetails" {
            Required = []
            Optional = [
                "total", PaymentItem.Type
                "displayItems", !| PaymentItem.Type
                "shippingOptions", !| T<obj>
                "modifiers", !| PaymentDetailsModifier.Type
                "id", T<string>
            ]
        }

    let PaymentOptions =
        Pattern.Config "PaymentOptions" {
            Required = [
                "requestPayerName", T<bool>
                "requestPayerEmail", T<bool>
                "requestPayerPhone", T<bool>
                "requestShipping", T<bool>
                "shippingType", Enum.PaymentShippingType.Type
            ]
            Optional = []
        }

    let PaymentResponseErrorFields = 
        Pattern.Config "PaymentResponseErrorFields" {
            Required = []
            Optional = [
                "error", T<string>
                "paymentMethod", T<obj> 
            ]
        }

    let PaymentResponse =
        Class "PaymentResponse"
        |=> Inherits T<Dom.EventTarget>
        |+> Instance [
            "details" =? T<obj>
            "methodName" =? T<string>
            "payerEmail" =? T<string>
            "payerName" =? T<string>
            "payerPhone" =? T<string>
            "requestId" =? T<string>
            "shippingAddress" =? T<obj>
            "shippingOption" =? T<string>

            "complete" => !?Enum.PaymentCompletionResult?result ^-> T<Promise<unit>>
            "retry" => !?PaymentResponseErrorFields?errorFields ^-> T<Promise<unit>>
            "toJSON" => T<unit> ^-> T<obj>
        ]

    let PaymentRequest = 
        Class "PaymentRequest"
        |+> Static [
            Constructor ((!|PaymentMethodData)?methodData * PaymentDetails?Details * !?PaymentOptions?options)
        ]
        |+> Instance [
            "show" => !?PaymentUpdateDetails?details ^-> T<Promise<_>>[PaymentResponse]
            "abort" => T<unit> ^-> T<unit>
            "canMakePayment" => T<unit> ^-> T<Promise<bool>>
        ]

    let SecurePaymentInstrument = 
        Pattern.Config "SecurePaymentInstrument" {
            Required = [
                "displayName", T<string>
                "icon", T<string>
            ]
            Optional = [
                "iconMustBeShown", T<bool>
            ]
        }

    let SecurePaymentConfirmationRequest = 
        Pattern.Config "SecurePaymentConfirmationRequest" {
            Required = [
                "challenge", BinaryData
                "credentialIds", !| BinaryData
                "instrument", SecurePaymentInstrument.Type
                "rpId", T<string>
            ]
            Optional = [
                "extensions", T<obj>
                "locale", !| T<string>
                "payeeName", T<string>
                "payeeOrigin", T<string>
                "showOptOut", T<bool>
                "timeout", T<int>
            ]
        }

    let Assembly =
        Assembly [
            Namespace "WebSharper.PaymentRequest" [
                SecurePaymentConfirmationRequest
                SecurePaymentInstrument
                PaymentRequest
                PaymentResponse
                PaymentOptions
                PaymentDetails
                PaymentMethodData
                PaymentMethodChangeEvent
                PaymentMethodChangeOptions
                PaymentRequestUpdateEvent
                PaymentUpdateDetails
                PaymentDetailsModifier
                PaymentItem
                PaymentResponseErrorFields
                Amount

                Enum.PaymentShippingType
                Enum.PaymentCompletionResult
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()

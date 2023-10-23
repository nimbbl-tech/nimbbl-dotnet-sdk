namespace Nimbbl.Sdk.Rest.Payments;

public class PaymentModed
{
    public string PaymentMode { get; init; }

    public class ExtraInfo
    {
        public long EtaCompletion { get; init; }
        public string AdditionalCharges { get; init; }
        public string? AutoDebitFlowPossible { get; init; }
        public string?  Next { get; init; }

    } 
    //Request 
 // "order_id": "o_DZ0X4AgEwnYNr0bY",
 // "payment_mode": "Google Pay",
 // "payment_mode_id": null,
 // "bank": null,
 // "canMakePayment": true,
 // "app_present": null,
 // "callback_url": null,
 // "user_agent": "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.41 Mobile Safari/537.36 Edg/101.0.1210.32",
 // "ipAddress": "49.207.210.62",
 // "fingerPrint": "71902f5855d5b4d46326813b24c19b0b"
    
    
    //Response
    // {
    //     "payment_mode": "Credit Card",
    //     "extra_info": {
    //         "eta_completion": 60,
    //         "additional_charges": "0"
    //     }
    // },
    // {
    //     "payment_mode": "Debit Card",
    //     "extra_info": {
    //         "eta_completion": 60,
    //         "additional_charges": "0"
    //     }
    // },
    // {
    //     "payment_mode": "Netbanking",
    //     "extra_info": {
    //         "eta_completion": 60,
    //         "additional_charges": "0"
    //     }
    // },
    // {
    //     "payment_mode": "Wallet",
    //     "extra_info": {
    //         "eta_completion": 60,
    //         "additional_charges": "0"
    //     }
    // },
    // {
    //     "payment_mode": "Prepaid Card",
    //     "extra_info": {
    //         "eta_completion": 60,
    //         "additional_charges": "0"
    //     }
    // },
    // {
    //     "payment_mode": "Cred",
    //     "extra_info": {
    //         "auto_debit_flow_possible": "no",
    //         "eta_completion": 5,
    //         "additional_charges": "0",
    //         "next": "initiatePayment"
    //     }
    // },
    // {
    //     "payment_mode": "Simpl",
    //     "extra_info": {
    //         "auto_debit_flow_possible": "no",
    //         "eta_completion": 5,
    //         "additional_charges": "0",
    //         "next": "initiatePayment"
    //     }
    // },
    // {
    //     "payment_mode": "Lazypay",
    //     "extra_info": {
    //         "auto_debit_flow_possible": "no",
    //         "eta_completion": 15,
    //         "additional_charges": "0",
    //         "next": "initiatePayment"
    //     }
    // },
    // {
    //     "payment_mode": "Olamoney",
    //     "extra_info": {
    //         "auto_debit_flow_possible": "no",
    //         "eta_completion": 5,
    //         "additional_charges": "0",
    //         "next": "initiatePayment"
    //     }
    // },
    // {
    //     "payment_mode": "UPI",
    //     "extra_info": {
    //         "user_vpa": "Prashant Chaudhari",
    //         "vpa_id": "9635020816@paytm",
    //         "vpa_provider": "PayTM",
    //         "additional_charges": "0",
    //         "eta_completion": 60,
    //         "server_intent": true
    //     }
    // },
    // {
    //     "payment_mode": "UPI",
    //     "extra_info": {
    //         "user_vpa": "PRASHANT CHOUDHARY",
    //         "vpa_id": "9635020816@axisb",
    //         "vpa_provider": "CRED",
    //         "additional_charges": "0",
    //         "eta_completion": 60,
    //         "server_intent": true
    //     }
    // },
    // {
    //     "payment_mode": "UPI",
    //     "extra_info": {
    //         "user_vpa": "PRASHANT CHOUDHARY",
    //         "vpa_id": "9635020816@upi",
    //         "vpa_provider": "BHIM",
    //         "additional_charges": "0",
    //         "eta_completion": 60,
    //         "server_intent": true
    //     }
    // },
    // {
    //     "payment_mode": "UPI",
    //     "extra_info": {
    //         "user_vpa": "PRASHANT CHOUDHARY",
    //         "vpa_id": "9635020816@axl",
    //         "vpa_provider": "PhonePe",
    //         "additional_charges": "0",
    //         "eta_completion": 60,
    //         "server_intent": true,
    //         "is_transacted": null
    //     }
    // },
    // {
    //     "payment_mode": "UPI",
    //     "extra_info": {
    //         "additional_charges": "0",
    //         "eta_completion": 60,
    //         "server_intent": true
    //     }
    // }
}
using System.Text.Json;
using Nimbbl.Sdk.Rest.Api;
using Nimbbl.Sdk.Rest.Exception;

namespace Examples;

/// <summary>
/// Address Examples
/// </summary>
public static class AddressExamples
{
    /// <summary>
    /// List Addresses - Function to be called from Program.cs or standalone
    /// </summary>
    public static async Task RunAllExamples(NimbblApi api)
    {
        Helpers.PrintHeader("=== Addresses API Examples ===");
        
        Helpers.PrintStep(1, "List Addresses");
        await ListAddressesExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(2, "Create Address");
        await CreateAddressExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(3, "Update Address");
        await UpdateAddressExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(4, "Delete Address");
        await DeleteAddressExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(5, "Get Address by ID");
        await GetAddressByIdExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(6, "Import Addresses");
        await ImportAddressesExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(7, "Check Address Eligibility");
        await CheckAddressEligibilityExample(api);
        
        Console.WriteLine();
        Helpers.PrintStep(8, "Link Address with Order");
        await LinkAddressWithOrderExample(api);
        
        Console.WriteLine("\nFor more information, see: https://nimbbl.biz/docs/category/api-reference/addresses/\n");
    }

    public static async Task ListAddressesExample(NimbblApi api)
    {
        try
        {
            var token = Helpers.GetInput("Enter Order Token: ", false);
            if (string.IsNullOrWhiteSpace(token))
            {
                Helpers.PrintError("Order Token is required.\n");
                return;
            }
            
            api.SetBearerToken(token);
            
            var data = new Dictionary<string, object?>();
            
            // user_id - required for listing addresses
            var userId = Helpers.GetInput("Enter User ID: ", false);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                data["user_id"] = userId;
            }
            
            // amount - order amount to calculate shipping charges
            var amountStr = Helpers.GetInput("Enter Order Amount (for shipping calculation, optional): ", false);
            if (!string.IsNullOrWhiteSpace(amountStr) && decimal.TryParse(amountStr, out var amount))
            {
                data["amount"] = amount;
            }
            
            // currency - currency code in ISO-4217 format
            var currency = Helpers.GetInput("Enter Currency (ISO-4217 format, e.g., INR, optional): ", false);
            if (!string.IsNullOrWhiteSpace(currency))
            {
                data["currency"] = currency.ToUpper();
            }
            
            if (data.Count == 0)
            {
                Helpers.PrintWarning("No query parameters provided. At least one parameter (user_id, amount, currency) is recommended.\n");
                var continueChoice = Helpers.GetInput("Continue anyway? (y/n): ", false);
                if (continueChoice?.ToLower() != "y")
                {
                    return;
                }
            }
            
            var result = await api.Addresses().ListAddressesAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Addresses retrieved successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task CreateAddressExample(NimbblApi api)
    {
        try
        {
            var token = Helpers.GetInput("Enter Order Token: ", false);
            if (string.IsNullOrWhiteSpace(token))
            {
                Helpers.PrintError("Order Token is required.\n");
                return;
            }
            
            api.SetBearerToken(token);
            
            var userId = Helpers.GetInput("Enter User ID (optional): ", false);
            Helpers.PrintInfo("Enter address details:\n");
            
            // Required fields
            var firstName = Helpers.GetInput("First Name: ");
            var lastName = Helpers.GetInput("Last Name: ");
            var address1 = Helpers.GetInput("Address Line 1: ");
            var area = Helpers.GetInput("Area/Locality: ");
            var city = Helpers.GetInput("City: ");
            var state = Helpers.GetInput("State: ");
            var pincode = Helpers.GetInput("Pincode: ");
            var addressType = Helpers.GetInput("Address Type (home/office/etc): ");
            
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || 
                string.IsNullOrWhiteSpace(address1) || string.IsNullOrWhiteSpace(area) || 
                string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(state) || 
                string.IsNullOrWhiteSpace(pincode) || string.IsNullOrWhiteSpace(addressType))
            {
                Helpers.PrintError("First Name, Last Name, Address Line 1, Area, City, State, Pincode, and Address Type are required.\n");
                return;
            }
            
            // Build address object
            var addressData = new Dictionary<string, object?>
            {
                ["first_name"] = firstName,
                ["last_name"] = lastName,
                ["address_1"] = address1,
                ["area"] = area,
                ["city"] = city,
                ["state"] = state,
                ["pincode"] = pincode,
                ["address_type"] = addressType
            };
            
            // Optional fields
            var street = Helpers.GetInput("Street (optional): ", false);
            if (!string.IsNullOrWhiteSpace(street)) addressData["street"] = street;
            
            var landmark = Helpers.GetInput("Landmark (optional): ", false);
            if (!string.IsNullOrWhiteSpace(landmark)) addressData["landmark"] = landmark;
            
            var label = Helpers.GetInput("Label (optional): ", false);
            if (!string.IsNullOrWhiteSpace(label)) addressData["label"] = label;
            
            var country = Helpers.GetInput("Country (optional, default: India): ", false);
            addressData["country"] = country ?? "India";
            
            var linkAs = Helpers.GetInput("Link As (shipping/billing, optional): ", false);
            if (!string.IsNullOrWhiteSpace(linkAs) && 
                (linkAs.ToLower() == "shipping" || linkAs.ToLower() == "billing"))
            {
                addressData["link_as"] = linkAs.ToLower();
            }
            
            // Build request body with addresses array
            var data = new Dictionary<string, object?>
            {
                ["addresses"] = new[] { addressData }
            };
            
            if (!string.IsNullOrWhiteSpace(userId))
            {
                data["user_id"] = userId;
            }
            
            // Amount and currency for shipping calculation
            var amountStr = Helpers.GetInput("Order Amount (for shipping calculation, default: 5000): ", false);
            var currency = Helpers.GetInput("Currency (default: INR): ", false);
            
            data["amount"] = !string.IsNullOrWhiteSpace(amountStr) && decimal.TryParse(amountStr, out var amt) ? amt : 5000m;
            data["currency"] = currency ?? "INR";
            
            var result = await api.Addresses().CreateAddressAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Address created successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task UpdateAddressExample(NimbblApi api)
    {
        try
        {
            var token = Helpers.GetInput("Enter Order Token: ", false);
            if (string.IsNullOrWhiteSpace(token))
            {
                Helpers.PrintError("Order Token is required.\n");
                return;
            }
            
            api.SetBearerToken(token);
            
            var addressId = Helpers.GetInput("Enter Address ID: ");
            if (string.IsNullOrWhiteSpace(addressId))
            {
                Helpers.PrintError("Address ID is required.\n");
                return;
            }
            
            Helpers.PrintInfo("Enter address fields to update (press Enter to skip)\n");
            var data = new Dictionary<string, object?>();
            
            var line1 = Helpers.GetInput("Address Line 1: ", false);
            if (!string.IsNullOrWhiteSpace(line1)) data["address_1"] = line1;
            
            var city = Helpers.GetInput("City: ", false);
            if (!string.IsNullOrWhiteSpace(city)) data["city"] = city;
            
            var result = await api.Addresses().UpdateAddressAsync(addressId, data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Address updated successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task DeleteAddressExample(NimbblApi api)
    {
        try
        {
            var token = Helpers.GetInput("Enter Order Token: ", false);
            if (string.IsNullOrWhiteSpace(token))
            {
                Helpers.PrintError("Order Token is required.\n");
                return;
            }
            
            api.SetBearerToken(token);
            
            var addressId = Helpers.GetInput("Enter Address ID: ");
            if (string.IsNullOrWhiteSpace(addressId))
            {
                Helpers.PrintError("Address ID is required.\n");
                return;
            }
            
            var result = await api.Addresses().DeleteAddressAsync(addressId);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Address deleted successfully!\n");
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task GetAddressByIdExample(NimbblApi api)
    {
        try
        {
            var token = Helpers.GetInput("Enter Order Token: ", false);
            if (string.IsNullOrWhiteSpace(token))
            {
                Helpers.PrintError("Order Token is required.\n");
                return;
            }
            
            api.SetBearerToken(token);
            
            var addressId = Helpers.GetInput("Enter Address ID: ");
            if (string.IsNullOrWhiteSpace(addressId))
            {
                Helpers.PrintError("Address ID is required.\n");
                return;
            }
            
            var result = await api.Addresses().GetAddressByIdAsync(addressId);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Address retrieved successfully!\n");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task ImportAddressesExample(NimbblApi api)
    {
        try
        {
            var token = Helpers.GetInput("Enter Order Token: ", false);
            if (string.IsNullOrWhiteSpace(token))
            {
                Helpers.PrintError("Order Token is required.\n");
                return;
            }
            
            api.SetBearerToken(token);
            
            Helpers.PrintInfo("Import addresses from a provider (e.g., shiprocket)\n");
            Helpers.PrintInfo("This is a two-step process:\n");
            Helpers.PrintInfo("1. First call 'auth' command to initiate import\n");
            Helpers.PrintInfo("2. Then call 'verify' command with OTP to complete import\n\n");
            
            Console.WriteLine("Select command:\n");
            Console.WriteLine("1. auth (Initiate address import)\n");
            Console.WriteLine("2. verify (Verify OTP and import addresses)\n");
            var commandChoice = Helpers.GetInput("Enter choice (1 or 2): ");
            
            Dictionary<string, object?> importData;
            if (commandChoice == "1")
            {
                var command = "auth";
                var provider = Helpers.GetInput("Enter Provider Code (e.g., shiprocket): ");
                if (string.IsNullOrWhiteSpace(provider))
                {
                    Helpers.PrintError("Provider code is required.\n");
                    return;
                }
                importData = new Dictionary<string, object?>
                {
                    ["command"] = command,
                    ["provider"] = provider
                };
            }
            else if (commandChoice == "2")
            {
                var command = "verify";
                var provider = Helpers.GetInput("Enter Provider Code (e.g., shiprocket): ");
                var otp = Helpers.GetInput("Enter OTP (required): ");
                if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(otp))
                {
                    Helpers.PrintError("Provider code and OTP are required.\n");
                    return;
                }
                importData = new Dictionary<string, object?>
                {
                    ["command"] = command,
                    ["provider"] = provider,
                    ["otp"] = otp
                };
            }
            else
            {
                Helpers.PrintError("Invalid choice. Must be 1 or 2.\n");
                return;
            }
            
            var result = await api.Addresses().ImportAddressesAsync(importData);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Import request processed successfully!\n");
                if (result.TryGetProperty("next", out var nextProp) && nextProp.ValueKind == JsonValueKind.Array)
                {
                    Console.WriteLine("\nNext steps:\n");
                    foreach (var nextAction in nextProp.EnumerateArray())
                    {
                        if (nextAction.TryGetProperty("action", out var actionProp))
                        {
                            Console.WriteLine($"  - Action: {actionProp.GetString()}\n");
                            if (nextAction.TryGetProperty("url", out var urlProp))
                            {
                                Console.WriteLine($"    URL: {urlProp.GetString()}\n");
                            }
                            if (nextAction.TryGetProperty("required_parameters", out var paramsProp) && 
                                paramsProp.ValueKind == JsonValueKind.Array)
                            {
                                var paramList = paramsProp.EnumerateArray().Select(p => p.GetString()).Where(s => s != null);
                                Console.WriteLine($"    Required Parameters: {string.Join(", ", paramList)}\n");
                            }
                        }
                    }
                }
                if (!result.TryGetProperty("success", out var success) || !success.GetBoolean())
                {
                    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
                }
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task CheckAddressEligibilityExample(NimbblApi api)
    {
        try
        {
            var token = Helpers.GetInput("Enter Order Token: ", false);
            if (string.IsNullOrWhiteSpace(token))
            {
                Helpers.PrintError("Order Token is required.\n");
                return;
            }
            
            api.SetBearerToken(token);
            
            Helpers.PrintInfo("Enter eligibility check details:\n");
            var pincode = Helpers.GetInput("Pincode (required): ");
            if (string.IsNullOrWhiteSpace(pincode))
            {
                Helpers.PrintError("Pincode is required.\n");
                return;
            }
            
            var data = new Dictionary<string, object?> { ["pincode"] = pincode };
            
            var countryCode = Helpers.GetInput("Country Code (optional, default: IND): ", false);
            if (!string.IsNullOrWhiteSpace(countryCode))
            {
                data["country_code"] = countryCode;
            }
            
            var amountStr = Helpers.GetInput("Order Amount (optional, for shipping calculation): ", false);
            if (!string.IsNullOrWhiteSpace(amountStr) && decimal.TryParse(amountStr, out var amount))
            {
                data["amount"] = amount;
            }
            
            var currency = Helpers.GetInput("Currency (optional, e.g., INR): ", false);
            if (!string.IsNullOrWhiteSpace(currency))
            {
                data["currency"] = currency;
            }
            
            var result = await api.Addresses().CheckAddressEligibilityAsync(data);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Address eligibility checked!\n");
                var isEligible = result.TryGetProperty("is_eligible_for_shipping", out var eligProp) && eligProp.GetBoolean();
                Console.WriteLine($"  Eligible for Shipping: {(isEligible ? "Yes" : "No")}\n");
                if (result.TryGetProperty("max_shipping_charges", out var charges))
                {
                    Console.WriteLine($"  Max Shipping Charges: {charges.GetDecimal()}\n");
                }
                if (result.TryGetProperty("pincode_details", out var details) && details.ValueKind == JsonValueKind.Object)
                {
                    var city = details.TryGetProperty("city", out var c) ? c.GetString() : "N/A";
                    var state = details.TryGetProperty("state", out var s) ? s.GetString() : "N/A";
                    var cc = details.TryGetProperty("country_code", out var ccProp) ? ccProp.GetString() : "N/A";
                    Console.WriteLine($"  City: {city}\n");
                    Console.WriteLine($"  State: {state}\n");
                    Console.WriteLine($"  Country Code: {cc}\n");
                }
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }

    public static async Task LinkAddressWithOrderExample(NimbblApi api)
    {
        try
        {
            var token = Helpers.GetInput("Enter Order Token: ", false);
            if (string.IsNullOrWhiteSpace(token))
            {
                Helpers.PrintError("Order Token is required.\n");
                return;
            }
            
            api.SetBearerToken(token);
            
            var addressId = Helpers.GetInput("Enter Address ID (required): ");
            if (string.IsNullOrWhiteSpace(addressId))
            {
                Helpers.PrintError("Address ID is required.\n");
                return;
            }
            
            var orderId = Helpers.GetInput("Enter Order ID (optional, not required if using order token): ", false);
            Console.WriteLine("Link as:\n");
            Console.WriteLine("1. shipping\n");
            Console.WriteLine("2. billing\n");
            var linkAsChoice = Helpers.GetInput("Enter choice (1 or 2): ");
            var linkAs = linkAsChoice == "1" ? "shipping" : (linkAsChoice == "2" ? "billing" : null);
            if (linkAs == null)
            {
                Helpers.PrintError("Invalid choice. Must be 'shipping' or 'billing'.\n");
                return;
            }
            
            var linkData = new Dictionary<string, object?>
            {
                ["address"] = new Dictionary<string, object?>
                {
                    ["address_id"] = addressId
                },
                ["link_as"] = linkAs
            };
            
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                linkData["order_id"] = orderId;
            }
            
            var result = await api.Addresses().LinkAddressWithOrderAsync(linkData);
            
            if (result.TryGetProperty("error", out var errorProp))
            {
                Helpers.PrintError($"Error: {errorProp}\n");
            }
            else
            {
                Helpers.PrintSuccess("Order linked to address successfully!\n");
                if (result.TryGetProperty("success", out var success) && success.GetBoolean())
                {
                    var message = result.TryGetProperty("message", out var msg) ? msg.GetString() : "Address linked successfully";
                    Console.WriteLine($"  {message}\n");
                }
                else
                {
                    if (!result.TryGetProperty("response", out var resp) || resp.ValueKind != JsonValueKind.Null)
                    {
                        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Helpers.PrintException(ex);
        }
    }
}

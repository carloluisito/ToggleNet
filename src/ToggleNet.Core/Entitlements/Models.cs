using System;
using System.Collections.Generic;

namespace ToggleNet.Core.Entitlements;

public enum SubscriptionStatus 
{ 
    Unknown, 
    Active, 
    Trialing, 
    PastDue, 
    Canceled, 
    Inactive 
}

public sealed record Entitlements(
    string Plan,
    SubscriptionStatus Status,
    HashSet<string> Features,
    IReadOnlyDictionary<string, int> Limits
);

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ToggleNet.Core.Entities;

namespace ToggleNet.Core.FeatureGate;

/// <summary>
/// Wrapper around FeatureFlagManager for flag evaluation only
/// </summary>
public sealed class FlagEvaluator : IFlagEvaluator
{
    private readonly FeatureFlagManager _flagManager;

    public FlagEvaluator(FeatureFlagManager flagManager)
    {
        _flagManager = flagManager ?? throw new ArgumentNullException(nameof(flagManager));
    }

    public async Task<bool> IsEnabledAsync(string featureKey, string tenantId, string userId, CancellationToken ct = default)
    {
        // Create UserContext with tenantId as an attribute for multi-tenant support
        var userContext = new UserContext 
        { 
            UserId = userId,
            Attributes = new Dictionary<string, object> { ["tenantId"] = tenantId }
        };
        
        return await _flagManager.IsEnabledAsync(featureKey, userContext, trackUsage: false);
    }
}

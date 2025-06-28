// Targeting Rules Dashboard JavaScript

let currentFeatureFlagId = null;
let ruleGroupCounter = 0;
let ruleCounter = 0;

// Initialize the page
document.addEventListener('DOMContentLoaded', function() {
    // Any initialization code
});

// Load targeting rules for selected flag
async function loadTargetingRules() {
    const flagSelect = document.getElementById('flagSelect');
    const selectedOption = flagSelect.options[flagSelect.selectedIndex];
    
    if (!selectedOption.value) {
        hideTargetingRulesSection();
        return;
    }
    
    currentFeatureFlagId = selectedOption.value;
    const flagName = selectedOption.dataset.name;
    
    try {
        const response = await fetch('?handler=GetTargetingRules', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': getAntiForgeryToken()
            },
            body: JSON.stringify({ FeatureFlagId: currentFeatureFlagId })
        });
        
        if (response.ok) {
            const data = await response.json();
            populateTargetingRules(data);
            showTargetingRulesSection();
        } else {
            console.error('Error loading targeting rules:', response.statusText);
            showAlert('Error loading targeting rules', 'danger');
        }
    } catch (error) {
        console.error('Error loading targeting rules:', error);
        showAlert('Error loading targeting rules', 'danger');
    }
}

// Show targeting rules section
function showTargetingRulesSection() {
    document.getElementById('flagInfo').classList.remove('d-none');
    document.getElementById('targetingRulesSection').classList.remove('d-none');
    document.getElementById('actionSection').classList.remove('d-none');
}

// Hide targeting rules section
function hideTargetingRulesSection() {
    document.getElementById('flagInfo').classList.add('d-none');
    document.getElementById('targetingRulesSection').classList.add('d-none');
    document.getElementById('actionSection').classList.add('d-none');
    currentFeatureFlagId = null;
}

// Populate targeting rules from server data
function populateTargetingRules(data) {
    // Set flag info
    document.getElementById('useTargetingRules').checked = data.featureFlag.useTargetingRules;
    document.getElementById('fallbackPercentage').value = data.featureFlag.rolloutPercentage;
    
    // Clear existing rule groups
    const container = document.getElementById('ruleGroupsContainer');
    container.innerHTML = '';
    
    if (data.ruleGroups && data.ruleGroups.length > 0) {
        data.ruleGroups.forEach(ruleGroup => {
            addRuleGroup(ruleGroup);
        });
        toggleTargetingRulesDisplay();
    } else {
        showEmptyState();
    }
}

// Toggle targeting rules display
function toggleTargetingRules() {
    toggleTargetingRulesDisplay();
}

function toggleTargetingRulesDisplay() {
    const useTargetingRules = document.getElementById('useTargetingRules').checked;
    const container = document.getElementById('ruleGroupsContainer');
    
    if (!useTargetingRules) {
        showEmptyState();
    } else if (container.children.length === 0 || container.querySelector('.text-center')) {
        // Show empty state but allow adding rule groups
        showEmptyState();
    }
}

function showEmptyState() {
    const container = document.getElementById('ruleGroupsContainer');
    container.innerHTML = `
        <div class="text-center py-5 text-muted">
            <i class="bi bi-target fs-2 d-block mb-3"></i>
            <p>No targeting rule groups configured.</p>
            <p class="small">Add a rule group to start targeting specific users based on their attributes.</p>
        </div>
    `;
}

// Add new rule group
function addRuleGroup(existingData = null) {
    const container = document.getElementById('ruleGroupsContainer');
    
    // Remove empty state if present
    if (container.querySelector('.text-center')) {
        container.innerHTML = '';
    }
    
    const template = document.getElementById('ruleGroupTemplate');
    const clone = template.content.cloneNode(true);
    
    ruleGroupCounter++;
    const groupId = existingData?.id || `group-${ruleGroupCounter}`;
    
    // Set group data
    const groupElement = clone.querySelector('.rule-group');
    groupElement.dataset.groupId = groupId;
    
    if (existingData) {
        clone.querySelector('.group-name').value = existingData.name || '';
        clone.querySelector('.group-logic').value = existingData.logicalOperator || 'And';
        clone.querySelector('.group-priority').value = existingData.priority || 1;
        clone.querySelector('.group-rollout').value = existingData.rolloutPercentage || 100;
        clone.querySelector('.group-enabled').checked = existingData.isEnabled !== false;
        clone.querySelector('.priority-display').textContent = existingData.priority || 1;
    }
    
    container.appendChild(clone);
    
    // Add existing rules
    if (existingData?.rules) {
        const rulesContainer = container.querySelector(`[data-group-id="${groupId}"] .rules-container`);
        existingData.rules.forEach(rule => {
            addRuleToGroup(rulesContainer, rule);
        });
    }
}

// Add rule to specific group
function addRule(button) {
    const ruleGroup = button.closest('.rule-group');
    const rulesContainer = ruleGroup.querySelector('.rules-container');
    addRuleToGroup(rulesContainer);
}

function addRuleToGroup(container, existingData = null) {
    const template = document.getElementById('ruleTemplate');
    const clone = template.content.cloneNode(true);
    
    ruleCounter++;
    const ruleId = existingData?.id || `rule-${ruleCounter}`;
    
    const ruleElement = clone.querySelector('.rule');
    ruleElement.dataset.ruleId = ruleId;
    
    if (existingData) {
        clone.querySelector('.rule-name').value = existingData.name || '';
        clone.querySelector('.rule-attribute').value = existingData.attribute || '';
        clone.querySelector('.rule-operator').value = existingData.operator || 'Equals';
        clone.querySelector('.rule-value').value = existingData.value || '';
        clone.querySelector('.rule-priority').value = existingData.priority || 1;
        clone.querySelector('.rule-enabled').checked = existingData.isEnabled !== false;
    }
    
    container.appendChild(clone);
}

// Remove rule group
function removeRuleGroup(button) {
    const ruleGroup = button.closest('.rule-group');
    ruleGroup.remove();
    
    // Show empty state if no groups left
    const container = document.getElementById('ruleGroupsContainer');
    if (container.children.length === 0) {
        showEmptyState();
    }
}

// Remove rule
function removeRule(button) {
    const rule = button.closest('.rule');
    rule.remove();
}

// Update priority display
function updatePriorityDisplay(input) {
    const ruleGroup = input.closest('.rule-group');
    const display = ruleGroup.querySelector('.priority-display');
    display.textContent = input.value;
}

// Save targeting rules
async function saveTargetingRules() {
    if (!currentFeatureFlagId) {
        showActionAlert('Please select a feature flag first', 'warning');
        return;
    }
    
    const data = collectTargetingRulesData();
    
    try {
        const response = await fetch('?handler=SaveTargetingRules', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': getAntiForgeryToken()
            },
            body: JSON.stringify(data)
        });
        
        if (response.ok) {
            const result = await response.json();
            showActionAlert(result.message || 'Targeting rules saved successfully', 'success');
        } else {
            const error = await response.text();
            showActionAlert('Error saving targeting rules: ' + error, 'danger');
        }
    } catch (error) {
        console.error('Error saving targeting rules:', error);
        showActionAlert('Error saving targeting rules', 'danger');
    }
}

// Test targeting rules
async function testTargetingRules() {
    if (!currentFeatureFlagId) {
        showAlert('Please select a feature flag first', 'warning');
        return;
    }
    
    // Show test modal
    showTestModal();
}

function showTestModal() {
    // Create test modal dynamically
    const modalHtml = `
        <div class="modal fade" id="testModal" tabindex="-1">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Test Targeting Rules</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="mb-3">
                            <label class="form-label">Test User ID</label>
                            <input type="text" id="testUserId" class="form-control" value="test-user-123">
                        </div>
                        <div class="mb-3">
                            <label class="form-label">User Attributes (JSON)</label>
                            <textarea id="testAttributes" class="form-control" rows="8">{
  "country": "US",
  "userType": "premium",
  "age": 28,
  "deviceType": "mobile",
  "appVersion": "2.1.0",
  "plan": "enterprise",
  "registrationDate": "2023-01-15"
}</textarea>
                        </div>
                        <div id="testResult" class="d-none"></div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                        <button type="button" class="btn btn-primary" onclick="executeTest()">Run Test</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Remove existing modal if present
    const existingModal = document.getElementById('testModal');
    if (existingModal) {
        existingModal.remove();
    }
    
    // Add modal to DOM
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    
    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('testModal'));
    modal.show();
}

async function executeTest() {
    const testUserId = document.getElementById('testUserId').value;
    const testAttributesText = document.getElementById('testAttributes').value;
    
    let testAttributes;
    try {
        testAttributes = JSON.parse(testAttributesText);
    } catch (error) {
        showModalAlert('Invalid JSON in test attributes', 'danger');
        return;
    }
    
    const data = collectTargetingRulesData();
    data.UserId = testUserId;
    data.UserAttributes = testAttributes;
    
    try {
        const response = await fetch('?handler=TestTargetingRules', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': getAntiForgeryToken()
            },
            body: JSON.stringify(data)
        });
        
        if (response.ok) {
            const result = await response.json();
            displayTestResult(result);
        } else {
            const error = await response.text();
            showModalAlert('Error testing targeting rules: ' + error, 'danger');
        }
    } catch (error) {
        console.error('Error testing targeting rules:', error);
        showModalAlert('Error testing targeting rules: ' + error.message, 'danger');
    }
}

function displayTestResult(result) {
    // Display in modal
    const resultDiv = document.getElementById('testResult');
    const alertClass = result.result ? 'alert-success' : 'alert-warning';
    const icon = result.result ? 'check-circle' : 'x-circle';
    
    resultDiv.innerHTML = `
        <div class="alert ${alertClass}">
            <i class="bi bi-${icon} me-2"></i>
            <strong>Test Result:</strong> ${result.message}
        </div>
        <div class="small text-muted">
            <strong>User Context:</strong><br>
            User ID: ${result.userContext.userId}<br>
            Attributes: ${JSON.stringify(result.userContext.attributes, null, 2)}
        </div>
    `;
    
    resultDiv.classList.remove('d-none');
    
    // Also display in main page test results section
    displayTestResultOnMainPage(result);
}

function displayTestResultOnMainPage(result) {
    const testResultsSection = document.getElementById('testResultsSection');
    const testResultsContent = document.getElementById('testResultsContent');
    
    const resultClass = result.result ? 'enabled' : 'disabled';
    const icon = result.result ? 'check-circle-fill' : 'x-circle-fill';
    const statusText = result.result ? 'ENABLED' : 'DISABLED';
    const statusColor = result.result ? 'text-success' : 'text-danger';
    
    const timestamp = new Date().toLocaleString();
    
    const resultHtml = `
        <div class="test-result-item ${resultClass}">
            <div class="d-flex justify-content-between align-items-start mb-2">
                <div class="d-flex align-items-center">
                    <i class="bi bi-${icon} me-2 ${statusColor}"></i>
                    <strong>Feature Flag: ${statusText}</strong>
                </div>
                <small class="text-muted">${timestamp}</small>
            </div>
            
            <div class="test-user-input">
                <div class="row">
                    <div class="col-md-6">
                        <strong>User ID:</strong> ${result.userContext.userId}
                    </div>
                    <div class="col-md-6">
                        <strong>Result:</strong> <span class="${statusColor}">${result.message}</span>
                    </div>
                </div>
                <div class="mt-2">
                    <strong>User Attributes:</strong>
                    <div class="rule-evaluation">
                        ${JSON.stringify(result.userContext.attributes, null, 2)}
                    </div>
                </div>
            </div>
            
            ${result.evaluationDetails ? `
                <div class="mt-2">
                    <strong>Evaluation Details:</strong>
                    <div class="rule-evaluation">
                        ${result.evaluationDetails}
                    </div>
                </div>
            ` : ''}
        </div>
    `;
    
    // Prepend new result (show latest first)
    testResultsContent.insertAdjacentHTML('afterbegin', resultHtml);
    
    // Show the test results section
    testResultsSection.classList.remove('d-none');
    
    // Limit to last 10 results to avoid clutter
    const resultItems = testResultsContent.querySelectorAll('.test-result-item');
    if (resultItems.length > 10) {
        for (let i = 10; i < resultItems.length; i++) {
            resultItems[i].remove();
        }
    }
}

// Show alert within the modal
function showModalAlert(message, type = 'info') {
    const modalBody = document.querySelector('#testModal .modal-body');
    if (!modalBody) {
        // Fallback to regular alert if modal is not available
        showAlert(message, type);
        return;
    }
    
    // Remove any existing modal alerts
    const existingAlert = modalBody.querySelector('.modal-alert');
    if (existingAlert) {
        existingAlert.remove();
    }
    
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show modal-alert" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    // Insert alert at the top of modal body
    modalBody.insertAdjacentHTML('afterbegin', alertHtml);
    
    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        const alert = modalBody.querySelector('.modal-alert');
        if (alert) {
            alert.remove();
        }
    }, 5000);
}

// Show alert in the action section near save/test buttons
function showActionAlert(message, type = 'info') {
    const actionNotifications = document.getElementById('actionNotifications');
    if (!actionNotifications) {
        // Fallback to regular alert if action notifications area is not available
        showAlert(message, type);
        return;
    }
    
    // Remove any existing action alerts
    const existingAlert = actionNotifications.querySelector('.action-alert');
    if (existingAlert) {
        existingAlert.remove();
    }
    
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show action-alert" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    // Insert alert in the action notifications area
    actionNotifications.innerHTML = alertHtml;
    
    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        const alert = actionNotifications.querySelector('.action-alert');
        if (alert) {
            alert.remove();
        }
    }, 5000);
}

// Clear test results
function clearTestResults() {
    const testResultsSection = document.getElementById('testResultsSection');
    const testResultsContent = document.getElementById('testResultsContent');
    
    testResultsContent.innerHTML = '';
    testResultsSection.classList.add('d-none');
}

// Collect targeting rules data from form
function collectTargetingRulesData() {
    const useTargetingRules = document.getElementById('useTargetingRules').checked;
    const fallbackPercentage = parseInt(document.getElementById('fallbackPercentage').value) || 0;
    
    const ruleGroups = [];
    
    if (useTargetingRules) {
        document.querySelectorAll('.rule-group').forEach(groupElement => {
            const groupData = {
                Id: groupElement.dataset.groupId,
                Name: groupElement.querySelector('.group-name').value,
                LogicalOperator: groupElement.querySelector('.group-logic').value,
                Priority: parseInt(groupElement.querySelector('.group-priority').value) || 1,
                RolloutPercentage: parseInt(groupElement.querySelector('.group-rollout').value) || 100,
                IsEnabled: groupElement.querySelector('.group-enabled').checked,
                Rules: []
            };
            
            groupElement.querySelectorAll('.rule').forEach(ruleElement => {
                const ruleData = {
                    Id: ruleElement.dataset.ruleId,
                    Name: ruleElement.querySelector('.rule-name').value,
                    Attribute: ruleElement.querySelector('.rule-attribute').value,
                    Operator: ruleElement.querySelector('.rule-operator').value,
                    Value: ruleElement.querySelector('.rule-value').value,
                    Priority: parseInt(ruleElement.querySelector('.rule-priority').value) || 1,
                    IsEnabled: ruleElement.querySelector('.rule-enabled').checked
                };
                
                groupData.Rules.push(ruleData);
            });
            
            ruleGroups.push(groupData);
        });
    }
    
    return {
        FeatureFlagId: currentFeatureFlagId,
        UseTargetingRules: useTargetingRules,
        FallbackPercentage: fallbackPercentage,
        RuleGroups: ruleGroups
    };
}

// Utility functions
function getAntiForgeryToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
}

function showAlert(message, type = 'info') {
    // Create alert element
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    // Find container (preferably at top of main content)
    const container = document.querySelector('.container');
    container.insertAdjacentHTML('afterbegin', alertHtml);
    
    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        const alert = container.querySelector('.alert');
        if (alert) {
            alert.remove();
        }
    }, 5000);
}

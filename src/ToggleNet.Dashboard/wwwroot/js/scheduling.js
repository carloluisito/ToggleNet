// Global variables
let currentFlagName = '';

// Initialize the page
document.addEventListener('DOMContentLoaded', function() {
    // Set current datetime as minimum for datetime-local inputs
    const now = new Date();
    const localDateTime = new Date(now.getTime() - (now.getTimezoneOffset() * 60000)).toISOString().slice(0, 16);
    
    document.getElementById('activationStartTime').min = localDateTime;
    document.getElementById('deactivationEndTime').min = localDateTime;
    
    // Convert UTC times to local times for display
    convertUtcTimesToLocal();
    
    // Set up form handlers
    setupFormHandlers();
    
    // Load flag scheduling if a flag is pre-selected
    if (document.getElementById('flagSelect').value) {
        loadFlagScheduling();
    }
});

function setupFormHandlers() {
    // Schedule Activation Form
    document.getElementById('scheduleActivationForm').addEventListener('submit', async function(e) {
        e.preventDefault();
        await scheduleActivation();
    });
    
    // Schedule Deactivation Form
    document.getElementById('scheduleDeactivationForm').addEventListener('submit', async function(e) {
        e.preventDefault();
        await scheduleDeactivation();
    });
    
    // Schedule Temporary Form
    document.getElementById('scheduleTemporaryForm').addEventListener('submit', async function(e) {
        e.preventDefault();
        await scheduleTemporary();
    });
}

function loadFlagScheduling() {
    const flagSelect = document.getElementById('flagSelect');
    const selectedOption = flagSelect.options[flagSelect.selectedIndex];
    
    if (!selectedOption.value) {
        hideSchedulingOptions();
        return;
    }
    
    currentFlagName = selectedOption.value;
    showSchedulingOptions();
    
    // Here you could load current scheduling info via AJAX if needed
    // For now, we'll just show the options
}

function showSchedulingOptions() {
    document.getElementById('schedulingOptions').classList.remove('d-none');
}

function hideSchedulingOptions() {
    document.getElementById('schedulingOptions').classList.add('d-none');
    document.getElementById('currentSchedulingInfo').classList.add('d-none');
}

function selectFlag(flagName) {
    const flagSelect = document.getElementById('flagSelect');
    flagSelect.value = flagName;
    loadFlagScheduling();
    
    // Scroll to the scheduling options
    document.getElementById('schedulingOptions').scrollIntoView({ 
        behavior: 'smooth', 
        block: 'start' 
    });
}

async function scheduleActivation() {
    if (!currentFlagName) {
        showAlert('Please select a feature flag first', 'warning');
        return;
    }
    
    const startTime = document.getElementById('activationStartTime').value;
    const hours = parseInt(document.getElementById('activationDurationHours').value) || 0;
    const days = parseInt(document.getElementById('activationDurationDays').value) || 0;
    const months = parseInt(document.getElementById('activationDurationMonths').value) || 0;
    const timeZone = document.getElementById('activationTimeZone').value;
    
    if (!startTime) {
        showAlert('Start time is required', 'danger');
        return;
    }
    
    let duration = null;
    if (hours > 0 || days > 0 || months > 0) {
        let totalHours;
        
        if (months > 0) {
            // For months, calculate the actual date difference to get precise hours
            const startTime = document.getElementById('activationStartTime').value;
            if (startTime) {
                const startDate = new Date(startTime);
                const endDate = new Date(startDate);
                endDate.setMonth(endDate.getMonth() + months);
                endDate.setDate(endDate.getDate() + days);
                endDate.setHours(endDate.getHours() + hours);
                
                const diffMs = endDate.getTime() - startDate.getTime();
                totalHours = diffMs / (1000 * 60 * 60); // Convert ms to hours
            } else {
                // Fallback to approximate calculation if no start time
                totalHours = hours + (days * 24) + (months * 24 * 30.44);
            }
        } else {
            // Simple calculation for hours and days only
            totalHours = hours + (days * 24);
        }
        
        // Format as proper TimeSpan string: if we have more than 24 hours, we need to use days.hours:minutes:seconds format
        if (totalHours >= 24) {
            const wholeDays = Math.floor(totalHours / 24);
            const remainingHours = Math.round(totalHours % 24);
            duration = `${wholeDays}.${remainingHours.toString().padStart(2, '0')}:00:00`;
        } else {
            duration = `${Math.round(totalHours).toString().padStart(2, '0')}:00:00`;
        }
    }
    
    const data = {
        FlagName: currentFlagName,
        StartTime: startTime, // Send the raw datetime-local value, let C# handle timezone conversion
        Duration: duration,
        TimeZone: timeZone || null
    };
    
    try {
        const response = await fetch('?handler=ScheduleActivation', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': getAntiForgeryToken()
            },
            body: JSON.stringify(data)
        });
        
        if (response.ok) {
            const result = await response.json();
            showAlert(result.message || 'Activation scheduled successfully', 'success');
            setTimeout(() => location.reload(), 1500);
        } else {
            const error = await response.text();
            showAlert('Error scheduling activation: ' + error, 'danger');
        }
    } catch (error) {
        console.error('Error scheduling activation:', error);
        showAlert('Error scheduling activation', 'danger');
    }
}

async function scheduleDeactivation() {
    if (!currentFlagName) {
        showAlert('Please select a feature flag first', 'warning');
        return;
    }
    
    const endTime = document.getElementById('deactivationEndTime').value;
    
    if (!endTime) {
        showAlert('End time is required', 'danger');
        return;
    }
    
    const data = {
        FlagName: currentFlagName,
        EndTime: endTime  // Send the raw datetime-local value, let C# handle timezone conversion
    };
    
    try {
        const response = await fetch('?handler=ScheduleDeactivation', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': getAntiForgeryToken()
            },
            body: JSON.stringify(data)
        });
        
        if (response.ok) {
            const result = await response.json();
            showAlert(result.message || 'Deactivation scheduled successfully', 'success');
            setTimeout(() => location.reload(), 1500);
        } else {
            const error = await response.text();
            showAlert('Error scheduling deactivation: ' + error, 'danger');
        }
    } catch (error) {
        console.error('Error scheduling deactivation:', error);
        showAlert('Error scheduling deactivation', 'danger');
    }
}

async function scheduleTemporary() {
    if (!currentFlagName) {
        showAlert('Please select a feature flag first', 'warning');
        return;
    }
    
    const hours = parseInt(document.getElementById('tempDurationHours').value) || 0;
    const days = parseInt(document.getElementById('tempDurationDays').value) || 0;
    const months = parseInt(document.getElementById('tempDurationMonths').value) || 0;
    
    if (hours === 0 && days === 0 && months === 0) {
        showAlert('Please specify a duration', 'danger');
        return;
    }
    
    const totalHours = hours + (days * 24) + (months * 24 * 30.44); // Use simple calculation for temporary (no specific start date)
    
    // Format as proper TimeSpan string: if we have more than 24 hours, we need to use days.hours:minutes:seconds format
    let duration;
    if (totalHours >= 24) {
        const wholeDays = Math.floor(totalHours / 24);
        const remainingHours = Math.round(totalHours % 24);
        duration = `${wholeDays}.${remainingHours.toString().padStart(2, '0')}:00:00`;
    } else {
        duration = `${Math.round(totalHours).toString().padStart(2, '0')}:00:00`;
    }
    
    const data = {
        FlagName: currentFlagName,
        Duration: duration
    };
    
    try {
        const response = await fetch('?handler=ScheduleTemporary', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': getAntiForgeryToken()
            },
            body: JSON.stringify(data)
        });
        
        if (response.ok) {
            const result = await response.json();
            showAlert(result.message || 'Temporary activation scheduled successfully', 'success');
            setTimeout(() => location.reload(), 1500);
        } else {
            const error = await response.text();
            showAlert('Error scheduling temporary activation: ' + error, 'danger');
        }
    } catch (error) {
        console.error('Error scheduling temporary activation:', error);
        showAlert('Error scheduling temporary activation', 'danger');
    }
}

async function removeScheduling() {
    if (!currentFlagName) {
        showAlert('Please select a feature flag first', 'warning');
        return;
    }
    
    if (!confirm('Are you sure you want to remove time-based scheduling from this flag?')) {
        return;
    }
    
    const data = {
        FlagName: currentFlagName
    };
    
    try {
        const response = await fetch('?handler=RemoveScheduling', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': getAntiForgeryToken()
            },
            body: JSON.stringify(data)
        });
        
        if (response.ok) {
            const result = await response.json();
            showAlert(result.message || 'Scheduling removed successfully', 'success');
            setTimeout(() => location.reload(), 1500);
        } else {
            const error = await response.text();
            showAlert('Error removing scheduling: ' + error, 'danger');
        }
    } catch (error) {
        console.error('Error removing scheduling:', error);
        showAlert('Error removing scheduling', 'danger');
    }
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

function convertUtcTimesToLocal() {
    // Find all elements with UTC times and convert them to local time
    document.querySelectorAll('.utc-time').forEach(element => {
        const utcTimeStr = element.getAttribute('data-utc');
        if (utcTimeStr) {
            try {
                const utcDate = new Date(utcTimeStr);
                const localDate = new Date(utcDate.getTime());
                
                // Format the local time
                const options = {
                    year: 'numeric',
                    month: 'short',
                    day: 'numeric',
                    hour: 'numeric',
                    minute: '2-digit',
                    hour12: true
                };
                const localTimeStr = localDate.toLocaleString(undefined, options);
                
                // Update the element to show both local and UTC time
                element.innerHTML = `${localTimeStr} (Local)<br><small class="text-muted">${element.innerHTML}</small>`;
            } catch (error) {
                console.error('Error converting UTC time to local:', error);
            }
        }
    });
}

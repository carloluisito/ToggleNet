// Scheduling Examples JavaScript
// Handles timezone conversion and UI interactions

document.addEventListener('DOMContentLoaded', function() {
    // Convert UTC times to local times for display
    convertUtcTimesToLocal();
    
    // Auto-dismiss alerts after 5 seconds
    setTimeout(function() {
        const alerts = document.querySelectorAll('.alert-dismissible');
        alerts.forEach(alert => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);
});

/**
 * Converts UTC times to local times for better user experience
 * Finds all elements with the 'utc-time' class and converts their data-utc attribute
 */
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
                // If conversion fails, keep the original content
            }
        }
    });
}

/**
 * Confirmation dialog for removing scheduling
 */
function confirmRemoveScheduling(flagName) {
    return confirm(`Are you sure you want to remove scheduling from "${flagName}"?\n\nThis will stop any scheduled activation or deactivation for this flag.`);
}

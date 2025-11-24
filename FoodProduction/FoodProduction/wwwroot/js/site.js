// Enable popovers everywhere
const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
const popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
    return new bootstrap.Popover(popoverTriggerEl)
})

// Enable tooltips everywhere
const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl)
})

// Auto-hide alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function () {
    const alerts = document.querySelectorAll('.alert')
    alerts.forEach(alert => {
        const closeButton = alert.querySelector('[data-bs-dismiss="alert"]')
        if (!closeButton) {
            setTimeout(() => {
                const bsAlert = new bootstrap.Alert(alert)
            }, 5000)
        }
    })
})

// Format currency inputs
function formatCurrency(input) {
    let value = input.value.replace(/\D/g, '')
    value = (value / 100).toFixed(2)
    input.value = '$' + value
}

// Confirm delete actions
function confirmDelete(message) {
    return confirm(message || 'Are you sure you want to delete this?')
}

// Form validation
function validateForm(formId) {
    const form = document.getElementById(formId)
    if (form && !form.checkValidity()) {
        event.preventDefault()
        event.stopPropagation()
    }
    form.classList.add('was-validated')
}

// Number formatting
function formatNumber(num, decimals) {
    return Number(num).toLocaleString('en-US', {
        minimumFractionDigits: decimals || 0,
        maximumFractionDigits: decimals || 0
    })
}

// Session management
function setSessionValue(key, value) {
    sessionStorage.setItem(key, value)
}

function getSessionValue(key) {
    return sessionStorage.getItem(key)
}

function clearSessionValue(key) {
    sessionStorage.removeItem(key)
}

// Dashboard JavaScript functions

// Sparkline chart for KPI cards
window.initializeSparkline = (elementId, data, color) => {
    const ctx = document.getElementById(elementId);
    if (!ctx) return;

    // Destroy existing chart if any
    const existingChart = Chart.getChart(ctx);
    if (existingChart) {
        existingChart.destroy();
    }

    // Ensure data is an array
    const chartData = Array.isArray(data) ? data : [];
    if (chartData.length === 0) return;

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: chartData.map((_, i) => i.toString()),
            datasets: [{
                data: chartData,
                borderColor: color,
                backgroundColor: color + '20',
                borderWidth: 2,
                fill: true,
                tension: 0.4,
                pointRadius: 0,
                pointHoverRadius: 3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    enabled: true,
                    mode: 'index',
                    intersect: false,
                    callbacks: {
                        title: () => '',
                        label: (context) => context.raw.toString()
                    }
                }
            },
            scales: {
                x: { display: false },
                y: { display: false }
            },
            interaction: {
                mode: 'nearest',
                axis: 'x',
                intersect: false
            }
        }
    });
};

// Chart.js configuration
window.initializeDeliveryModeChart = (data) => {
    const ctx = document.getElementById('deliveryModeChart');
    if (!ctx) return;

    // Destroy existing chart if any
    const existingChart = Chart.getChart(ctx);
    if (existingChart) {
        existingChart.destroy();
    }

    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Normal Vaginal', 'Assisted Vaginal', 'Elective C-Section', 'Emergency C-Section'],
            datasets: [{
                data: [data.normalVaginal, data.assistedVaginal, data.electiveCaesarean, data.emergencyCaesarean],
                backgroundColor: [
                    '#198754', // success
                    '#0dcaf0', // info
                    '#ffc107', // warning
                    '#dc3545'  // danger
                ],
                borderWidth: 0,
                hoverOffset: 10
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '60%',
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((context.raw / total) * 100).toFixed(1);
                            return `${context.label}: ${context.raw} (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
};

// Initialize trend chart
window.initializeTrendChart = (elementId, labels, data, label) => {
    const ctx = document.getElementById(elementId);
    if (!ctx) return;

    // Destroy existing chart if any
    const existingChart = Chart.getChart(ctx);
    if (existingChart) {
        existingChart.destroy();
    }

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: label,
                data: data,
                borderColor: '#4f46e5',
                backgroundColor: 'rgba(79, 70, 229, 0.1)',
                borderWidth: 2,
                fill: true,
                tension: 0.4,
                pointBackgroundColor: '#4f46e5',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointRadius: 4,
                pointHoverRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    mode: 'index',
                    intersect: false
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    }
                }
            },
            interaction: {
                mode: 'nearest',
                axis: 'x',
                intersect: false
            }
        }
    });
};

// Initialize bar chart for comparisons
window.initializeBarChart = (elementId, labels, datasets) => {
    const ctx = document.getElementById(elementId);
    if (!ctx) return;

    // Destroy existing chart if any
    const existingChart = Chart.getChart(ctx);
    if (existingChart) {
        existingChart.destroy();
    }

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: datasets
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    }
                }
            }
        }
    });
};

// Format numbers with comma separators
window.formatNumber = (num) => {
    return new Intl.NumberFormat().format(num);
};

// Show notification toast
window.showToast = (message, type = 'info') => {
    const toastContainer = document.getElementById('toast-container') || createToastContainer();

    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0`;
    toast.setAttribute('role', 'alert');
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;

    toastContainer.appendChild(toast);
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();

    toast.addEventListener('hidden.bs.toast', () => {
        toast.remove();
    });
};

function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toast-container';
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    container.style.zIndex = '1100';
    document.body.appendChild(container);
    return container;
}

// Print report
window.printReport = () => {
    window.print();
};

// Export to CSV
window.exportToCSV = (data, filename) => {
    const csvContent = "data:text/csv;charset=utf-8,"
        + data.map(e => e.join(",")).join("\n");

    const encodedUri = encodeURI(csvContent);
    const link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", filename);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Auto-refresh setup
let refreshInterval = null;

window.startAutoRefresh = (intervalMs, callback) => {
    if (refreshInterval) {
        clearInterval(refreshInterval);
    }
    refreshInterval = setInterval(callback, intervalMs);
};

window.stopAutoRefresh = () => {
    if (refreshInterval) {
        clearInterval(refreshInterval);
        refreshInterval = null;
    }
};

// Confirm dialog
window.confirmAction = (message) => {
    return confirm(message);
};

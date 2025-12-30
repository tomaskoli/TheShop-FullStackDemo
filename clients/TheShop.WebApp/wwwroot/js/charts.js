let charts = {};

function getThemeColors() {
    const isLightMode = document.body.classList.contains('theme-light');
    return {
        textColor: isLightMode ? '#1a1a1a' : '#e0e0e0',
        textSecondary: isLightMode ? '#666666' : '#a0a0a0',
        gridColor: isLightMode ? 'rgba(0,0,0,0.08)' : 'rgba(255,255,255,0.05)',
        borderColor: isLightMode ? '#e0e0e0' : '#1a1a2e'
    };
}

export function renderPieChart(canvasId, labels, data, title) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    if (charts[canvasId]) {
        charts[canvasId].destroy();
    }

    const theme = getThemeColors();
    const colors = [
        '#4dc9f6', '#f67019', '#f53794', '#537bc4', '#acc236',
        '#166a8f', '#00a950', '#58595b', '#8549ba', '#ff6384',
        '#36a2eb', '#ffce56', '#4bc0c0', '#9966ff', '#ff9f40'
    ];

    charts[canvasId] = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors.slice(0, data.length),
                borderWidth: 1,
                borderColor: theme.borderColor
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { 
                        color: theme.textColor,
                        padding: 15,
                        font: { size: 12 }
                    }
                },
                title: {
                    display: true,
                    text: title,
                    color: theme.textColor,
                    font: { size: 14, weight: '500' }
                }
            }
        }
    });
}

export function renderLineChart(canvasId, labels, data, title) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    if (charts[canvasId]) {
        charts[canvasId].destroy();
    }

    const theme = getThemeColors();

    charts[canvasId] = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Registrations',
                data: data,
                borderColor: '#4dc9f6',
                backgroundColor: 'rgba(77, 201, 246, 0.1)',
                fill: true,
                tension: 0.3,
                pointBackgroundColor: '#4dc9f6',
                pointBorderColor: theme.borderColor,
                pointBorderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                title: {
                    display: true,
                    text: title,
                    color: theme.textColor,
                    font: { size: 14, weight: '500' }
                }
            },
            scales: {
                x: {
                    ticks: { color: theme.textSecondary },
                    grid: { color: theme.gridColor }
                },
                y: {
                    beginAtZero: true,
                    ticks: { 
                        color: theme.textSecondary,
                        stepSize: 1
                    },
                    grid: { color: theme.gridColor }
                }
            }
        }
    });
}

export function destroyCharts() {
    Object.keys(charts).forEach(key => {
        if (charts[key]) {
            charts[key].destroy();
        }
    });
    charts = {};
}

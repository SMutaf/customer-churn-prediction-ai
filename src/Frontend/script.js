const API_BASE_URL = "https://localhost:7272/api";

let riskChart = null;

// Sayfa yüklenince verileri çek
document.addEventListener("DOMContentLoaded", () => {
    loadData();
});

async function loadData() {
    try {
        const response = await fetch(`${API_BASE_URL}/Reports/dashboard`);
        if (!response.ok) throw new Error("API'ye ulaşılamadı");

        const data = await response.json();
        updateUI(data);
    } catch (error) {
        console.error(error);
    }
}

// ui güncelleme
function updateUI(data) {
    document.getElementById("totalCustomers").innerText = data.totalCustomers;
    document.getElementById("highRiskCustomers").innerText = data.highRiskCustomers;
    document.getElementById("avgScore").innerText = "%" + (data.averageChurnScore * 100).toFixed(0);

    const tbody = document.querySelector("#predictionTable tbody");
    tbody.innerHTML = "";

    data.recentPredictions.forEach(pred => {
        const badgeClass = pred.riskLevel === 'High' ? 'badge-risk-high' :
            pred.riskLevel === 'Medium' ? 'badge-risk-medium' : 'badge-risk-low';

        const riskLabel = pred.riskLevel === 'High' ? 'YÜKSEK' :
            pred.riskLevel === 'Medium' ? 'ORTA' : 'DÜŞÜK';

        const reasonText = pred.mainReason ? pred.mainReason : 'Belirtilmemiş';

        const row = `
                    <tr>
                        <td class="fw-bold">${pred.customerName || 'Müşteri ' + pred.customerId}</td>
                        <td>${(pred.churnScore).toFixed(2)}</td>
                        <td><span class="badge ${badgeClass} p-2">${riskLabel}</span></td>
                        <td class="text-muted small">${pred.recommendedAction}</td>
                        <td class="text-muted small"> ${reasonText}</td>
                    </tr>
                `;
        tbody.innerHTML += row;
    });

    updateChart(data.riskDistribution);
}

// grafik çizimi
function updateChart(distribution) {
    const ctx = document.getElementById('riskChart').getContext('2d');

    const low = distribution['Low'] || 0;
    const medium = distribution['Medium'] || 0;
    const high = distribution['High'] || 0;

    if (riskChart) riskChart.destroy();

    riskChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Düşük Risk', 'Orta Risk', 'Yüksek Risk'],
            datasets: [{
                data: [low, medium, high],
                backgroundColor: ['#1cc88a', '#f6c23e', '#e74a3b'],
                hoverOffset: 4
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'bottom',
                }
            }
        }
    });
}

// toplu analiz 
async function triggerAnalysis() {
    document.getElementById("loadingScreen").style.display = "flex";

    try {
        const response = await fetch(`${API_BASE_URL}/Analytics/analyze-all`, {
            method: 'POST'
        });

        if (response.ok) {
            const result = await response.json();
            document.getElementById("loadingScreen").style.display = "none";
            Swal.fire('Başarılı!', result.message, 'success');
            loadData();
        } else {
            throw new Error("Analiz hatası");
        }
    } catch (error) {
        document.getElementById("loadingScreen").style.display = "none";
        Swal.fire('Hata', 'Analiz başlatılamadı. Python servisi açık mı?', 'error');
    }
}

// customer verisi oluştur
async function triggerSeed() {
    Swal.fire({
        title: 'Veri Üretilsin mi?',
        text: "1000 adet rastgele müşteri eklenecek.",
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Evet, Üret',
        cancelButtonText: 'İptal'
    }).then(async (result) => {
        if (result.isConfirmed) {
            try {
                const response = await fetch(`${API_BASE_URL}/Seed/generate-fake-data`, { method: 'POST' });
                if (response.ok) {
                    Swal.fire('Tamamlandı', 'Veriler üretildi. Şimdi analiz yapabilirsiniz.', 'success');
                    loadData();
                }
            } catch (e) {
                Swal.fire('Hata', 'Seed işlemi başarısız', 'error');
            }
        }
    });
}

async function downloadRiskyReport() {
    try {
        Swal.fire({
            title: 'Rapor Hazırlanıyor...',
            text: 'Veriler analiz edilip Excel formatına çevriliyor.',
            allowOutsideClick: false,
            didOpen: () => Swal.showLoading()
        });

        const response = await fetch(`${API_BASE_URL}/Reports/export-risky-customers`);

        if (!response.ok) {
            throw new Error("Sunucu hatası: Rapor oluşturulamadı.");
        }

        const data = await response.json();

        if (!data || data.length === 0) {
            Swal.fire('Bilgi', 'Şu an riskli kategoride müşteri bulunmamaktadır.', 'info');
            return;
        }

        let csvContent = "\uFEFF";
        csvContent += "Musteri Adi;Email;Telefon;Risk Skoru;Risk Seviyesi;AI Onerisi\n"; 

        data.forEach(row => {
            const cleanAction = row.recommendedAction ? row.recommendedAction.replace(/;/g, " -") : "";

            const line = `${row.customerName};${row.email};${row.phone};${row.churnScore};${row.riskLevel};${cleanAction}`;
            csvContent += line + "\n";
        });

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);

        const link = document.createElement("a");
        link.setAttribute("href", url);
        link.setAttribute("download", `Riskli_Musteriler_${new Date().toLocaleDateString()}.csv`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        Swal.fire('Başarılı', 'Rapor bilgisayarınıza indirildi.', 'success');

    } catch (error) {
        console.error(error);
        Swal.fire('Hata', 'Rapor indirilirken bir sorun oluştu. API çalışıyor mu?', 'error');
    }
}
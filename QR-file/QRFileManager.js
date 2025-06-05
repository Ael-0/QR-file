class QRFileManager {
    constructor() {
        this.fileInput = document.getElementById('fileInput');
        this.fileLabel = document.getElementById('fileLabel');
        this.uploadForm = document.getElementById('uploadForm');
        this.uploadBtn = document.getElementById('uploadBtn');
        this.filesContainer = document.getElementById('filesContainer');

        this.init();
    }

    init() {
        this.setupEventListeners();
        this.loadFiles();
    }

    setupEventListeners() {
        this.uploadForm.addEventListener('submit', (e) => this.handleUpload(e));
        this.fileInput.addEventListener('change', (e) => this.handleFileSelect(e));

        this.fileLabel.addEventListener('dragover', (e) => {
            e.preventDefault();
            this.fileLabel.classList.add('dragover');
        });

        this.fileLabel.addEventListener('dragleave', () => {
            this.fileLabel.classList.remove('dragover');
        });

        this.fileLabel.addEventListener('drop', (e) => {
            e.preventDefault();
            this.fileLabel.classList.remove('dragover');
            const files = e.dataTransfer.files;
            if (files.length > 0) {
                this.fileInput.files = files;
                this.handleFileSelect({ target: { files } });
            }
        });
    }

    handleFileSelect(e) {
        const file = e.target.files[0];
        if (file) {
            this.fileLabel.innerHTML = `📄 ${file.name}<br><small>${this.formatFileSize(file.size)}</small>`;
        } else {
            this.fileLabel.innerHTML = '🗃️ Оберіть файл або перетягніть сюди';
        }
    }

    async handleUpload(e) {
        e.preventDefault();

        const file = this.fileInput.files[0];
        if (!file) {
            this.showNotification('Оберіть файл для завантаження!', 'error');
            return;
        }

        this.uploadBtn.disabled = true;
        this.uploadBtn.textContent = '⏳ Завантаження...';

        const formData = new FormData();
        formData.append('file', file);

        try {
            const response = await fetch('/api/files/upload', {
                method: 'POST',
                body: formData
            });

            if (!response.ok) throw new Error('Помилка під час завантаження');

            this.showNotification('✅ Файл завантажено успішно', 'success');
            this.fileInput.value = '';
            this.fileLabel.innerHTML = '🗃️ Оберіть файл або перетягніть сюди';
            this.loadFiles();
        } catch (error) {
            console.error(error);
            this.showNotification('❌ Помилка при завантаженні', 'error');
        } finally {
            this.uploadBtn.disabled = false;
            this.uploadBtn.textContent = '⬆️ Завантажити файл';
        }
    }

    async loadFiles() {
        try {
            const response = await fetch('/api/files');
            const files = await response.json();

            if (files.length === 0) {
                this.filesContainer.innerHTML = '<div class="empty-state">Файлів ще немає</div>';
                return;
            }

            const grid = document.createElement('div');
            grid.className = 'files-grid';

            files.forEach(file => {
                const card = document.createElement('div');
                card.className = 'file-card';

                card.innerHTML = `
                    <div class="file-info">
                        <h3>${file.originalName}</h3>
                        <div class="file-meta">
                             ${this.formatFileSize(file.fileSize)}<br>
                             ${new Date(file.uploadDate).toLocaleString()}
                        </div>
                    </div>
                    <div class="qr-code">
                        <img src="${file.qrCodePath}" alt="QR Code">
                        <div class="qr-code-label">Скануй для доступу</div>
                    </div>
                    <div class="file-actions">
                        <a href="/api/files/download/${file.id}" class="btn btn-primary">⬇️ Завантажити</a>
                        <button class="btn btn-danger" onclick="deleteFile('${file.id}')">🗑️ Видалити</button>
                    </div>
                `;

                grid.appendChild(card);
            });

            this.filesContainer.innerHTML = '';
            this.filesContainer.appendChild(grid);
        } catch (error) {
            console.error(error);
            this.filesContainer.innerHTML = '<div class="empty-state">Не вдалося завантажити файли</div>';
        }
    }

    formatFileSize(size) {
        const i = Math.floor(Math.log(size) / Math.log(1024));
        return (size / Math.pow(1024, i)).toFixed(2) * 1 + ' ' + ['Б', 'КБ', 'МБ', 'ГБ', 'ТБ'][i];
    }

    showNotification(message, type) {
        const notif = document.createElement('div');
        notif.className = `notification show ${type}`;
        notif.textContent = message;
        document.body.appendChild(notif);

        setTimeout(() => {
            notif.classList.remove('show');
            notif.addEventListener('transitionend', () => notif.remove());
        }, 3000);
    }
}

const formFile = document.getElementById('formFile');
const fileIcon = document.getElementById('fileIcon');
const warningMessage = document.getElementById('warningMessage');
const submitButton = document.getElementById('submitButton');
const successMessage = document.getElementById('successMessage');
const fileDetailsModal = document.getElementById('fileDetailsModal');
const fileDetailsText = document.getElementById('fileDetailsText');
const warningModal = document.getElementById('warningModal');

const fileTypeIcons = {
    'pdf': '/images/icons/pdf.png',
    'doc': '/images/icons/docs.png',
    'docx': '/images/icons/docs.png',
    'txt': '/images/icons/txtfile.png',
    'jpg': '/images/icons/camerafile.png',
    'jpeg': '/images/icons/camerafile.png',
    'png': '/images/icons/camerafile.png',
    'xls': '/images/icons/excelfile.png',
    'xlsx': '/images/icons/excelfile.png',
    'ppt': '/images/icons/pptfile.png',
    'pptx': '/images/icons/pptfile.png',
    'default': '/images/icons/uploadfile.png'
};

formFile.addEventListener('change', (event) => {
    const file = event.target.files[0];

    if (file) {
        const fileExtension = file.name.split('.').pop().toLowerCase();
        const iconPath = fileTypeIcons[fileExtension] || '/css/images/unsupported.png';

        fileIcon.src = iconPath;
    } else {
        resetFileInput();
    }
});

function resetFileInput() {
    fileIcon.src = '/css/images/uploadfile.png';
    fileDetailsText.textContent = '';
    warningMessage.style.display = 'none';
    submitButton.disabled = true;
}

function showWarningModal(message) {
    document.getElementById('modalWarningMessage').textContent = message;
    warningModal.style.display = 'flex';
}

document.getElementById('closeWarningModal').onclick = () => warningModal.style.display = 'none';
document.getElementById('closeWarningButton').onclick = () => warningModal.style.display = 'none';
document.getElementById('closeFileDetailsModal').onclick = () => fileDetailsModal.style.display = 'none';

submitButton.addEventListener('click', () => {
    const file = formFile.files[0];
    if (file) {
        document.getElementById('fileDetails').textContent = `Name: ${file.name} | Size: ${Math.round(file.size / 1024)} KB`;
        fileDetailsModal.style.display = 'flex';
        successMessage.style.display = 'none';
    } else {
        showWarningModal('Please select a valid file before submitting.');
    }
});

document.getElementById('nextButton').onclick = () => {
    successMessage.style.display = 'block';
    fileDetailsModal.style.display = 'none';
    resetFileInput();
};